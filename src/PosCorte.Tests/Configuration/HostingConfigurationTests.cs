using Microsoft.Extensions.Configuration;
using Xunit;
using PosCorte.API.Configuration;

namespace PosCorte.Tests.Configuration
{
    public class HostingConfigurationTests
    {
        [Fact]
        public void ResolveDatabaseConnection_SubstituiDbPassword()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Host=db.test.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=${DB_PASSWORD};SSL Mode=Require",
                    ["DB_PASSWORD"] = "senha-secreta"
                })
                .Build();

            var cs = HostingConfiguration.ResolveDatabaseConnection(config);
            Assert.Contains("Password=senha-secreta", cs);
            Assert.DoesNotContain("${DB_PASSWORD}", cs);
        }

        [Fact]
        public void ResolveDatabaseConnection_SemSenha_DeveLancar()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Host=db.test.supabase.co;Password=${DB_PASSWORD}"
                })
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
                HostingConfiguration.ResolveDatabaseConnection(config));
        }

        [Fact]
        public void ResolveJwtKey_UsaJwtSecret()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "${JWT_SECRET}",
                    ["JWT_SECRET"] = "chave-super-secreta-com-32-caracteres-min"
                })
                .Build();

            var key = HostingConfiguration.ResolveJwtKey(config);
            Assert.Equal("chave-super-secreta-com-32-caracteres-min", key);
        }
    }
}
