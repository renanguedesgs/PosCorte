using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class MarceneirosModel : PageModel
    {
        private readonly ApiService _api;

        public MarceneirosModel(ApiService api) => _api = api;

        public List<MarceneiroViewModel> Marceneiros { get; set; } = new();

        [BindProperty]
        public CreateMarceneiroAdminInput Input { get; set; } = new();

        public string? Mensagem { get; set; }
        public string? Erro { get; set; }

        public async Task OnGetAsync()
        {
            Marceneiros = await _api.ListarMarceneirosAsync();
        }

        public async Task<IActionResult> OnPostCadastrarAsync()
        {
            Marceneiros = await _api.ListarMarceneirosAsync();

            if (string.IsNullOrWhiteSpace(Input.Nome) || string.IsNullOrWhiteSpace(Input.Telefone) || string.IsNullOrWhiteSpace(Input.Cidade))
            {
                Erro = "Nome, telefone e cidade são obrigatórios.";
                return Page();
            }

            var (ok, erro) = await _api.CadastrarMarceneiroAdminAsync(Input);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível cadastrar.";
                return Page();
            }

            Mensagem = $"Montador {Input.Nome} adicionado à rede.";
            Input = new();
            Marceneiros = await _api.ListarMarceneirosAsync();
            return Page();
        }
    }
}
