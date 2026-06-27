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

        public List<MarceneiroAdminViewModel> Pendentes { get; set; } = new();
        public List<MarceneiroAdminViewModel> Homologados { get; set; } = new();

        [BindProperty]
        public CreateMarceneiroAdminInput Input { get; set; } = new();

        public string? Mensagem { get; set; }
        public string? Erro { get; set; }

        public async Task OnGetAsync() => await CarregarAsync();

        private async Task CarregarAsync()
        {
            Pendentes = await _api.ListarMarceneirosAdminAsync(verificado: false);
            Homologados = await _api.ListarMarceneirosAdminAsync(verificado: true);
        }

        public async Task<IActionResult> OnPostCadastrarAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.Nome) || string.IsNullOrWhiteSpace(Input.Telefone) || string.IsNullOrWhiteSpace(Input.Cidade))
            {
                Erro = "Nome, telefone e cidade são obrigatórios.";
                await CarregarAsync();
                return Page();
            }

            var (ok, erro) = await _api.CadastrarMarceneiroAdminAsync(Input);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível cadastrar.";
                await CarregarAsync();
                return Page();
            }

            Mensagem = $"Montador {Input.Nome} adicionado à rede.";
            Input = new();
            await CarregarAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostVerificarAsync(int id)
        {
            var (ok, erro) = await _api.VerificarMarceneiroAsync(id);
            Mensagem = ok ? "Montador homologado. Já entra na alocação automática." : null;
            Erro = ok ? null : (erro ?? "Não foi possível homologar.");
            await CarregarAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDisponibilidadeAsync(int id)
        {
            var (ok, erro) = await _api.AlternarDisponibilidadeMarceneiroAsync(id);
            Mensagem = ok ? "Disponibilidade atualizada." : null;
            Erro = ok ? null : (erro ?? "Não foi possível atualizar.");
            await CarregarAsync();
            return Page();
        }
    }
}
