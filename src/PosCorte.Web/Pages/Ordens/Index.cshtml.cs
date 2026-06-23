using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Ordens
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;

        public IndexModel(ApiService api) => _api = api;

        public List<OrdemViewModel> Ordens { get; set; } = new();
        public Dictionary<int, string> NomesProjetos { get; set; } = new();

        public async Task OnGetAsync()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var projetos = await _api.ListarProjetosAsync();

            if (int.TryParse(idClaim, out var uid))
                projetos = projetos.Where(p => p.UsuarioId == uid).ToList();

            NomesProjetos = projetos.ToDictionary(p => p.Id, p => p.NomeProjeto);
            var meusIds = NomesProjetos.Keys.ToHashSet();

            var ordens = await _api.ListarOrdensAsync();
            Ordens = ordens.Where(o => meusIds.Contains(o.ProjetoId))
                           .OrderByDescending(o => o.DataAtualizacao)
                           .ToList();
        }
    }
}
