using Refit;
using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    public interface IProvedorApi
    {
        [Post("/ordensservico")]
        Task<ProvedorResponse> CriarOrdemServico([Body] ProvedorRequest request);

        [Get("/ordensservico/{externalId}")]
        Task<ProvedorResponse> ObterStatusOrdem(string externalId);
    }

    public class ProvedorService : IProvedorService
    {
        private readonly IProvedorApi _provedorApi;
        private readonly ILogger<ProvedorService> _logger;

        public ProvedorService(IProvedorApi provedorApi, ILogger<ProvedorService> logger)
        {
            _provedorApi = provedorApi;
            _logger = logger;
        }

        public async Task<ProvedorResponse> CriarOrdemServicoAsync(ProvedorRequest request)
        {
            try
            {
                _logger.LogInformation("Criando ordem de serviço para: {Endereco}", request.EnderecoCompleto);

                var resposta = await _provedorApi.CriarOrdemServico(request);

                _logger.LogInformation("Ordem criada com sucesso. ID Externo: {ExternalId}", resposta.ExternalProviderId);

                return resposta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar ordem no provedor");
                throw new InvalidOperationException("Falha na comunicação com provedor de montadores", ex);
            }
        }

        public async Task<ProvedorResponse> ObterStatusAsync(string externalId)
        {
            try
            {
                _logger.LogInformation("Consultando status de ordem: {ExternalId}", externalId);

                return await _provedorApi.ObterStatusOrdem(externalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status da ordem {ExternalId}", externalId);
                throw;
            }
        }
    }
}
