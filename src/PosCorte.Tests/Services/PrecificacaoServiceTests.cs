using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PosCorte.API.Services;
using PosCorte.API.Interfaces;

namespace PosCorte.Tests.Services
{
    public class PrecificacaoServiceTests
    {
        private readonly Mock<ILogger<PrecificacaoService>> _loggerMock;
        private readonly IPrecificacaoService _service;

        public PrecificacaoServiceTests()
        {
            _loggerMock = new Mock<ILogger<PrecificacaoService>>();
            _service = new PrecificacaoService(_loggerMock.Object);
        }

        [Fact]
        public void ProcessarProjeto_ComValoresValidos_DeveRetornarOrcamentoCorreto()
        {
            var resultado = _service.ProcessarProjeto(10, 5);

            Assert.NotNull(resultado);
            Assert.True(resultado.ValorTotal > resultado.CustoPrestador);
            Assert.True(resultado.MargemLucro > 0);
            Assert.Equal(20, resultado.TaxaPlataforma);
        }

        [Theory]
        [InlineData(1, 0, 15.62)]
        [InlineData(0, 1, 50.00)]
        [InlineData(5, 5, 328.12)]
        [InlineData(10, 10, 656.25)]
        public void ProcessarProjeto_VariosValores_DeveCalcularCorretamente(int pecas, int gavetas, double esperado)
        {
            var resultado = _service.ProcessarProjeto(pecas, gavetas);

            Assert.Equal(Math.Round((decimal)esperado, 2), resultado.ValorTotal);
        }

        [Fact]
        public void ProcessarProjeto_ComValoresNegativos_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => _service.ProcessarProjeto(-5, 5));
            Assert.Throws<ArgumentException>(() => _service.ProcessarProjeto(5, -5));
        }

        [Fact]
        public void ProcessarProjeto_ComZeroPecasEZeroGavetas_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => _service.ProcessarProjeto(0, 0));
        }

        [Fact]
        public void ProcessarProjeto_MargemLucroEhCorreta()
        {
            int pecas = 10;
            int gavetas = 5;
            decimal custoEsperado = (pecas * 12.50m) + (gavetas * 40.00m);
            decimal precoFinalEsperado = custoEsperado / (1 - 0.20m);
            decimal margemEsperada = precoFinalEsperado - custoEsperado;

            var resultado = _service.ProcessarProjeto(pecas, gavetas);

            Assert.Equal(Math.Round(margemEsperada, 2), resultado.MargemLucro);
        }
    }
}
