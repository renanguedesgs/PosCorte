using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Projetos
{
    [Authorize]
    public class PagarModel : PageModel
    {
        private readonly ApiService _api;
        private readonly IWebHostEnvironment _env;

        public PagarModel(ApiService api, IWebHostEnvironment env)
        {
            _api = api;
            _env = env;
        }

        public ProjetoViewModel? Projeto { get; set; }
        public GerarPixViewModel? Pix { get; set; }
        public string? Erro { get; set; }
        public string? Sucesso { get; set; }
        public bool IsDevelopment => _env.IsDevelopment();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Projeto = await _api.ObterProjetoAsync(id);
            if (Projeto is null) return RedirectToPage("/Projetos/Index");

            if (Projeto.StatusProjeto != "Aguardando_Pagamento")
                return RedirectToPage("/Projetos/Detalhes", new { id });

            Pix = await _api.GerarPixAsync(id);
            if (Pix is null)
                Erro = "Não foi possível gerar o PIX. Tente novamente.";

            return Page();
        }

        public async Task<IActionResult> OnPostSimularAsync(int id, int pagamentoId)
        {
            if (!_env.IsDevelopment())
                return RedirectToPage(new { id });

            var (ok, erro) = await _api.SimularPagamentoAsync(pagamentoId);
            if (!ok)
            {
                Erro = "Falha na simulação. " + erro;
                return await OnGetAsync(id);
            }

            return RedirectToPage("/Projetos/Sucesso", new { id });
        }

        public async Task<IActionResult> OnGetStatusAsync(int id)
        {
            var status = await _api.ObterStatusPagamentoAsync(id);
            if (status?.Status == "Confirmado" || status?.StatusProjeto == "Aguardando_Provedor")
                return new JsonResult(new { pago = true, redirect = Url.Page("/Projetos/Sucesso", new { id }) });

            return new JsonResult(new { pago = false });
        }
    }
}
