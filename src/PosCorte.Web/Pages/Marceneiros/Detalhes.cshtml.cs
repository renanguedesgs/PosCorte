using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Marceneiros
{
    [Authorize]
    public class DetalhesModel : PageModel
    {
        private readonly ApiService _api;

        public DetalhesModel(ApiService api) => _api = api;

        public MarceneiroDetalheViewModel? Marceneiro { get; set; }

        [BindProperty] public int Nota { get; set; }
        [BindProperty] public string Comentario { get; set; } = string.Empty;

        public string? Mensagem { get; set; }
        public string? Erro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Marceneiro = await _api.ObterMarceneiroAsync(id);
            if (Marceneiro is null)
                return RedirectToPage("/Marceneiros/Index");

            if (TempData["msg"] is string m) Mensagem = m;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (Nota < 1 || Nota > 5)
            {
                Marceneiro = await _api.ObterMarceneiroAsync(id);
                Erro = "Escolha uma nota de 1 a 5 estrelas.";
                return Page();
            }

            var (ok, erro) = await _api.AvaliarMarceneiroAsync(id, Nota, Comentario);
            if (!ok)
            {
                Marceneiro = await _api.ObterMarceneiroAsync(id);
                Erro = "Não foi possível enviar a avaliação. " + erro;
                return Page();
            }

            TempData["msg"] = "Avaliação enviada com sucesso!";
            return RedirectToPage(new { id });
        }
    }
}
