using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ProjetosModel : PageModel
    {
        private readonly ApiService _api;

        public ProjetosModel(ApiService api) => _api = api;

        public List<ProjetoViewModel> Projetos { get; set; } = new();

        public async Task OnGetAsync()
        {
            Projetos = await _api.ListarProjetosAsync();
            Projetos = Projetos.OrderByDescending(p => p.Id).ToList();
        }
    }
}
