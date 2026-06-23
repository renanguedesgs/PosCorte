using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/webhooks")]
    [Produces("application/json")]
    public class WebhookPoscorteController : ControllerBase
    {
        private readonly ILogger<WebhookPoscorteController> _logger;
        private readonly IRepositorio<Projeto> _projetoRepo;
        private readonly IRepositorio<OrdemServico> _ordemRepo;
        private readonly IPagamentoService _pagamentoService;
        private readonly IPagamentoConfirmacaoService _confirmacao;
        private readonly IWebHostEnvironment _env;

        public WebhookPoscorteController(
            ILogger<WebhookPoscorteController> logger,
            IRepositorio<Projeto> projetoRepo,
            IRepositorio<OrdemServico> ordemRepo,
            IPagamentoService pagamentoService,
            IPagamentoConfirmacaoService confirmacao,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _projetoRepo = projetoRepo;
            _ordemRepo = ordemRepo;
            _pagamentoService = pagamentoService;
            _confirmacao = confirmacao;
            _env = env;
        }

        /// <summary>
        /// Webhook legado/manual para desenvolvimento. Em producao use POST /webhooks/asaas.
        /// </summary>
        [HttpPost("pagamento-confirmado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TratarPagamentoConfirmado([FromBody] WebhookPagamento dados)
        {
            if (!_env.IsDevelopment() && _pagamentoService.GatewayConfigurado)
            {
                _logger.LogWarning("Webhook manual bloqueado: use Asaas em producao.");
                return BadRequest(new { error = "Use webhook Asaas em producao." });
            }

            _logger.LogInformation("Webhook pagamento manual: Projeto {ProjetoId}", dados.ProjetoId);

            var projeto = await _projetoRepo.GetByIdAsync(dados.ProjetoId);
            if (projeto == null)
                return NotFound(new { error = "Projeto nao encontrado" });

            if (dados.Status != "pago")
                return Ok(new { message = "Ignorado" });

            var ok = await _confirmacao.ConfirmarPagamentoAsync(dados.ProjetoId, dados.PixId, dados.Valor);
            if (!ok)
                return BadRequest(new { error = "Pagamento nao validado ou falha no fluxo" });

            return Ok(new { message = "Webhook de pagamento processado com sucesso" });
        }

        /// <summary>
        /// Webhook: Atualizacao de status do montador (provedor parceiro).
        /// </summary>
        [HttpPost("atualizacao-montador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TratarAtualizacaoMontador([FromBody] WebhookData dados)
        {
            _logger.LogInformation("Webhook montador: ID {ExternalId}, Status: {Status}", dados.IdExternalProviderId, dados.Status);

            var ordensServico = await _ordemRepo.GetAllAsync();
            var ordem = ordensServico.FirstOrDefault(o => o.ExternalProviderId == dados.IdExternalProviderId);

            if (ordem == null)
                return NotFound(new { error = "Ordem nao encontrada" });

            var projeto = await _projetoRepo.GetByIdAsync(ordem.ProjetoId);
            if (projeto == null)
                return NotFound(new { error = "Projeto nao encontrado" });

            if (dados.Status == "aceito")
            {
                ordem.StatusProvedor = "Prestador_Alocado";
                ordem.MontadorNome = dados.NomeMontador;
                ordem.MontadorTelefone = dados.TelefoneMontador;
                ordem.MontadorFotoUrl = dados.FotoMontadorUrl;
                projeto.StatusProjeto = "Prestador_Alocado";
            }
            else if (dados.Status == "concluido")
            {
                ordem.StatusProvedor = "Concluido";
                projeto.StatusProjeto = "Aguardando_Vistoria";
                projeto.DataLimiteVistoria = DateTime.UtcNow.AddHours(Services.VistoriaService.HorasJanelaVistoria);
            }
            else if (dados.Status == "cancelado")
            {
                ordem.StatusProvedor = "Cancelado";
                projeto.StatusProjeto = "Cancelado";
            }

            ordem.DataAtualizacao = DateTime.UtcNow;
            await _ordemRepo.UpdateAsync(ordem);
            await _ordemRepo.SaveChangesAsync();
            await _projetoRepo.UpdateAsync(projeto);
            await _projetoRepo.SaveChangesAsync();

            return Ok(new { message = "Atualizacao de montador processada" });
        }
    }
}
