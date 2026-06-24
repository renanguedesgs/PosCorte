using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ContaModel : PageModel
    {
        private readonly ApiService _api;

        public ContaModel(ApiService api) => _api = api;

        [BindProperty]
        public string SenhaAtual { get; set; } = string.Empty;

        [BindProperty]
        public string SenhaNova { get; set; } = string.Empty;

        [BindProperty]
        public string SenhaNovaConfirmacao { get; set; } = string.Empty;

        public string? Mensagem { get; set; }
        public string? Erro { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (SenhaNova != SenhaNovaConfirmacao)
            {
                Erro = "A confirmação da nova senha não confere.";
                return Page();
            }

            var (ok, erro) = await _api.AlterarSenhaAdminAsync(SenhaAtual, SenhaNova);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível alterar a senha.";
                return Page();
            }

            Mensagem = "Senha alterada. Use a nova senha no próximo login.";
            SenhaAtual = SenhaNova = SenhaNovaConfirmacao = string.Empty;
            return Page();
        }
    }
}
