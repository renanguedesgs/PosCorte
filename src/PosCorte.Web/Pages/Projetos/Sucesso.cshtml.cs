using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Projetos
{
    [Authorize]
    public class SucessoModel : PageModel
    {
        private readonly ApiService _api;

        public SucessoModel(ApiService api) => _api = api;

        public ProjetoViewModel? Projeto { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Projeto = await _api.ObterProjetoAsync(id);
            if (Projeto is null) return RedirectToPage("/Projetos/Index");

            // Só mostra celebração se pagamento já foi confirmado ou projeto em fluxo post-pagamento.
            var statusOk = Projeto.StatusProjeto is not "Aguardando_Pagamento";
            if (!statusOk)
                return RedirectToPage("/Projetos/Pagar", new { id });

            return Page();
        }
    }
}
