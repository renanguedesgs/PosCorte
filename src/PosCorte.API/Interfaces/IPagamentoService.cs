namespace PosCorte.API.Interfaces
{
    public interface IPagamentoService
    {
        Task<bool> ValidarPagamentoPixAsync(string pixId, decimal valorEsperado);
        Task<bool> LiquidarFundosAsync(string pixId, decimal valor);
        Task<bool> ReservarFundosAsync(string pixId, decimal valor);
    }
}
