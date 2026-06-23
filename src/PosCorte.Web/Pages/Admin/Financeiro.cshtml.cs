using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PosCorte.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class FinanceiroModel : PageModel
    {
        public void OnGet() { }
    }
}
