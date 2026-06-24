using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PosCorte.API.Data;
using PosCorte.API.Services;
using PosCorte.Domain.Entities;

namespace PosCorte.Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task AlterarSenha_ComSenhaAtualCorreta_DeveAtualizar()
        {
            var senhaInicial = "SenhaAntiga123";
            var db = new PosCorteDbContext(new DbContextOptionsBuilder<PosCorteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var u = new Usuario("Admin", "a@test.com", "1", "")
            {
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaInicial)
            };
            db.Usuarios.Add(u);
            await db.SaveChangesAsync();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Key"] = "test-key-min-32-characters-long!!" })
                .Build();

            var service = new AuthService(db, config, Mock.Of<ILogger<AuthService>>());

            var (ok, erro) = await service.AlterarSenhaAsync(u.Id, senhaInicial, "NovaSenha456");
            Assert.True(ok, erro);
            Assert.True(BCrypt.Net.BCrypt.Verify("NovaSenha456", (await db.Usuarios.FindAsync(u.Id))!.SenhaHash));
        }
    }
}
