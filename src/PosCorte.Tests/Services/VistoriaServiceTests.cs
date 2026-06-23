using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Services;
using PosCorte.Domain.Entities;

namespace PosCorte.Tests.Services
{
    public class VistoriaServiceTests
    {
        private static PosCorteDbContext NovoDb()
        {
            var options = new DbContextOptionsBuilder<PosCorteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new PosCorteDbContext(options);
        }

        private static VistoriaService NovoServico(PosCorteDbContext db, Mock<IPagamentoService> pagamentoMock)
        {
            var notifMock = new Mock<INotificacaoService>();
            return new VistoriaService(
                Mock.Of<ILogger<VistoriaService>>(),
                db,
                pagamentoMock.Object,
                notifMock.Object);
        }

        [Fact]
        public async Task AprovarMontagem_ComDono_DeveLiquidarEConcluir()
        {
            var db = NovoDb();
            var projeto = new Projeto(1, "Cozinha", "url", 10, 2, "01310-100", "Av. Paulista")
            {
                StatusProjeto = "Aguardando_Vistoria",
                DataLimiteVistoria = DateTime.UtcNow.AddHours(70)
            };
            db.Projetos.Add(projeto);
            await db.SaveChangesAsync();

            var pagamentoMock = new Mock<IPagamentoService>();
            pagamentoMock.Setup(p => p.LiquidarPorProjetoAsync(projeto.Id)).ReturnsAsync(true);
            var service = NovoServico(db, pagamentoMock);

            var resultado = await service.AprovarMontagemAsync(projeto.Id, usuarioId: 1);

            Assert.Equal(ResultadoVistoria.Ok, resultado);
            var atualizado = await db.Projetos.FindAsync(projeto.Id);
            Assert.Equal("Concluido", atualizado!.StatusProjeto);
            pagamentoMock.Verify(p => p.LiquidarPorProjetoAsync(projeto.Id), Times.Once);
        }

        [Fact]
        public async Task AprovarMontagem_NaoDono_DeveNegar()
        {
            var db = NovoDb();
            var projeto = new Projeto(1, "Cozinha", "url", 10, 2, "01310-100", "Av. Paulista")
            {
                StatusProjeto = "Aguardando_Vistoria"
            };
            db.Projetos.Add(projeto);
            await db.SaveChangesAsync();

            var service = NovoServico(db, new Mock<IPagamentoService>());

            var resultado = await service.AprovarMontagemAsync(projeto.Id, usuarioId: 999);

            Assert.Equal(ResultadoVistoria.NaoAutorizado, resultado);
        }

        [Fact]
        public async Task AbrirDisputa_DeveCongelarEscrow()
        {
            var db = NovoDb();
            var projeto = new Projeto(1, "Cozinha", "url", 10, 2, "01310-100", "Av. Paulista")
            {
                StatusProjeto = "Aguardando_Vistoria",
                DataLimiteVistoria = DateTime.UtcNow.AddHours(70)
            };
            db.Projetos.Add(projeto);
            await db.SaveChangesAsync();

            var service = NovoServico(db, new Mock<IPagamentoService>());

            var resultado = await service.AbrirDisputaAsync(projeto.Id, usuarioId: 1, motivo: "porta torta");

            Assert.Equal(ResultadoVistoria.Ok, resultado);
            var atualizado = await db.Projetos.FindAsync(projeto.Id);
            Assert.Equal("Em_Disputa", atualizado!.StatusProjeto);
            Assert.Null(atualizado.DataLimiteVistoria);
            Assert.Equal("porta torta", atualizado.MotivoDisputa);
        }

        [Fact]
        public async Task LiquidarVencidos_DeveLiberarApenasProjetosForaDoPrazo()
        {
            var db = NovoDb();
            var vencido = new Projeto(1, "Vencido", "url", 5, 1, "01000-000", "Rua A")
            {
                StatusProjeto = "Aguardando_Vistoria",
                DataLimiteVistoria = DateTime.UtcNow.AddHours(-1)
            };
            var dentroPrazo = new Projeto(1, "No prazo", "url", 5, 1, "01000-000", "Rua B")
            {
                StatusProjeto = "Aguardando_Vistoria",
                DataLimiteVistoria = DateTime.UtcNow.AddHours(10)
            };
            db.Projetos.AddRange(vencido, dentroPrazo);
            await db.SaveChangesAsync();

            var pagamentoMock = new Mock<IPagamentoService>();
            pagamentoMock.Setup(p => p.LiquidarPorProjetoAsync(It.IsAny<int>())).ReturnsAsync(true);
            var service = NovoServico(db, pagamentoMock);

            var total = await service.LiquidarVencidosAsync();

            Assert.Equal(1, total);
            var v = await db.Projetos.FindAsync(vencido.Id);
            var d = await db.Projetos.FindAsync(dentroPrazo.Id);
            Assert.Equal("Concluido", v!.StatusProjeto);
            Assert.Equal("Aguardando_Vistoria", d!.StatusProjeto);
        }
    }
}
