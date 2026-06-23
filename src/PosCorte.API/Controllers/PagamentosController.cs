using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Interfaces;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/pagamentos")]
    [Produces("application/json")]
    public class PagamentosController : ControllerBase
    {
        private readonly IPagamentoService _pagamentoService;
        private readonly IWebHostEnvironment _env;

        public PagamentosController(IPagamentoService pagamentoService, IWebHostEnvironment env)
        {
            _pagamentoService = pagamentoService;
            _env = env;
        }

        /// <summary>
        /// Simula confirmação de PIX em modo stub (somente Development).
        /// Não use em produção — substitua por webhook Asaas real.
        /// </summary>
        [HttpPost("{pagamentoId}/simular-confirmacao")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SimularConfirmacao(int pagamentoId)
        {
            if (!_env.IsDevelopment())
                return StatusCode(403, new { error = "Simulação disponível apenas em Development." });

            if (_pagamentoService.GatewayConfigurado)
                return BadRequest(new { error = "Gateway Asaas ativo. Use pagamento real ou webhook." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var ok = await _pagamentoService.SimularConfirmacaoStubAsync(pagamentoId, userId);

            if (!ok)
                return BadRequest(new { error = "Não foi possível simular. Verifique pagamento stub e dono do projeto." });

            return Ok(new { message = "Pagamento simulado com sucesso. Ordem de serviço criada." });
        }
    }
}
