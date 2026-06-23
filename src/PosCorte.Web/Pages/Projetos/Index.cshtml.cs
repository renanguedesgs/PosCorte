using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Projetos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;

        public IndexModel(ApiService api) => _api = api;

        public List<ProjetoViewModel> Projetos { get; set; } = new();

        public async Task OnGetAsync()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var todos = await _api.ListarProjetosAsync();

            Projetos = int.TryParse(idClaim, out var uid)
                ? todos.Where(p => p.UsuarioId == uid).OrderByDescending(p => p.Id).ToList()
                : todos.OrderByDescending(p => p.Id).ToList();
        }
    }
}
