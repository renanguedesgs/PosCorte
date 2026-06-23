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
        public string? Mensagem { get; set; }

        public async Task OnGetAsync()
        {
            Marceneiros = await _api.ListarMarceneirosAsync();
        }
    }
}
