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
        private readonly IProvedorService _provedorService;
        private readonly INotificacaoService _notificacaoService;

        public WebhookPoscorteController(
            ILogger<WebhookPoscorteController> logger,
            IRepositorio<Projeto> projetoRepo,
            IRepositorio<OrdemServico> ordemRepo,
            IPagamentoService pagamentoService,
            IProvedorService provedorService,
            INotificacaoService notificacaoService)
        {
            _logger = logger;
            _projetoRepo = projetoRepo;
            _ordemRepo = ordemRepo;
            _pagamentoService = pagamentoService;
            _provedorService = provedorService;
            _notificacaoService = notificacaoService;
        }

        /// <summary>
        /// Webhook: Pagamento confirmado via PIX.
        /// TRAVA DE SEGURANÇA: Ordem só é criada após confirmação de pagamento.
        /// </summary>
        [HttpPost("pagamento-confirmado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TratarPagamentoConfirmado([FromBody] WebhookPagamento dados)
        {
            _logger.LogInformation("Webhook pagamento recebido: Projeto {ProjetoId}, Status: {Status}", dados.ProjetoId, dados.Status);

            try
            {
                var projeto = await _projetoRepo.GetByIdAsync(dados.ProjetoId);
                if (projeto == null)
                    return NotFound(new { error = "Projeto não encontrado" });

                if (dados.Status == "pago")
                {
                    bool pagamentoValido = await _pagamentoService.ValidarPagamentoPixAsync(dados.PixId, dados.Valor);

                    if (!pagamentoValido)
                        return BadRequest(new { error = "Pagamento não validado" });

                    bool fundosReservados = await _pagamentoService.ReservarFundosAsync(dados.PixId, dados.Valor);

                    if (!fundosReservados)
                        return BadRequest(new { error = "Falha ao reservar fundos em escrow" });

                    projeto.StatusProjeto = "Pagamento_Confirmado";

                    var request = new ProvedorRequest
                    {
                        EnderecoCompleto = projeto.EnderecoCompleto,
                        Cep = projeto.CepObra,
                        DataAgendamento = DateTime.UtcNow.AddDays(1),
                        ValorTotal = dados.Valor,
                        UrlPlano = projeto.UrlArquivoCorteCloud
                    };

                    var provedorResponse = await _provedorService.CriarOrdemServicoAsync(request);

                    var ordem = new OrdemServico(projeto.Id, provedorResponse.ExternalProviderId)
                    {
                        StatusProvedor = provedorResponse.Status,
                        MontadorNome = provedorResponse.MontadorNome,
                        MontadorTelefone = provedorResponse.MontadorTelefone
                    };

                    await _ordemRepo.AddAsync(ordem);
                    await _ordemRepo.SaveChangesAsync();

                    projeto.StatusProjeto = "Ordem_Criada";
                    await _projetoRepo.UpdateAsync(projeto);
                    await _projetoRepo.SaveChangesAsync();

                    _logger.LogInformation("Ordem criada com sucesso para projeto {ProjetoId}", dados.ProjetoId);
                }

                return Ok(new { message = "Webhook de pagamento processado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook de pagamento");
                return StatusCode(500, new { error = "Erro ao processar webhook", details = ex.Message });
            }
        }

        /// <summary>
        /// Webhook: Atualização de status do montador.
        /// Estados: aceito, a_caminho, concluido, cancelado.
        /// </summary>
        [HttpPost("atualizacao-montador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TratarAtualizacaoMontador([FromBody] WebhookData dados)
        {
            _logger.LogInformation("Webhook montador recebido: ID {ExternalId}, Status: {Status}", dados.IdExternalProviderId, dados.Status);

            try
            {
                var ordensServico = await _ordemRepo.GetAllAsync();
                var ordem = ordensServico.FirstOrDefault(o => o.ExternalProviderId == dados.IdExternalProviderId);

                if (ordem == null)
                    return NotFound(new { error = "Ordem não encontrada" });

                var projeto = await _projetoRepo.GetByIdAsync(ordem.ProjetoId);
                if (projeto == null)
                    return NotFound(new { error = "Projeto não encontrado" });

                if (dados.Status == "aceito")
                {
                    ordem.StatusProvedor = "Prestador_Alocado";
                    ordem.MontadorNome = dados.NomeMontador;
                    ordem.MontadorTelefone = dados.TelefoneMontador;
                    ordem.MontadorFotoUrl = dados.FotoMontadorUrl;
                    projeto.StatusProjeto = "Prestador_Alocado";

                    _logger.LogInformation("Montador alocado: {NomeMontador}", dados.NomeMontador);
                }
                else if (dados.Status == "concluido")
                {
                    ordem.StatusProvedor = "Concluido";
                    projeto.StatusProjeto = "Aguardando_Vistoria";

                    // TODO: Implementar agendador (Hangfire/Quartz) para liquidação após 72h úteis
                    _logger.LogInformation("Serviço concluído. Iniciando contagem de 72h para liquidação");
                }
                else if (dados.Status == "cancelado")
                {
                    ordem.StatusProvedor = "Cancelado";
                    projeto.StatusProjeto = "Cancelado";

                    _logger.LogWarning("Serviço cancelado para projeto {ProjetoId}", ordem.ProjetoId);
                }

                ordem.DataAtualizacao = DateTime.UtcNow;
                await _ordemRepo.UpdateAsync(ordem);
                await _ordemRepo.SaveChangesAsync();

                await _projetoRepo.UpdateAsync(projeto);
                await _projetoRepo.SaveChangesAsync();

                return Ok(new { message = "Atualização de montador processada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook de montador");
                return StatusCode(500, new { error = "Erro ao processar webhook", details = ex.Message });
            }
        }
    }
}
