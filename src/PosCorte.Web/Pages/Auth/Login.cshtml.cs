using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly ApiService _api;

        public LoginModel(ApiService api) => _api = api;

        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Senha { get; set; } = string.Empty;

        public string? Erro { get; set; }
        public string? Sucesso { get; set; }

        public void OnGet(string? registrado)
        {
            if (registrado == "1")
                Sucesso = "Conta criada com sucesso! Faça login para continuar.";
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Erro = "Preencha e-mail e senha.";
                return Page();
            }

            var (ok, token, erro) = await _api.LoginAsync(Email, Senha);
            if (!ok || token is null)
            {
                Erro = erro ?? "E-mail ou senha inválidos.";
                return Page();
            }

            HttpContext.Session.SetString("jwt", token);

            var claims = JwtHelper.ExtrairClaims(token);
            if (!claims.Any(c => c.Type == ClaimTypes.Name))
                claims.Add(new Claim(ClaimTypes.Name, Email));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (role == "Admin")
                return RedirectToPage("/Admin/Index");

            return RedirectToPage("/Dashboard");
        }
    }
}
