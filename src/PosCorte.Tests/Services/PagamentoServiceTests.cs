using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PosCorte.API.Services;
using PosCorte.API.Interfaces;

namespace PosCorte.Tests.Services
{
    public class PagamentoServiceTests
    {
        private readonly Mock<ILogger<PagamentoService>> _loggerMock;
        private readonly IPagamentoService _service;

        public PagamentoServiceTests()
        {
            _loggerMock = new Mock<ILogger<PagamentoService>>();
            _service = new PagamentoService(_loggerMock.Object);
        }

        [Fact]
        public async Task ValidarPagamentoPixAsync_DeveRetornarTrue()
        {
            var resultado = await _service.ValidarPagamentoPixAsync("pix-123", 500.00m);

            Assert.True(resultado);
        }

        [Fact]
        public async Task ReservarFundosAsync_DeveRetornarTrue()
        {
            var resultado = await _service.ReservarFundosAsync("pix-123", 500.00m);

            Assert.True(resultado);
        }

        [Fact]
        public async Task LiquidarFundosAsync_DeveRetornarTrue()
        {
            var resultado = await _service.LiquidarFundosAsync("pix-123", 500.00m);

            Assert.True(resultado);
        }
    }
}
