using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Services;
using PosCorte.Domain.Entities;

namespace PosCorte.Tests.Services
{
    public class OperacaoManualServiceTests
    {
        private static PosCorteDbContext NovoDb()
        {
            var options = new DbContextOptionsBuilder<PosCorteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new PosCorteDbContext(options);
        }

        private static OperacaoManualService NovoServico(PosCorteDbContext db, Mock<INotificacaoService>? notif = null)
        {
            notif ??= new Mock<INotificacaoService>();
            return new OperacaoManualService(
                db,
                new PrecificacaoService(Mock.Of<ILogger<PrecificacaoService>>()),
                notif.Object,
                Mock.Of<ILogger<OperacaoManualService>>());
        }

        [Fact]
        public async Task CadastrarArquiteto_EmailNovo_DeveRetornarSenha()
        {
            var db = NovoDb();
            var service = NovoServico(db);

            var (response, resultado) = await service.CadastrarArquitetoAsync(new CreateArquitetoAdminDTO
            {
                Nome = "Ana Arquiteta",
                Email = "ana@test.com",
                CpfCnpj = "12345678901",
                Telefone = "11999990000"
            });

            Assert.Equal(ResultadoOperacaoManual.Ok, resultado);
            Assert.NotNull(response);
            Assert.False(string.IsNullOrEmpty(response.SenhaInicial));
            Assert.Equal(1, await db.Usuarios.CountAsync(u => u.Role == "Arquiteto"));
        }

        [Fact]
        public async Task CadastrarArquiteto_EmailDuplicado_DeveFalhar()
        {
            var db = NovoDb();
            db.Usuarios.Add(new Usuario("Existente", "ana@test.com", "123", ""));
            await db.SaveChangesAsync();
            var service = NovoServico(db);

            var (_, resultado) = await service.CadastrarArquitetoAsync(new CreateArquitetoAdminDTO
            {
                Nome = "Outra",
                Email = "ana@test.com",
                CpfCnpj = "999"
            });

            Assert.Equal(ResultadoOperacaoManual.EmailDuplicado, resultado);
        }

        [Fact]
        public async Task AlocarMontador_ProjetoAguardando_DeveAtualizarStatusENotificar()
        {
            var db = NovoDb();
            var projeto = new Projeto(1, "Cozinha Piloto", "https://plano", 12, 3, "01310-100", "Rua Teste")
            {
                StatusProjeto = "Aguardando_Provedor"
            };
            db.Projetos.Add(projeto);
            db.OrdensServico.Add(new OrdemServico(projeto.Id, "PC-abc") { StatusProvedor = "Aguardando_Provedor" });
            db.Marceneiros.Add(new Marceneiro { Nome = "João", Telefone = "11988887777", Cidade = "São Paulo", Estado = "SP" });
            await db.SaveChangesAsync();

            var marceneiroId = db.Marceneiros.First().Id;
            var notif = new Mock<INotificacaoService>();
            var service = NovoServico(db, notif);

            var resultado = await service.AlocarMontadorAsync(projeto.Id, new AlocarMontadorDTO { MarceneiroId = marceneiroId });

            Assert.Equal(ResultadoOperacaoManual.Ok, resultado);
            var atualizado = await db.Projetos.FindAsync(projeto.Id);
            Assert.Equal("Prestador_Alocado", atualizado!.StatusProjeto);
            var ordem = await db.OrdensServico.FirstAsync(o => o.ProjetoId == projeto.Id);
            Assert.Equal("João", ordem.MontadorNome);
            notif.Verify(n => n.NotificarMontador("11988887777", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task MarcarMontagemConcluida_ComMontadorAlocado_DeveIrParaVistoria()
        {
            var db = NovoDb();
            var projeto = new Projeto(1, "Cozinha", "url", 10, 2, "01310-100", "Av.")
            {
                StatusProjeto = "Prestador_Alocado"
            };
            db.Projetos.Add(projeto);
            await db.SaveChangesAsync();
            var service = NovoServico(db);

            var resultado = await service.MarcarMontagemConcluidaAsync(projeto.Id);

            Assert.Equal(ResultadoOperacaoManual.Ok, resultado);
            var atualizado = await db.Projetos.FindAsync(projeto.Id);
            Assert.Equal("Aguardando_Vistoria", atualizado!.StatusProjeto);
            Assert.NotNull(atualizado.DataLimiteVistoria);
        }
    }
}
