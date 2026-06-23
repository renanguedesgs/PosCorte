using System.Security.Claims;
using System.Text.Json;

namespace PosCorte.Web.Services
{
    /// <summary>
    /// Decodifica o payload de um JWT (sem validar assinatura — a API já validou ao emitir)
    /// apenas para preencher a identidade do cookie no front-end.
    /// </summary>
    public static class JwtHelper
    {
        public static List<Claim> ExtrairClaims(string token)
        {
            var claims = new List<Claim>();
            var partes = token.Split('.');
            if (partes.Length < 2) return claims;

            var json = DecodeBase64Url(partes[1]);
            using var doc = JsonDocument.Parse(json);

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var valor = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString() ?? string.Empty
                    : prop.Value.ToString();

                var tipo = prop.Name switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "email" => ClaimTypes.Email,
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role" => ClaimTypes.Role,
                    _ => prop.Name
                };

                claims.Add(new Claim(tipo, valor));
            }

            return claims;
        }

        public static string? ObterUsuarioId(string token) =>
            ExtrairClaims(token).FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        private static string DecodeBase64Url(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }
    }
}
