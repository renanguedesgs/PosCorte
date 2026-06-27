using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Marceneiros
{
    [AllowAnonymous]
    public class SejaModel : PageModel
    {
        private readonly ApiService _api;

        public SejaModel(ApiService api) => _api = api;

        [BindProperty] public AutoCadastroMarceneiroInput Input { get; set; } = new();

        public bool Sucesso { get; set; }
        public string? Erro { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.Nome) ||
                string.IsNullOrWhiteSpace(Input.Telefone) ||
                string.IsNullOrWhiteSpace(Input.Cidade))
            {
                Erro = "Preencha nome, WhatsApp e cidade.";
                return Page();
            }

            var (ok, erro) = await _api.AutoCadastrarMarceneiroAsync(Input);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível enviar seu cadastro.";
                return Page();
            }

            Sucesso = true;
            Input = new();
            return Page();
        }
    }
}
