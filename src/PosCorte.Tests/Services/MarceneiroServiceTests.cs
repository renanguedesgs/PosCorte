using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Services.Marceneiros;
using PosCorte.Domain.Entities;

namespace PosCorte.Tests.Services
{
    public class MarceneiroServiceTests
    {
        private static MarceneiroService CriarService(out PosCorteDbContext db, out Mock<INotificacaoService> notif)
        {
            var options = new DbContextOptionsBuilder<PosCorteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            db = new PosCorteDbContext(options);
            notif = new Mock<INotificacaoService>();
            notif.Setup(n => n.NotificarMontador(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            notif.Setup(n => n.EnviarEmailConfirmacao(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            return new MarceneiroService(db, notif.Object, Mock.Of<ILogger<MarceneiroService>>());
        }

        [Fact]
        public async Task AutoCadastrar_ComDadosValidos_CriaMontadorPendente()
        {
            var service = CriarService(out var db, out var notif);

            var (marceneiro, resultado) = await service.AutoCadastrarAsync(new AutoCadastroMarceneiroDTO
            {
                Nome = "João Montador",
                Telefone = "(11) 99999-8888",
                Cidade = "São Paulo",
                Estado = "sp"
            });

            Assert.Equal(ResultadoAutoCadastro.Ok, resultado);
            Assert.NotNull(marceneiro);
            Assert.False(marceneiro!.Verificado);
            Assert.False(marceneiro.Disponivel);
            Assert.Equal("autocadastro", marceneiro.OrigemExterna);
            Assert.Equal("11999998888", marceneiro.Telefone);
            Assert.Equal("SP", marceneiro.Estado);
            notif.Verify(n => n.NotificarMontador(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AutoCadastrar_TelefoneDuplicado_RetornaDuplicado()
        {
            var service = CriarService(out var db, out _);
            db.Marceneiros.Add(new Marceneiro { Nome = "Existente", Telefone = "11999998888", Cidade = "SP" });
            await db.SaveChangesAsync();

            var (marceneiro, resultado) = await service.AutoCadastrarAsync(new AutoCadastroMarceneiroDTO
            {
                Nome = "Outro",
                Telefone = "11 99999-8888",
                Cidade = "São Paulo"
            });

            Assert.Equal(ResultadoAutoCadastro.Duplicado, resultado);
            Assert.Null(marceneiro);
        }

        [Fact]
        public async Task AutoCadastrar_SemCamposObrigatorios_RetornaInvalido()
        {
            var service = CriarService(out _, out _);

            var (_, resultado) = await service.AutoCadastrarAsync(new AutoCadastroMarceneiroDTO
            {
                Nome = "",
                Telefone = "",
                Cidade = ""
            });

            Assert.Equal(ResultadoAutoCadastro.DadosInvalidos, resultado);
        }

        [Fact]
        public async Task Verificar_HomologaEDisponibilizaMontador()
        {
            var service = CriarService(out var db, out var notif);
            var m = new Marceneiro { Nome = "Pendente", Telefone = "11988887777", Email = "p@e.com", Cidade = "SP", Verificado = false, Disponivel = false };
            db.Marceneiros.Add(m);
            await db.SaveChangesAsync();

            var ok = await service.VerificarAsync(m.Id);

            Assert.True(ok);
            var atualizado = await db.Marceneiros.FindAsync(m.Id);
            Assert.True(atualizado!.Verificado);
            Assert.True(atualizado.Disponivel);
            notif.Verify(n => n.NotificarMontador(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AlternarDisponibilidade_InverteEstado()
        {
            var service = CriarService(out var db, out _);
            var m = new Marceneiro { Nome = "Disp", Telefone = "11000", Cidade = "SP", Disponivel = true };
            db.Marceneiros.Add(m);
            await db.SaveChangesAsync();

            var (ok, disponivel) = await service.AlternarDisponibilidadeAsync(m.Id);

            Assert.True(ok);
            Assert.False(disponivel);
        }

        [Fact]
        public async Task ListarParaAdmin_FiltraPorVerificado()
        {
            var service = CriarService(out var db, out _);
            db.Marceneiros.AddRange(
                new Marceneiro { Nome = "A", Cidade = "SP", Verificado = true },
                new Marceneiro { Nome = "B", Cidade = "SP", Verificado = false });
            await db.SaveChangesAsync();

            var pendentes = await service.ListarParaAdminAsync(verificado: false);
            var homologados = await service.ListarParaAdminAsync(verificado: true);

            Assert.Single(pendentes);
            Assert.Single(homologados);
        }
    }
}
