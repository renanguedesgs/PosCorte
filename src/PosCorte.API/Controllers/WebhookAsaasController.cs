using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PosCorte.API.Configuration;
using PosCorte.API.Interfaces;
using PosCorte.API.Services.Pagamentos.Asaas;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/webhooks")]
    [Produces("application/json")]
    public class WebhookAsaasController : ControllerBase
    {
        private readonly IPagamentoService _pagamentoService;
        private readonly AsaasOptions _options;
        private readonly ILogger<WebhookAsaasController> _logger;

        public WebhookAsaasController(
            IPagamentoService pagamentoService,
            IOptions<AsaasOptions> options,
            ILogger<WebhookAsaasController> logger)
        {
            _pagamentoService = pagamentoService;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Webhook oficial do Asaas (PIX confirmado). Configure no painel Asaas:
        /// URL = https://SEU_DOMINIO/api/v1/webhooks/asaas
        /// </summary>
        [HttpPost("asaas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReceberAsaas([FromBody] AsaasWebhookPayload payload)
        {
            if (!string.IsNullOrWhiteSpace(_options.WebhookToken))
            {
                var token = Request.Headers["asaas-access-token"].FirstOrDefault();
                if (!string.Equals(token, _options.WebhookToken, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Webhook Asaas rejeitado: token inválido");
                    return Unauthorized();
                }
            }

            _logger.LogInformation("Webhook Asaas: evento {Evento}", payload.Event);

            var ok = await _pagamentoService.ProcessarWebhookAsaasAsync(payload);
            return Ok(new { processed = ok });
        }
    }
}
