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
    public class ProjetosControllerTests
    {
        private readonly Mock<ILogger<ProjetosController>> _loggerMock;
        private readonly Mock<IRepositorio<Projeto>> _projetoRepoMock;
        private readonly Mock<IRepositorio<Usuario>> _usuarioRepoMock;
        private readonly Mock<IPrecificacaoService> _precificacaoMock;
        private readonly Mock<IPagamentoService> _pagamentoMock;
        private readonly Mock<IVistoriaService> _vistoriaMock;
        private readonly ProjetosController _controller;

        public ProjetosControllerTests()
        {
            _loggerMock = new Mock<ILogger<ProjetosController>>();
            _projetoRepoMock = new Mock<IRepositorio<Projeto>>();
            _usuarioRepoMock = new Mock<IRepositorio<Usuario>>();
            _precificacaoMock = new Mock<IPrecificacaoService>();
            _pagamentoMock = new Mock<IPagamentoService>();
            _vistoriaMock = new Mock<IVistoriaService>();
            _controller = new ProjetosController(
                _loggerMock.Object,
                _projetoRepoMock.Object,
                _usuarioRepoMock.Object,
                _precificacaoMock.Object,
                _pagamentoMock.Object,
                _vistoriaMock.Object);

            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
                }, "TestAuth"));
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task CriarProjeto_ComUsuarioInexistente_DeveRetornar400()
        {
            var dto = new CreateProjetoDTO { UsuarioId = 999, NomeProjeto = "Projeto Teste" };

            _usuarioRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Usuario?)null);

            var result = await _controller.CriarProjeto(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CriarProjeto_ComDadosValidos_DeveRetornar201()
        {
            var usuario = new Usuario("Arquiteto", "arq@email.com", "12345678000195", "11999990000") { Id = 1 };
            var dto = new CreateProjetoDTO
            {
                UsuarioId = 1,
                NomeProjeto = "Cozinha Moderna",
                UrlArquivoCorteCloud = "https://storage.cloud/projeto.pdf",
                QtdPecas = 20,
                QtdGavetas = 4,
                CepObra = "01310-100",
                EnderecoCompleto = "Av. Paulista, 1000 - SP"
            };

            var projetoCriado = new Projeto(dto.UsuarioId, dto.NomeProjeto, dto.UrlArquivoCorteCloud,
                dto.QtdPecas, dto.QtdGavetas, dto.CepObra, dto.EnderecoCompleto) { Id = 1 };

            _usuarioRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
            _projetoRepoMock.Setup(r => r.AddAsync(It.IsAny<Projeto>())).ReturnsAsync(projetoCriado);
            _projetoRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _controller.CriarProjeto(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var projetoDTO = Assert.IsType<ProjetoDTO>(createdResult.Value);
            Assert.Equal("Cozinha Moderna", projetoDTO.NomeProjeto);
        }

        [Fact]
        public async Task CalcularOrcamento_ComProjetoExistente_DeveRetornarOrcamento()
        {
            var projeto = new Projeto(1, "Sala", "url", 10, 2, "01310-100", "Av. Paulista") { Id = 1 };
            var orcamentoEsperado = new OrcamentoResultado
            {
                ValorTotal = 256.25m,
                CustoPrestador = 205.00m,
                MargemLucro = 51.25m,
                TaxaPlataforma = 20
            };

            _projetoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(projeto);
            _precificacaoMock.Setup(s => s.ProcessarProjeto(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(orcamentoEsperado);

            var result = await _controller.CalcularOrcamento(1, new OrcamentoRequest());

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var orcamento = Assert.IsType<OrcamentoResultado>(okResult.Value);
            Assert.Equal(256.25m, orcamento.ValorTotal);
        }

        [Fact]
        public async Task ObterProjeto_ComIdInvalido_DeveRetornar404()
        {
            _projetoRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Projeto?)null);

            var result = await _controller.ObterProjeto(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
