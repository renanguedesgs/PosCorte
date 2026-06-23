using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PosCorte.API.Configuration;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Services;
using PosCorte.API.Services.Pagamentos.Asaas;

namespace PosCorte.Tests.Services
{
    public class PagamentoServiceTests
    {
        private readonly PagamentoService _service;

        public PagamentoServiceTests()
        {
            var options = new DbContextOptionsBuilder<PosCorteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new PosCorteDbContext(options);

            var asaasOpts = Options.Create(new AsaasOptions { Enabled = false });
            var asaasMock = new Mock<IAsaasClient>();
            var precMock = new Mock<IPrecificacaoService>();
            var confMock = new Mock<IPagamentoConfirmacaoService>();
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(s => s.GetService(typeof(IPagamentoConfirmacaoService))).Returns(confMock.Object);
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns("Development");

            _service = new PagamentoService(
                Mock.Of<ILogger<PagamentoService>>(),
                db,
                asaasOpts,
                asaasMock.Object,
                precMock.Object,
                spMock.Object,
                envMock.Object);
        }

        [Fact]
        public void ModoStub_QuandoAsaasDesligado_DeveSerTrue()
        {
            Assert.True(_service.ModoStub);
            Assert.False(_service.GatewayConfigurado);
        }
    }
}
