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
    public class UsuariosControllerTests
    {
        private readonly Mock<ILogger<UsuariosController>> _loggerMock;
        private readonly Mock<IRepositorio<Usuario>> _usuarioRepoMock;
        private readonly UsuariosController _controller;

        public UsuariosControllerTests()
        {
            _loggerMock = new Mock<ILogger<UsuariosController>>();
            _usuarioRepoMock = new Mock<IRepositorio<Usuario>>();
            _controller = new UsuariosController(_loggerMock.Object, _usuarioRepoMock.Object);
        }

        [Fact]
        public async Task CriarUsuario_ComDadosValidos_DeveRetornar201()
        {
            var createDto = new CreateUsuarioDTO
            {
                Nome = "João Silva",
                Email = "joao@email.com",
                CpfCnpj = "12345678901234",
                Telefone = "11999999999"
            };

            var usuarioCriado = new Usuario(createDto.Nome, createDto.Email, createDto.CpfCnpj, createDto.Telefone) { Id = 1 };

            _usuarioRepoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>())).ReturnsAsync(usuarioCriado);
            _usuarioRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _controller.CriarUsuario(createDto);

            Assert.NotNull(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(UsuariosController.ObterUsuario), createdResult.ActionName);
            Assert.Equal(1, ((UsuarioDTO)createdResult.Value!).Id);
        }

        [Fact]
        public async Task ObterUsuario_ComIdValido_DeveRetornarUsuario()
        {
            var usuario = new Usuario("João", "joao@email.com", "12345678901234", "11999999999") { Id = 1 };
            _usuarioRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);

            var result = await _controller.ObterUsuario(1);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDto = Assert.IsType<UsuarioDTO>(okResult.Value);
            Assert.Equal("João", returnedDto.Nome);
        }

        [Fact]
        public async Task ObterUsuario_ComIdInvalido_DeveRetornar404()
        {
            _usuarioRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Usuario?)null);

            var result = await _controller.ObterUsuario(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
