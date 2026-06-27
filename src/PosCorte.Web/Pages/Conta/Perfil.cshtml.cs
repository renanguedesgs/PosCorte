using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Domain.Validation;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Conta
{
    [Authorize]
    public class PerfilModel : PageModel
    {
        private readonly ApiService _api;

        public PerfilModel(ApiService api) => _api = api;

        public PerfilViewModel? Perfil { get; set; }
        public string? Mensagem { get; set; }
        public string? Erro { get; set; }

        [BindProperty] public string SenhaAtual { get; set; } = string.Empty;
        [BindProperty] public string SenhaNova { get; set; } = string.Empty;
        [BindProperty] public string SenhaNovaConfirmacao { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            Perfil = await _api.ObterPerfilAsync();
            if (Perfil == null)
                Erro = "Não foi possível carregar seu perfil.";
            return Page();
        }

        public async Task<IActionResult> OnPostSenhaAsync()
        {
            Perfil = await _api.ObterPerfilAsync();

            if (SenhaNova != SenhaNovaConfirmacao)
            {
                Erro = "A confirmação da nova senha não confere.";
                return Page();
            }

            var erroSenha = SenhaPolicy.ObterErro(SenhaNova);
            if (erroSenha is not null)
            {
                Erro = erroSenha;
                return Page();
            }

            var (ok, erro) = await _api.AlterarSenhaAsync(SenhaAtual, SenhaNova);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível alterar a senha.";
                return Page();
            }

            Mensagem = "Senha alterada com sucesso.";
            SenhaAtual = SenhaNova = SenhaNovaConfirmacao = string.Empty;
            return Page();
        }
    }
}
