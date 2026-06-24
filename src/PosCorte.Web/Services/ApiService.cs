using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PosCorte.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        private void SetAuthHeader()
        {
            var token = _ctx.HttpContext?.Session.GetString("jwt");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // ?? AUTH ????????????????????????????????????????????????
        public async Task<(bool ok, string? token, string? erro)> LoginAsync(string email, string senha)
        {
            var body = JsonSerializer.Serialize(new { email, senha });
            var res = await _http.PostAsync("api/v1/auth/login",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!res.IsSuccessStatusCode)
                return (false, null, "Email ou senha inv�lidos");

            var json = await res.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            return (true, obj.GetProperty("token").GetString(), null);
        }

        public async Task<(bool ok, string? erro)> RegisterAsync(string nome, string email, string cpfCnpj, string telefone, string senha)
        {
            var body = JsonSerializer.Serialize(new { nome, email, cpfCnpj, telefone, senha });
            var res = await _http.PostAsync("api/v1/auth/register",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                return (false, "Email j� cadastrado");
            }
            return (true, null);
        }

        // ?? USU�RIOS ?????????????????????????????????????????????
        public async Task<List<UsuarioViewModel>> ListarUsuariosAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/usuarios");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<UsuarioViewModel>>(json, _json) ?? new();
        }

        // ?? PROJETOS ?????????????????????????????????????????????
        public async Task<List<ProjetoViewModel>> ListarProjetosAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/projetos");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProjetoViewModel>>(json, _json) ?? new();
        }

        public async Task<ProjetoViewModel?> ObterProjetoAsync(int id)
        {
            SetAuthHeader();
            var res = await _http.GetAsync($"api/v1/projetos/{id}");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProjetoViewModel>(json, _json);
        }

        public async Task<(bool ok, string? erro)> CriarProjetoAsync(CriarProjetoInput input)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(input);
            var res = await _http.PostAsync("api/v1/projetos",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<OrcamentoViewModel?> CalcularOrcamentoAsync(int projetoId, int pecas, int gavetas)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(new { qtdPecas = pecas, qtdGavetas = gavetas });
            var res = await _http.PostAsync($"api/v1/projetos/{projetoId}/calcular-orcamento",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OrcamentoViewModel>(json, _json);
        }

        public async Task<GerarPixViewModel?> GerarPixAsync(int projetoId)
        {
            SetAuthHeader();
            var res = await _http.PostAsync($"api/v1/projetos/{projetoId}/gerar-pix", null);
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GerarPixViewModel>(json, _json);
        }

        public async Task<PagamentoStatusViewModel?> ObterStatusPagamentoAsync(int projetoId)
        {
            SetAuthHeader();
            var res = await _http.GetAsync($"api/v1/projetos/{projetoId}/pagamento");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PagamentoStatusViewModel>(json, _json);
        }

        public async Task<(bool ok, string? erro)> SimularPagamentoAsync(int pagamentoId)
        {
            SetAuthHeader();
            var res = await _http.PostAsync($"api/v1/pagamentos/{pagamentoId}/simular-confirmacao", null);
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<(bool ok, string? erro)> AprovarMontagemAsync(int projetoId)
        {
            SetAuthHeader();
            var res = await _http.PostAsync($"api/v1/projetos/{projetoId}/aprovar-montagem", null);
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<(bool ok, string? erro)> AbrirDisputaAsync(int projetoId, string motivo)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(new { motivo });
            var res = await _http.PostAsync($"api/v1/projetos/{projetoId}/abrir-disputa",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<(bool ok, string? erro)> SimularConclusaoMontagemAsync(int projetoId)
        {
            SetAuthHeader();
            var res = await _http.PostAsync($"api/v1/projetos/{projetoId}/simular-conclusao-montagem", null);
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        // ?? ORDENS ???????????????????????????????????????????????
        public async Task<List<OrdemViewModel>> ListarOrdensAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/ordens-servico");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<OrdemViewModel>>(json, _json) ?? new();
        }

        // ?? MARCENEIROS ??????????????????????????????????????????
        public async Task<List<MarceneiroViewModel>> ListarMarceneirosAsync(string? cidade = null, string? especialidade = null, decimal? notaMin = null, bool? disponivel = null)
        {
            SetAuthHeader();
            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(cidade)) qs.Add($"cidade={Uri.EscapeDataString(cidade)}");
            if (!string.IsNullOrWhiteSpace(especialidade)) qs.Add($"especialidade={Uri.EscapeDataString(especialidade)}");
            if (notaMin.HasValue) qs.Add($"notaMin={notaMin.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            if (disponivel.HasValue) qs.Add($"disponivel={disponivel.Value.ToString().ToLowerInvariant()}");
            var query = qs.Count > 0 ? "?" + string.Join("&", qs) : "";

            var res = await _http.GetAsync($"api/v1/marceneiros{query}");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<MarceneiroViewModel>>(json, _json) ?? new();
        }

        public async Task<MarceneiroDetalheViewModel?> ObterMarceneiroAsync(int id)
        {
            SetAuthHeader();
            var res = await _http.GetAsync($"api/v1/marceneiros/{id}");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MarceneiroDetalheViewModel>(json, _json);
        }

        public async Task<(bool ok, string? erro)> AvaliarMarceneiroAsync(int id, int nota, string comentario, int? projetoId = null)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(new { nota, comentario, projetoId });
            var res = await _http.PostAsync($"api/v1/marceneiros/{id}/avaliacoes",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        // ?? ADMIN ?????????????????????????????????????????????????
        public async Task<AdminDashboardViewModel?> ObterAdminDashboardAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/admin/dashboard");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AdminDashboardViewModel>(json, _json);
        }

        public async Task<List<ArquitetoAdminViewModel>> ListarArquitetosAdminAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/admin/arquitetos");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ArquitetoAdminViewModel>>(json, _json) ?? new();
        }

        public async Task<(bool ok, string? erro, CreateArquitetoAdminResponseViewModel? data)> CadastrarArquitetoAdminAsync(CreateArquitetoAdminInput input)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(input);
            var res = await _http.PostAsync("api/v1/admin/arquitetos",
                new StringContent(body, Encoding.UTF8, "application/json"));
            var json = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
                return (false, json, null);
            return (true, null, JsonSerializer.Deserialize<CreateArquitetoAdminResponseViewModel>(json, _json));
        }

        public async Task<(bool ok, string? erro)> CadastrarMarceneiroAdminAsync(CreateMarceneiroAdminInput input)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(input);
            var res = await _http.PostAsync("api/v1/admin/marceneiros",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<ProjetoOperacaoAdminViewModel?> ObterProjetoOperacaoAdminAsync(int projetoId)
        {
            SetAuthHeader();
            var res = await _http.GetAsync($"api/v1/admin/projetos/{projetoId}/operacao");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProjetoOperacaoAdminViewModel>(json, _json);
        }

        public async Task<(bool ok, string? erro)> AlocarMontadorAdminAsync(int projetoId, int marceneiroId, DateTime? dataAgendamento = null)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(new { marceneiroId, dataAgendamento });
            var res = await _http.PostAsync($"api/v1/admin/projetos/{projetoId}/alocar-montador",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<(bool ok, string? erro)> MarcarMontagemConcluidaAdminAsync(int projetoId)
        {
            SetAuthHeader();
            var res = await _http.PostAsync($"api/v1/admin/projetos/{projetoId}/marcar-montagem-concluida", null);
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<(bool ok, string? erro)> AlterarSenhaAdminAsync(string senhaAtual, string senhaNova)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(new { senhaAtual, senhaNova });
            var res = await _http.PostAsync("api/v1/admin/conta/alterar-senha",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }
    }

    // ?? VIEW MODELS ???????????????????????????????????????????
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }

    public class ProjetoViewModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string UrlArquivoCorteCloud { get; set; } = string.Empty;
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
        public string StatusProjeto { get; set; } = string.Empty;
        public DateTime? DataLimiteVistoria { get; set; }
        public string? MotivoDisputa { get; set; }
    }

    public class CriarProjetoInput
    {
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string UrlArquivoCorteCloud { get; set; } = string.Empty;
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
    }

    public class OrcamentoViewModel
    {
        public decimal ValorTotal { get; set; }
        public decimal CustoPrestador { get; set; }
        public decimal MargemLucro { get; set; }
        public decimal TaxaPlataforma { get; set; }
    }

    public class GerarPixViewModel
    {
        public int PagamentoId { get; set; }
        public int ProjetoId { get; set; }
        public string Modo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public decimal ValorMarceneiro { get; set; }
        public decimal ValorPlataforma { get; set; }
        public string? PixCopiaECola { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? InvoiceUrl { get; set; }
        public DateTime? ExpiraEm { get; set; }
        public bool GatewayConfigurado { get; set; }
        public string? Aviso { get; set; }
    }

    public class PagamentoStatusViewModel
    {
        public int PagamentoId { get; set; }
        public int ProjetoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Modo { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public string StatusProjeto { get; set; } = string.Empty;
    }

    public class OrdemViewModel
    {
        public int Id { get; set; }
        public int ProjetoId { get; set; }
        public string ExternalProviderId { get; set; } = string.Empty;
        public string StatusProvedor { get; set; } = string.Empty;
        public string MontadorNome { get; set; } = string.Empty;
        public string MontadorTelefone { get; set; } = string.Empty;
        public DateTime DataAtualizacao { get; set; }
    }

    public class MarceneiroViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public List<string> Especialidades { get; set; } = new();
        public string Bio { get; set; } = string.Empty;
        public decimal NotaMedia { get; set; }
        public int TotalAvaliacoes { get; set; }
        public int TotalServicos { get; set; }
        public bool Disponivel { get; set; }
        public bool Verificado { get; set; }
    }

    public class MarceneiroDetalheViewModel : MarceneiroViewModel
    {
        public List<AvaliacaoViewModel> Avaliacoes { get; set; } = new();
    }

    public class AvaliacaoViewModel
    {
        public int Id { get; set; }
        public int MarceneiroId { get; set; }
        public int? ProjetoId { get; set; }
        public string AutorNome { get; set; } = string.Empty;
        public int Nota { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalArquitetos { get; set; }
        public int TotalMarceneiros { get; set; }
        public int TotalProjetos { get; set; }
        public int TotalOrdens { get; set; }
        public decimal ReceitaPlataformaEstimada { get; set; }
        public decimal VolumeTransacionadoEstimado { get; set; }
        public string GatewayPagamento { get; set; } = string.Empty;
        public string StatusEscrow { get; set; } = string.Empty;
        public Dictionary<string, int> ProjetosPorStatus { get; set; } = new();
        public List<ProjetoResumoAdminViewModel> ProjetosRecentes { get; set; } = new();
    }

    public class ProjetoResumoAdminViewModel
    {
        public int Id { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string StatusProjeto { get; set; } = string.Empty;
        public string ArquitetoNome { get; set; } = string.Empty;
        public decimal ValorEstimado { get; set; }
        public decimal MargemEstimada { get; set; }
        public string? MontadorNome { get; set; }
    }

    public class ArquitetoAdminViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }

    public class CreateArquitetoAdminInput
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string? Senha { get; set; }
    }

    public class CreateArquitetoAdminResponseViewModel
    {
        public ArquitetoAdminViewModel Arquiteto { get; set; } = new();
        public string SenhaInicial { get; set; } = string.Empty;
    }

    public class CreateMarceneiroAdminInput
    {
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = "SP";
        public string Bairro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Especialidades { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public bool Verificado { get; set; } = true;
        public bool Disponivel { get; set; } = true;
    }

    public class ProjetoOperacaoAdminViewModel
    {
        public ProjetoViewModel Projeto { get; set; } = new();
        public string ArquitetoNome { get; set; } = string.Empty;
        public string ArquitetoEmail { get; set; } = string.Empty;
        public string ArquitetoTelefone { get; set; } = string.Empty;
        public OrcamentoViewModel? Orcamento { get; set; }
        public OrdemOperacaoAdminViewModel? Ordem { get; set; }
        public PagamentoResumoAdminViewModel? Pagamento { get; set; }
    }

    public class OrdemOperacaoAdminViewModel
    {
        public int Id { get; set; }
        public string StatusProvedor { get; set; } = string.Empty;
        public string MontadorNome { get; set; } = string.Empty;
        public string MontadorTelefone { get; set; } = string.Empty;
        public DateTime DataAgendamento { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }

    public class PagamentoResumoAdminViewModel
    {
        public string Status { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public decimal ValorMarceneiro { get; set; }
    }
}
