namespace PosCorte.API.Interfaces
{
    public interface IProvedorService
    {
        /// <summary>
        /// Indica se a integração com a API externa de marceneiros está
        /// configurada (BaseUrl + ApiKey reais e habilitada). Quando false, o
        /// sistema não inventa montadores: a ordem fica "Aguardando_Provedor".
        /// </summary>
        bool EstaConfigurado { get; }

        Task<ProvedorResponse> CriarOrdemServicoAsync(ProvedorRequest request);
        Task<ProvedorResponse> ObterStatusAsync(string externalId);
    }

    public class ProvedorRequest
    {
        public string NomeProjeto { get; set; } = string.Empty;
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
