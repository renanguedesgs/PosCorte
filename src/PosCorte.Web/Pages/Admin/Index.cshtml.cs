using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;

        public IndexModel(ApiService api) => _api = api;

        public AdminDashboardViewModel? Dashboard { get; set; }

        public async Task OnGetAsync()
        {
            Dashboard = await _api.ObterAdminDashboardAsync();
        }
    }
}
