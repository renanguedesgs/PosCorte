using PosCorte.API.Models.DTOs;
using PosCorte.API.Services.Pagamentos.Asaas;

namespace PosCorte.API.Interfaces
{
    public interface IPagamentoService
    {
        /// <summary>Gateway Asaas configurado com ApiKey (cobrança real possível).</summary>
        bool GatewayConfigurado { get; }

        /// <summary>Modo stub ativo (sem cobrança real).</summary>
        bool ModoStub { get; }

        Task<GerarPixResponseDTO?> GerarPixAsync(int projetoId, int usuarioId);
        Task<PagamentoStatusDTO?> ObterStatusPagamentoAsync(int projetoId);
        Task<bool> SimularConfirmacaoStubAsync(int pagamentoId, int usuarioId);
        Task<bool> ProcessarWebhookAsaasAsync(AsaasWebhookPayload payload);
        Task<bool> ValidarPagamentoPixAsync(string pixId, decimal valorEsperado);
        Task<bool> LiquidarFundosAsync(string pixId, decimal valor);
        Task<bool> ReservarFundosAsync(string pixId, decimal valor);

        /// <summary>Liquida o último pagamento retido em escrow de um projeto (split marceneiro/plataforma).</summary>
        Task<bool> LiquidarPorProjetoAsync(int projetoId);
    }
}
