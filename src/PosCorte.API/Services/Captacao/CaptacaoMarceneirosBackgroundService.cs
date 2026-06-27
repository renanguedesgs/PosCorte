using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosCorte.API.Configuration;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services.Captacao
{
    /// <summary>
    /// Robô de captação: a cada ciclo busca montadores de móveis planejados no
    /// Google Places (por cidade × termo), cadastra como leads pendentes
    /// (Verificado=false, Disponivel=false), deduplica por OrigemExterna="places:{place_id}"
    /// e dispara um convite de auto-cadastro. Só roda quando <c>Captacao</c> está configurada.
    /// </summary>
    public class CaptacaoMarceneirosBackgroundService : BackgroundService
    {
        public const string HttpClientName = "captacao";

        private readonly IServiceProvider _services;
        private readonly IOptions<CaptacaoOptions> _options;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<CaptacaoMarceneirosBackgroundService> _logger;

        public CaptacaoMarceneirosBackgroundService(
            IServiceProvider services,
            IOptions<CaptacaoOptions> options,
            IHttpClientFactory httpFactory,
            ILogger<CaptacaoMarceneirosBackgroundService> logger)
        {
            _services = services;
            _options = options;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var opt = _options.Value;
            if (!opt.EstaConfigurado)
            {
                _logger.LogInformation("Captação automática DESLIGADA (preencha Captacao:Enabled + GooglePlacesApiKey para ativar).");
                return;
            }

            _logger.LogInformation("CaptacaoMarceneirosBackgroundService iniciado (intervalo {Horas}h).", opt.IntervaloHoras);

            try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecutarCicloAsync(opt, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha no ciclo de captação automática");
                }

                try { await Task.Delay(TimeSpan.FromHours(Math.Max(1, opt.IntervaloHoras)), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task ExecutarCicloAsync(CaptacaoOptions opt, CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PosCorteDbContext>();
            var notificacao = scope.ServiceProvider.GetRequiredService<INotificacaoService>();
            var notifOpt = scope.ServiceProvider.GetRequiredService<IOptions<NotificacaoOptions>>().Value;
            var client = _httpFactory.CreateClient(HttpClientName);

            var linkOnboarding = $"{notifOpt.AppBaseUrl.TrimEnd('/')}/Marceneiros/Seja";
            var inseridos = 0;

            foreach (var cidade in opt.Cidades)
            {
                foreach (var termo in opt.TermosBusca)
                {
                    if (ct.IsCancellationRequested || inseridos >= opt.MaxLeadsPorCiclo) return;

                    var candidatos = await BuscarNoPlacesAsync(client, opt.GooglePlacesApiKey, $"{termo} em {cidade}", ct);

                    foreach (var c in candidatos)
                    {
                        if (inseridos >= opt.MaxLeadsPorCiclo) break;

                        var origem = $"places:{c.PlaceId}";
                        if (await db.Marceneiros.AnyAsync(m => m.OrigemExterna == origem, ct))
                            continue;

                        var telefone = await ObterTelefoneAsync(client, opt.GooglePlacesApiKey, c.PlaceId, ct);

                        var (cidadeNome, uf) = SepararCidadeUf(cidade);
                        var lead = new Marceneiro
                        {
                            Nome = c.Nome,
                            Telefone = telefone,
                            Cidade = cidadeNome,
                            Estado = uf,
                            Especialidades = "Montagem de planejados",
                            Bio = c.Endereco,
                            Verificado = false,
                            Disponivel = false,
                            OrigemExterna = origem,
                            DataCadastro = DateTime.UtcNow
                        };

                        db.Marceneiros.Add(lead);
                        await db.SaveChangesAsync(ct);
                        inseridos++;

                        _logger.LogInformation("Lead captado: {Nome} ({Cidade}) origem {Origem}", lead.Nome, lead.Cidade, origem);

                        if (opt.EnviarConvite && telefone.Length >= 10)
                        {
                            var convite =
                                $"Olá! Aqui é da PósCorte. Conectamos montadores de móveis planejados a arquitetos, " +
                                $"com pagamento garantido (escrow). Quer receber montagens na região de {cidadeNome}? " +
                                $"Cadastre-se em 1 minuto: {linkOnboarding}";
                            await notificacao.NotificarMontador(telefone, convite);
                        }
                    }
                }
            }

            _logger.LogInformation("Ciclo de captação concluído. {Inseridos} novos leads.", inseridos);
        }

        private async Task<List<PlaceCandidato>> BuscarNoPlacesAsync(HttpClient client, string apiKey, string query, CancellationToken ct)
        {
            var lista = new List<PlaceCandidato>();
            try
            {
                var url = "https://maps.googleapis.com/maps/api/place/textsearch/json" +
                          $"?query={Uri.EscapeDataString(query)}&region=br&language=pt-BR&key={apiKey}";

                var json = await client.GetStringAsync(url, ct);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("results", out var results))
                    return lista;

                foreach (var r in results.EnumerateArray())
                {
                    var placeId = r.TryGetProperty("place_id", out var pid) ? pid.GetString() : null;
                    var nome = r.TryGetProperty("name", out var n) ? n.GetString() : null;
                    var endereco = r.TryGetProperty("formatted_address", out var a) ? a.GetString() : string.Empty;

                    if (!string.IsNullOrWhiteSpace(placeId) && !string.IsNullOrWhiteSpace(nome))
                        lista.Add(new PlaceCandidato(placeId!, nome!, endereco ?? string.Empty));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Busca no Places falhou para '{Query}'", query);
            }
            return lista;
        }

        private async Task<string> ObterTelefoneAsync(HttpClient client, string apiKey, string placeId, CancellationToken ct)
        {
            try
            {
                var url = "https://maps.googleapis.com/maps/api/place/details/json" +
                          $"?place_id={Uri.EscapeDataString(placeId)}&fields=international_phone_number,formatted_phone_number&language=pt-BR&key={apiKey}";

                var json = await client.GetStringAsync(url, ct);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("result", out var result))
                {
                    var fone = result.TryGetProperty("international_phone_number", out var ip) ? ip.GetString()
                             : result.TryGetProperty("formatted_phone_number", out var fp) ? fp.GetString()
                             : null;

                    if (!string.IsNullOrWhiteSpace(fone))
                        return new string(fone.Where(char.IsDigit).ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Place Details falhou para {PlaceId}", placeId);
            }
            return string.Empty;
        }

        private static (string cidade, string uf) SepararCidadeUf(string cidadeConfig)
        {
            var partes = cidadeConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (partes.Length >= 2) return (partes[0], partes[1].ToUpperInvariant());
            return (cidadeConfig.Trim(), "SP");
        }

        private record PlaceCandidato(string PlaceId, string Nome, string Endereco);
    }
}
