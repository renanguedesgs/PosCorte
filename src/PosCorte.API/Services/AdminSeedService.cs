using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosCorte.API.Data;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services
{
    public static class AdminSeedService
    {
        public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration config, ILogger logger)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PosCorteDbContext>();

            var email = config["Admin:Email"] ?? "admin@poscorte.com";
            var senha = config["Admin:Password"] ?? "Admin@PosCorte2026";

            if (!await db.Usuarios.AnyAsync(u => u.Email == email))
            {
                var admin = new Usuario("Administrador PósCorte", email, "00000000000", "11900000000")
                {
                    Role = "Admin",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                    Ativo = true
                };

                db.Usuarios.Add(admin);
                await db.SaveChangesAsync();
                logger.LogInformation("Usuário Admin criado: {Email}", email);
            }

            await SeedMarceneiroDemoAsync(db, logger);
        }

        private static async Task SeedMarceneiroDemoAsync(PosCorteDbContext db, ILogger logger)
        {
            if (await db.Marceneiros.AnyAsync()) return;

            var demo = new Marceneiro
            {
                Nome = "Carlos Montador Demo",
                Email = "carlos.montador@poscorte.demo",
                Telefone = "11987654321",
                Cidade = "São Paulo",
                Estado = "SP",
                Bairro = "Pinheiros",
                Cep = "05422000",
                Especialidades = "Cozinha,Dormitório,Home Office",
                Bio = "15 anos em móveis planejados · Grande SP",
                NotaMedia = 4.8m,
                TotalAvaliacoes = 12,
                TotalServicos = 28,
                Disponivel = true,
                Verificado = true,
                OrigemExterna = "seed:demo",
                FotoUrl = "https://ui-avatars.com/api/?name=Carlos+Montador&background=4f46e5&color=fff"
            };

            db.Marceneiros.Add(demo);
            await db.SaveChangesAsync();
            logger.LogInformation("Montador demo homologado criado (Pinheiros, SP) para testes de mapa.");
        }
    }
}
