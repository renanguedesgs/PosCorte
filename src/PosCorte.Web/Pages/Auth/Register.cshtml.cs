using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        [BindProperty] public string Senha { get; set; } = string.Empty;

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
                string.IsNullOrWhiteSpace(Senha))
            {
                Erro = "Preencha todos os campos.";
                return Page();
            }

            if (Senha.Length < 6)
            {
                Erro = "A senha deve ter pelo menos 6 caracteres.";
                return Page();
            }

            var (ok, erro) = await _api.RegisterAsync(Nome, Email, CpfCnpj, Telefone, Senha);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível criar a conta.";
                return Page();
            }

            return RedirectToPage("/Auth/Login", new { registrado = "1" });
        }
    }
}
