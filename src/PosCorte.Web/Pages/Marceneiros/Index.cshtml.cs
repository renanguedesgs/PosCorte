using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PosCorte.Web.Pages.Marceneiros
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Admin/Marceneiros");
    }
}
