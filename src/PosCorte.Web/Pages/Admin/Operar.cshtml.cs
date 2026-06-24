using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OperarModel : PageModel
    {
        private readonly ApiService _api;

        public OperarModel(ApiService api) => _api = api;

        public ProjetoOperacaoAdminViewModel? Operacao { get; set; }
        public List<MarceneiroViewModel> Marceneiros { get; set; } = new();

        [BindProperty]
        public int MarceneiroId { get; set; }

        [BindProperty]
        public DateTime? DataAgendamento { get; set; }

        public string? Mensagem { get; set; }
        public string? Erro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Operacao = await _api.ObterProjetoOperacaoAdminAsync(id);
            if (Operacao == null) return NotFound();
            Marceneiros = await _api.ListarMarceneirosAsync(disponivel: true);
            return Page();
        }

        public async Task<IActionResult> OnPostAlocarAsync(int id)
        {
            var (ok, erro) = await _api.AlocarMontadorAdminAsync(id, MarceneiroId, DataAgendamento);
            if (!ok)
            {
                Erro = erro ?? "Falha ao alocar montador.";
                return await OnGetAsync(id);
            }
            Mensagem = "Montador alocado. Ele foi notificado (stub — veja logs da API).";
            return await OnGetAsync(id);
        }

        public async Task<IActionResult> OnPostConcluirMontagemAsync(int id)
        {
            var (ok, erro) = await _api.MarcarMontagemConcluidaAdminAsync(id);
            if (!ok)
            {
                Erro = erro ?? "Não foi possível marcar como concluída.";
                return await OnGetAsync(id);
            }
            Mensagem = "Montagem concluída. Arquiteto tem 72h para vistoriar.";
            return await OnGetAsync(id);
        }
    }
}
