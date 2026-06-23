using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ApiService _api;

        public DashboardModel(ApiService api) => _api = api;

        public List<ProjetoViewModel> Projetos { get; set; } = new();
        public List<OrdemViewModel> Ordens { get; set; } = new();
        public string NomeUsuario { get; set; } = "Arquiteto";

        public int TotalProjetos => Projetos.Count;
        public int AguardandoPagamento => Projetos.Count(p => p.StatusProjeto == "Aguardando_Pagamento");
        public int EmAndamento => Projetos.Count(p =>
            p.StatusProjeto is "Pagamento_Confirmado" or "Ordem_Criada" or "Prestador_Alocado" or "Aguardando_Vistoria");
        public int TotalOrdens => Ordens.Count;

        public async Task OnGetAsync()
        {
            NomeUsuario = User.Identity?.Name ?? "Arquiteto";

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var todos = await _api.ListarProjetosAsync();

            Projetos = int.TryParse(idClaim, out var uid)
                ? todos.Where(p => p.UsuarioId == uid).ToList()
                : todos;

            Ordens = await _api.ListarOrdensAsync();
            var meusIds = Projetos.Select(p => p.Id).ToHashSet();
            Ordens = Ordens.Where(o => meusIds.Contains(o.ProjetoId)).ToList();
        }
    }
}
