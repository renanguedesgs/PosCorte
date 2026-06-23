using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PosCorte.API.Services;
using PosCorte.API.Interfaces;

namespace PosCorte.Tests.Services
{
    public class ProvedorServiceTests
    {
        private readonly Mock<ILogger<ProvedorService>> _loggerMock;
        private readonly Mock<IProvedorApi> _provedorApiMock;
        private readonly IProvedorService _service;

        public ProvedorServiceTests()
        {
            _loggerMock = new Mock<ILogger<ProvedorService>>();
            _provedorApiMock = new Mock<IProvedorApi>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ProvedorApi:BaseUrl"] = "https://api.parceiro-real.com",
                    ["ProvedorApi:ApiKey"] = "chave-de-teste",
                    ["ProvedorApi:Enabled"] = "true"
                })
                .Build();

            _service = new ProvedorService(_provedorApiMock.Object, _loggerMock.Object, config);
        }

        [Fact]
        public void EstaConfigurado_ComChaveEBaseUrlReal_DeveSerTrue()
        {
            Assert.True(_service.EstaConfigurado);
        }

        [Fact]
        public void EstaConfigurado_SemChave_DeveSerFalse()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ProvedorApi:BaseUrl"] = "https://api.provider.com",
                    ["ProvedorApi:ApiKey"] = "",
                    ["ProvedorApi:Enabled"] = "false"
                })
                .Build();

            var service = new ProvedorService(_provedorApiMock.Object, _loggerMock.Object, config);

            Assert.False(service.EstaConfigurado);
        }

        [Fact]
        public async Task CriarOrdemServicoAsync_ComDadosValidos_DeveRetornarResposta()
        {
            var request = new ProvedorRequest
            {
                EnderecoCompleto = "Rua Teste, 123 - São Paulo/SP",
                Cep = "01310-100",
                DataAgendamento = DateTime.UtcNow.AddDays(1),
                ValorTotal = 500.00m,
                UrlPlano = "https://storage.cloud/plano.pdf"
            };

            var respostaEsperada = new ProvedorResponse
            {
                ExternalProviderId = "EXT-99999",
                Status = "Pendente",
                MontadorNome = "Carlos Silva",
                MontadorTelefone = "11988887777"
            };

            _provedorApiMock.Setup(api => api.CriarOrdemServico(It.IsAny<ProvedorRequest>()))
                .ReturnsAsync(respostaEsperada);

            var resultado = await _service.CriarOrdemServicoAsync(request);

            Assert.NotNull(resultado);
            Assert.Equal("EXT-99999", resultado.ExternalProviderId);
            Assert.Equal("Pendente", resultado.Status);
        }

        [Fact]
        public async Task CriarOrdemServicoAsync_QuandoApiFalha_DeveLancarExcecao()
        {
            var request = new ProvedorRequest { EnderecoCompleto = "Rua Teste, 123" };

            _provedorApiMock.Setup(api => api.CriarOrdemServico(It.IsAny<ProvedorRequest>()))
                .ThrowsAsync(new HttpRequestException("Serviço indisponível"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CriarOrdemServicoAsync(request));
        }

        [Fact]
        public async Task ObterStatusAsync_ComExternalIdValido_DeveRetornarStatus()
        {
            var respostaEsperada = new ProvedorResponse
            {
                ExternalProviderId = "EXT-99999",
                Status = "Aceito"
            };

            _provedorApiMock.Setup(api => api.ObterStatusOrdem("EXT-99999"))
                .ReturnsAsync(respostaEsperada);

            var resultado = await _service.ObterStatusAsync("EXT-99999");

            Assert.NotNull(resultado);
            Assert.Equal("Aceito", resultado.Status);
        }
    }
}
