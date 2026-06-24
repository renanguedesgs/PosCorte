namespace PosCorte.API.Configuration
{
    public static class HostingConfiguration
    {
        /// <summary>
        /// Resolve a connection string do Supabase/PostgreSQL em produção.
        /// Ordem: DATABASE_URL → ConnectionStrings__DefaultConnection → DefaultConnection com DB_PASSWORD.
        /// </summary>
        public static string ResolveDatabaseConnection(IConfiguration configuration)
        {
            var databaseUrl = configuration["DATABASE_URL"]
                ?? Environment.GetEnvironmentVariable("DATABASE_URL");

            if (!string.IsNullOrWhiteSpace(databaseUrl))
                return NormalizePostgresUrl(databaseUrl);

            var fromEnv = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(fromEnv))
                throw new InvalidOperationException(
                    "Banco não configurado. Defina ConnectionStrings__DefaultConnection ou DATABASE_URL no host (Railway/Render).");

            var password = configuration["DB_PASSWORD"]
                ?? Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrWhiteSpace(password))
                fromEnv = fromEnv.Replace("${DB_PASSWORD}", password, StringComparison.Ordinal);

            if (fromEnv.Contains("${DB_PASSWORD}", StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Senha do banco ausente. Defina a variável DB_PASSWORD ou use ConnectionStrings__DefaultConnection completa.");

            return fromEnv;
        }

        public static string ResolveJwtKey(IConfiguration configuration)
        {
            var key = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key) || key == "${JWT_SECRET}")
            {
                key = configuration["JWT_SECRET"]
                    ?? Environment.GetEnvironmentVariable("JWT_SECRET");
            }

            if (string.IsNullOrWhiteSpace(key) || key == "${JWT_SECRET}")
                throw new InvalidOperationException(
                    "JWT não configurado. Defina Jwt__Key ou JWT_SECRET (mín. 32 caracteres) no host.");

            if (key.Length < 32)
                throw new InvalidOperationException("JWT_SECRET deve ter pelo menos 32 caracteres.");

            return key;
        }

        /// <summary>Converte postgres:// ou postgresql:// para formato Npgsql.</summary>
        private static string NormalizePostgresUrl(string url)
        {
            if (!url.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                && !url.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
                return url;

            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);
            var user = Uri.UnescapeDataString(userInfo[0]);
            var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var db = uri.AbsolutePath.TrimStart('/');
            var port = uri.Port > 0 ? uri.Port : 5432;

            return $"Host={uri.Host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true";
        }
    }
}
