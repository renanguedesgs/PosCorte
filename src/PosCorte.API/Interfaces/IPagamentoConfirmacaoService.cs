namespace PosCorte.API.Interfaces
{
    /// <summary>
    /// Fluxo pós-confirmação de pagamento: escrow, ordem no provedor, atualização de status.
    /// </summary>
    public interface IPagamentoConfirmacaoService
    {
        Task<bool> ConfirmarPagamentoAsync(int projetoId, string pixId, decimal valor);
    }
}
