using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Domain.Validation;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly ApiService _api;

        public RegisterModel(ApiService api) => _api = api;

        [BindProperty] public string Nome { get; set; } = string.Empty;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string CpfCnpj { get; set; } = string.Empty;
        [BindProperty] public string Telefone { get; set; } = string.Empty;
        [BindProperty] public string Cep { get; set; } = string.Empty;
        [BindProperty] public string Endereco { get; set; } = string.Empty;
        [BindProperty] public string Senha { get; set; } = string.Empty;
        [BindProperty] public string SenhaConfirmacao { get; set; } = string.Empty;

        public string? Erro { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Dashboard");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(CpfCnpj) || string.IsNullOrWhiteSpace(Telefone) ||
                string.IsNullOrWhiteSpace(Cep) || string.IsNullOrWhiteSpace(Endereco) ||
                string.IsNullOrWhiteSpace(Senha) || string.IsNullOrWhiteSpace(SenhaConfirmacao))
            {
                Erro = "Preencha todos os campos.";
                return Page();
            }

            if (!DocumentoBrasil.EhCpfOuCnpjValido(CpfCnpj))
            {
                Erro = "CPF ou CNPJ inválido.";
                return Page();
            }

            if (!TelefoneBrasil.EhValido(Telefone))
            {
                Erro = "Telefone inválido. Use DDD + número (10 ou 11 dígitos).";
                return Page();
            }

            if (!CepBrasil.EhValido(Cep))
            {
                Erro = "CEP inválido.";
                return Page();
            }

            var erroSenha = SenhaPolicy.ObterErro(Senha);
            if (erroSenha is not null)
            {
                Erro = erroSenha;
                return Page();
            }

            if (Senha != SenhaConfirmacao)
            {
                Erro = "As senhas não conferem.";
                return Page();
            }

            var (ok, erro) = await _api.RegisterAsync(Nome, Email, CpfCnpj, Telefone, Senha);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível criar a conta.";
                return Page();
            }

            var (loginOk, token, _) = await _api.LoginAsync(Email.Trim(), Senha);
            if (loginOk && token is not null)
            {
                HttpContext.Session.SetString("jwt", token);
                var claims = JwtHelper.ExtrairClaims(token);
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                    claims.Add(new Claim(ClaimTypes.Name, Nome));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties { IsPersistent = true });

                return RedirectToPage("/Projetos/Criar", new { onboarding = true });
            }

            return RedirectToPage("/Auth/Login", new { registrado = "1" });
        }
    }
}
