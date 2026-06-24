using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ArquitetosModel : PageModel
    {
        private readonly ApiService _api;

        public ArquitetosModel(ApiService api) => _api = api;

        public List<ArquitetoAdminViewModel> Arquitetos { get; set; } = new();

        [BindProperty]
        public CreateArquitetoAdminInput Input { get; set; } = new();

        public string? Mensagem { get; set; }
        public string? Erro { get; set; }
        public string? SenhaGerada { get; set; }

        public async Task OnGetAsync()
        {
            Arquitetos = await _api.ListarArquitetosAdminAsync();
        }

        public async Task<IActionResult> OnPostCadastrarAsync()
        {
            Arquitetos = await _api.ListarArquitetosAdminAsync();

            if (!ModelState.IsValid)
            {
                Erro = "Preencha nome, e-mail e CPF/CNPJ.";
                return Page();
            }

            var (ok, erro, data) = await _api.CadastrarArquitetoAdminAsync(Input);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível cadastrar.";
                return Page();
            }

            Mensagem = $"Arquiteto {data?.Arquiteto.Nome} cadastrado.";
            SenhaGerada = data?.SenhaInicial;
            Input = new();
            Arquitetos = await _api.ListarArquitetosAdminAsync();
            return Page();
        }
    }
}
