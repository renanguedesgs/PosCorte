using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Controllers;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.Tests.Controllers
{
    public class WebhookTests
    {
        private readonly Mock<ILogger<WebhookPoscorteController>> _loggerMock;
        private readonly Mock<IRepositorio<Projeto>> _projetoRepoMock;
        private readonly Mock<IRepositorio<OrdemServico>> _ordemRepoMock;
        private readonly Mock<IPagamentoService> _pagamentoMock;
        private readonly Mock<IProvedorService> _provedorMock;
        private readonly Mock<INotificacaoService> _notificacaoMock;
        private readonly WebhookPoscorteController _controller;

        public WebhookTests()
        {
            _loggerMock = new Mock<ILogger<WebhookPoscorteController>>();
            _projetoRepoMock = new Mock<IRepositorio<Projeto>>();
            _ordemRepoMock = new Mock<IRepositorio<OrdemServico>>();
            _pagamentoMock = new Mock<IPagamentoService>();
            _provedorMock = new Mock<IProvedorService>();
            _notificacaoMock = new Mock<INotificacaoService>();

            _controller = new WebhookPoscorteController(
                _loggerMock.Object,
                _projetoRepoMock.Object,
                _ordemRepoMock.Object,
                _pagamentoMock.Object,
                _provedorMock.Object,
                _notificacaoMock.Object);
        }

        [Fact]
        public async Task TratarPagamentoConfirmado_ComProjetoInexistente_DeveRetornar404()
        {
            var dados = new WebhookPagamento { ProjetoId = 999, Status = "pago", PixId = "pix-1", Valor = 300m };

            _projetoRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Projeto?)null);

            var result = await _controller.TratarPagamentoConfirmado(dados);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task TratarPagamentoConfirmado_ComPagamentoInvalido_DeveRetornar400()
        {
            var projeto = new Projeto(1, "Sala", "url", 5, 1, "01310-100", "Av. Paulista") { Id = 1 };
            var dados = new WebhookPagamento { ProjetoId = 1, Status = "pago", PixId = "pix-1", Valor = 300m };

            _projetoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(projeto);
            _pagamentoMock.Setup(p => p.ValidarPagamentoPixAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(false);

            var result = await _controller.TratarPagamentoConfirmado(dados);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task TratarPagamentoConfirmado_ComPagamentoValido_DeveCriarOrdem()
        {
            var projeto = new Projeto(1, "Sala", "url", 5, 1, "01310-100", "Av. Paulista") { Id = 1 };
            var dados = new WebhookPagamento { ProjetoId = 1, Status = "pago", PixId = "pix-1", Valor = 300m };

            var provedorResponse = new ProvedorResponse
            {
                ExternalProviderId = "EXT-001",
                Status = "Pendente",
                MontadorNome = "João Montador",
                MontadorTelefone = "11988880000"
            };

            _projetoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(projeto);
            _pagamentoMock.Setup(p => p.ValidarPagamentoPixAsync(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(true);
            _pagamentoMock.Setup(p => p.ReservarFundosAsync(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(true);
            _provedorMock.Setup(p => p.CriarOrdemServicoAsync(It.IsAny<ProvedorRequest>())).ReturnsAsync(provedorResponse);
            _ordemRepoMock.Setup(r => r.AddAsync(It.IsAny<OrdemServico>())).ReturnsAsync(new OrdemServico(1, "EXT-001"));
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
            _projetoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Projeto>())).ReturnsAsync(projeto);
            _projetoRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _controller.TratarPagamentoConfirmado(dados);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task TratarAtualizacaoMontador_ComOrdemInexistente_DeveRetornar404()
        {
            var dados = new WebhookData { IdExternalProviderId = "EXT-INEXISTENTE", Status = "aceito" };

            _ordemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<OrdemServico>());

            var result = await _controller.TratarAtualizacaoMontador(dados);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task TratarAtualizacaoMontador_StatusConcluido_DeveAtualizarProjeto()
        {
            var ordem = new OrdemServico(1, "EXT-001") { Id = 1 };
            var projeto = new Projeto(1, "Sala", "url", 5, 1, "01310-100", "Av. Paulista") { Id = 1 };
            var dados = new WebhookData
            {
                IdExternalProviderId = "EXT-001",
                Status = "concluido"
            };

            _ordemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<OrdemServico> { ordem });
            _projetoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(projeto);
            _ordemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<OrdemServico>())).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
            _projetoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Projeto>())).ReturnsAsync(projeto);
            _projetoRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _controller.TratarAtualizacaoMontador(dados);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Aguardando_Vistoria", projeto.StatusProjeto);
        }
    }
}
