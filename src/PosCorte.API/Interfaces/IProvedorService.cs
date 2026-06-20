namespace PosCorte.API.Interfaces
{
    public interface IProvedorService
    {
        Task<ProvedorResponse> CriarOrdemServicoAsync(ProvedorRequest request);
        Task<ProvedorResponse> ObterStatusAsync(string externalId);
    }

    public class ProvedorRequest
    {
        public string EnderecoCompleto { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public DateTime DataAgendamento { get; set; }
        public decimal ValorTotal { get; set; }
        public string UrlPlano { get; set; } = string.Empty;
    }

    public class ProvedorResponse
    {
        public string ExternalProviderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MontadorNome { get; set; } = string.Empty;
        public string MontadorTelefone { get; set; } = string.Empty;
    }
}
