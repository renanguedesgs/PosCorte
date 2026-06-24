using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PosCorte.API.Configuration;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Services.Pagamentos.Asaas;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services
{
    public class PagamentoService : IPagamentoService
    {
        private readonly ILogger<PagamentoService> _logger;
        private readonly PosCorteDbContext _db;
        private readonly AsaasOptions _asaas;
        private readonly IAsaasClient _asaasClient;
        private readonly IPrecificacaoService _precificacao;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;

        // Resolu��o tardia evita o ciclo PagamentoService <-> PagamentoConfirmacaoService.
        private IPagamentoConfirmacaoService Confirmacao => _serviceProvider.GetRequiredService<IPagamentoConfirmacaoService>();

        public PagamentoService(
            ILogger<PagamentoService> logger,
            PosCorteDbContext db,
            IOptions<AsaasOptions> asaas,
            IAsaasClient asaasClient,
            IPrecificacaoService precificacao,
            IServiceProvider serviceProvider,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _db = db;
            _asaas = asaas.Value;
            _asaasClient = asaasClient;
            _precificacao = precificacao;
            _serviceProvider = serviceProvider;
            _env = env;
        }

        public bool GatewayConfigurado => _asaas.EstaConfigurado;
        public bool ModoStub => !_asaas.EstaConfigurado;

        public async Task<GerarPixResponseDTO?> GerarPixAsync(int projetoId, int usuarioId)
        {
            var projeto = await _db.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return null;
            if (projeto.UsuarioId != usuarioId) return null;
            if (projeto.StatusProjeto != "Aguardando_Pagamento")
                throw new InvalidOperationException("Projeto n�o est� aguardando pagamento.");

            var pendente = await _db.Pagamentos
                .Where(p => p.ProjetoId == projetoId && p.Status == "Aguardando_Pix")
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (pendente != null && pendente.ExpiraEm > DateTime.UtcNow)
                return MapGerarPix(pendente);

            var orc = _precificacao.ProcessarProjeto(projeto.QtdPecas, projeto.QtdGavetas);

            if (_asaas.EstaConfigurado)
                return await GerarPixAsaasAsync(projeto, usuarioId, orc);

            return await GerarPixStubAsync(projeto, orc);
        }

        private async Task<GerarPixResponseDTO> GerarPixStubAsync(Projeto projeto, OrcamentoResultado orc)
        {
            var stubId = $"STUB-{Guid.NewGuid():N}";
            var pagamento = new Pagamento
            {
                ProjetoId = projeto.Id,
                Modo = "Stub",
                Status = "Aguardando_Pix",
                AsaasPaymentId = stubId,
                ValorTotal = orc.ValorTotal,
                ValorMarceneiro = orc.CustoPrestador,
                ValorPlataforma = orc.MargemLucro,
                PixCopiaECola = $"00020126STUB-DEV-{stubId}",
                ExpiraEm = DateTime.UtcNow.AddHours(24),
                DataCriacao = DateTime.UtcNow
            };

            _db.Pagamentos.Add(pagamento);
            await _db.SaveChangesAsync();

            _logger.LogWarning("PIX STUB gerado para projeto {ProjetoId}. Nenhum valor real ser� cobrado.", projeto.Id);

            var dto = MapGerarPix(pagamento);
            dto.Aviso = "Gateway Asaas n�o configurado. Cobran�a simulada � use 'Simular pagamento' apenas em desenvolvimento.";
            return dto;
        }

        private async Task<GerarPixResponseDTO> GerarPixAsaasAsync(Projeto projeto, int usuarioId, OrcamentoResultado orc)
        {
            var usuario = await _db.Usuarios.FindAsync(usuarioId)
                ?? throw new InvalidOperationException("Usu�rio n�o encontrado.");

            var customer = await _asaasClient.CriarOuObterClienteAsync(new AsaasCustomerRequest
            {
                Name = usuario.Nome,
                CpfCnpj = usuario.CpfCnpj,
                Email = usuario.Email,
                MobilePhone = usuario.Telefone,
                ExternalReference = $"usuario-{usuario.Id}"
            });

            var dueDate = DateTime.UtcNow.AddDays(_asaas.DiasVencimentoPix);
            var payment = await _asaasClient.CriarCobrancaPixAsync(new AsaasPaymentRequest
            {
                Customer = customer.Id,
                BillingType = "PIX",
                Value = orc.ValorTotal,
                DueDate = dueDate,
                Description = $"Montagem P�sCorte � {projeto.NomeProjeto}",
                ExternalReference = projeto.Id.ToString()
            });

            var qr = await _asaasClient.ObterQrCodePixAsync(payment.Id);

            var pagamento = new Pagamento
            {
                ProjetoId = projeto.Id,
                Modo = "Asaas",
                Status = "Aguardando_Pix",
                AsaasPaymentId = payment.Id,
                AsaasCustomerId = customer.Id,
                ValorTotal = orc.ValorTotal,
                ValorMarceneiro = orc.CustoPrestador,
                ValorPlataforma = orc.MargemLucro,
                PixCopiaECola = qr?.Payload,
                QrCodeBase64 = qr?.EncodedImage,
                InvoiceUrl = payment.InvoiceUrl,
                ExpiraEm = ParseExpiration(qr?.ExpirationDate) ?? dueDate,
                DataCriacao = DateTime.UtcNow
            };

            _db.Pagamentos.Add(pagamento);
            await _db.SaveChangesAsync();

            return MapGerarPix(pagamento);
        }

        public async Task<PagamentoStatusDTO?> ObterStatusPagamentoAsync(int projetoId)
        {
            var pagamento = await _db.Pagamentos
                .Where(p => p.ProjetoId == projetoId)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            var projeto = await _db.Projetos.FindAsync(projetoId);
            if (pagamento == null || projeto == null) return null;

            if (_asaas.EstaConfigurado && pagamento.Modo == "Asaas" &&
                pagamento.Status == "Aguardando_Pix" && !string.IsNullOrEmpty(pagamento.AsaasPaymentId))
            {
                await SincronizarStatusAsaasAsync(pagamento);
            }

            return new PagamentoStatusDTO
            {
                PagamentoId = pagamento.Id,
                ProjetoId = projetoId,
                Status = pagamento.Status,
                Modo = pagamento.Modo,
                ValorTotal = pagamento.ValorTotal,
                DataConfirmacao = pagamento.DataConfirmacao,
                StatusProjeto = projeto.StatusProjeto
            };
        }

        public async Task<bool> SimularConfirmacaoStubAsync(int pagamentoId, int usuarioId)
        {
            if (!_env.IsDevelopment())
            {
                _logger.LogWarning("Simula��o de pagamento bloqueada fora de Development.");
                return false;
            }

            var pagamento = await _db.Pagamentos.FirstOrDefaultAsync(p => p.Id == pagamentoId);

            if (pagamento == null || pagamento.Modo != "Stub") return false;

            var projeto = await _db.Projetos.FindAsync(pagamento.ProjetoId);
            if (projeto == null || projeto.UsuarioId != usuarioId) return false;

            pagamento.Status = "Confirmado";
            pagamento.DataConfirmacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return await Confirmacao.ConfirmarPagamentoAsync(
                pagamento.ProjetoId,
                pagamento.AsaasPaymentId ?? $"STUB-{pagamento.Id}",
                pagamento.ValorTotal);
        }

        public async Task<bool> ProcessarWebhookAsaasAsync(AsaasWebhookPayload payload)
        {
            if (payload.Payment == null) return false;

            var evento = payload.Event?.ToUpperInvariant();
            if (evento is not ("PAYMENT_RECEIVED" or "PAYMENT_CONFIRMED"))
                return false;

            var pagamento = await _db.Pagamentos
                .FirstOrDefaultAsync(p => p.AsaasPaymentId == payload.Payment.Id);

            if (pagamento == null && int.TryParse(payload.Payment.ExternalReference, out var projetoId))
            {
                pagamento = await _db.Pagamentos
                    .Where(p => p.ProjetoId == projetoId)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();
            }

            if (pagamento == null)
            {
                _logger.LogWarning("Webhook Asaas: pagamento {Id} n�o encontrado", payload.Payment.Id);
                return false;
            }

            pagamento.Status = "Confirmado";
            pagamento.DataConfirmacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return await Confirmacao.ConfirmarPagamentoAsync(
                pagamento.ProjetoId,
                pagamento.AsaasPaymentId ?? payload.Payment.Id,
                pagamento.ValorTotal);
        }

        public async Task<bool> ValidarPagamentoPixAsync(string pixId, decimal valorEsperado)
        {
            var pagamento = await _db.Pagamentos.FirstOrDefaultAsync(p => p.AsaasPaymentId == pixId);
            if (pagamento == null)
            {
                _logger.LogWarning("Validar PIX: id {PixId} n�o encontrado", pixId);
                return false;
            }

            if (Math.Abs(pagamento.ValorTotal - valorEsperado) > 0.01m)
            {
                _logger.LogWarning("Valor PIX divergente. Esperado {E}, registro {R}", valorEsperado, pagamento.ValorTotal);
                return false;
            }

            if (pagamento.Modo == "Stub")
                return pagamento.Status is "Confirmado" or "Retido_Escrow";

            if (!_asaas.EstaConfigurado) return false;

            var cobranca = await _asaasClient.ObterCobrancaAsync(pixId);
            if (cobranca == null) return false;

            return cobranca.Status is "RECEIVED" or "CONFIRMED";
        }

        public async Task<bool> ReservarFundosAsync(string pixId, decimal valor)
        {
            var pagamento = await _db.Pagamentos.FirstOrDefaultAsync(p => p.AsaasPaymentId == pixId);
            if (pagamento == null) return false;

            pagamento.Status = "Retido_Escrow";
            await _db.SaveChangesAsync();

            _logger.LogInformation("Escrow: fundos retidos para {PixId}, R$ {Valor}", pixId, valor);
            return true;
        }

        public async Task<bool> LiquidarFundosAsync(string pixId, decimal valor)
        {
            var pagamento = await _db.Pagamentos.FirstOrDefaultAsync(p => p.AsaasPaymentId == pixId);
            if (pagamento == null) return false;

            var liquidacao = new Liquidacao
            {
                PagamentoId = pagamento.Id,
                ProjetoId = pagamento.ProjetoId,
                ValorMarceneiro = pagamento.ValorMarceneiro,
                ValorPlataforma = pagamento.ValorPlataforma,
                Status = _asaas.EstaConfigurado ? "Pendente" : "Concluida",
                DataConclusao = _asaas.EstaConfigurado ? null : DateTime.UtcNow
            };

            // TODO: quando Asaas configurado, chamar API de split/transfer�ncia aqui.
            if (_asaas.EstaConfigurado)
                _logger.LogInformation("Liquida��o registrada (split Asaas pendente de implementa��o) para {PixId}", pixId);

            pagamento.Status = "Liquidado";
            _db.Liquidacoes.Add(liquidacao);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LiquidarPorProjetoAsync(int projetoId)
        {
            var pagamento = await _db.Pagamentos
                .Where(p => p.ProjetoId == projetoId &&
                            (p.Status == "Retido_Escrow" || p.Status == "Confirmado"))
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (pagamento == null)
            {
                _logger.LogWarning("Liquidar projeto {ProjetoId}: nenhum pagamento retido encontrado", projetoId);
                return false;
            }

            var liquidacao = new Liquidacao
            {
                PagamentoId = pagamento.Id,
                ProjetoId = pagamento.ProjetoId,
                ValorMarceneiro = pagamento.ValorMarceneiro,
                ValorPlataforma = pagamento.ValorPlataforma,
                Status = _asaas.EstaConfigurado ? "Pendente" : "Concluida",
                DataConclusao = _asaas.EstaConfigurado ? null : DateTime.UtcNow
            };

            // TODO: quando Asaas configurado, chamar API de split/transferencia (80% marceneiro / 20% plataforma) aqui.
            if (_asaas.EstaConfigurado)
                _logger.LogInformation("Liquidacao registrada (split Asaas pendente de implementacao) para projeto {ProjetoId}", projetoId);

            pagamento.Status = "Liquidado";
            _db.Liquidacoes.Add(liquidacao);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Escrow liquidado para projeto {ProjetoId}: marceneiro R$ {M}, plataforma R$ {P}",
                projetoId, pagamento.ValorMarceneiro, pagamento.ValorPlataforma);
            return true;
        }

        private async Task SincronizarStatusAsaasAsync(Pagamento pagamento)
        {
            if (string.IsNullOrEmpty(pagamento.AsaasPaymentId)) return;

            var cobranca = await _asaasClient.ObterCobrancaAsync(pagamento.AsaasPaymentId);
            if (cobranca?.Status is "RECEIVED" or "CONFIRMED")
            {
                pagamento.Status = "Confirmado";
                pagamento.DataConfirmacao = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await Confirmacao.ConfirmarPagamentoAsync(
                    pagamento.ProjetoId,
                    pagamento.AsaasPaymentId,
                    pagamento.ValorTotal);
            }
        }

        private static GerarPixResponseDTO MapGerarPix(Pagamento p) => new()
        {
            PagamentoId = p.Id,
            ProjetoId = p.ProjetoId,
            Modo = p.Modo,
            Status = p.Status,
            ValorTotal = p.ValorTotal,
            ValorMarceneiro = p.ValorMarceneiro,
            ValorPlataforma = p.ValorPlataforma,
            PixCopiaECola = p.PixCopiaECola,
            QrCodeBase64 = p.QrCodeBase64,
            InvoiceUrl = p.InvoiceUrl,
            ExpiraEm = p.ExpiraEm,
            GatewayConfigurado = p.Modo == "Asaas"
        };

        private static DateTime? ParseExpiration(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return DateTime.TryParse(raw, out var dt) ? dt.ToUniversalTime() : null;
        }
    }
}
