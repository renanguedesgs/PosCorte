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

            if (await db.Usuarios.AnyAsync(u => u.Email == email))
                return;

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
    }
}
