using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Projetos
{
    [Authorize]
    public class DetalhesModel : PageModel
    {
        private readonly ApiService _api;
        private readonly IWebHostEnvironment _env;

        public DetalhesModel(ApiService api, IWebHostEnvironment env)
        {
            _api = api;
            _env = env;
        }

        public ProjetoViewModel? Projeto { get; set; }
        public OrcamentoViewModel? Orcamento { get; set; }
        public List<OrdemViewModel> Ordens { get; set; } = new();
        public MarceneiroDetalheViewModel? MontadorAlocado { get; set; }
        public bool MostrarTrajetoMontador { get; set; }
        public bool IsDevelopment => _env.IsDevelopment();

        [TempData] public string? Mensagem { get; set; }
        [TempData] public string? Erro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await CarregarAsync(id);
            if (Projeto is null) return RedirectToPage("/Projetos/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAprovarAsync(int id)
        {
            var (ok, erro) = await _api.AprovarMontagemAsync(id);
            if (ok) Mensagem = "Montagem aprovada! O pagamento foi liberado ao montador.";
            else Erro = "Não foi possível aprovar a montagem. " + erro;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDisputaAsync(int id, string motivo)
        {
            var (ok, erro) = await _api.AbrirDisputaAsync(id, motivo ?? "");
            if (ok) Mensagem = "Disputa registrada. O valor permanece retido em escrow até a resolução.";
            else Erro = "Não foi possível abrir a disputa. " + erro;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostSimularConclusaoAsync(int id)
        {
            if (!_env.IsDevelopment()) return RedirectToPage(new { id });
            var (ok, erro) = await _api.SimularConclusaoMontagemAsync(id);
            if (ok) Mensagem = "Montagem marcada como concluída. Janela de vistoria iniciada.";
            else Erro = "Falha ao simular conclusão. " + erro;
            return RedirectToPage(new { id });
        }

        private async Task CarregarAsync(int id)
        {
            Projeto = await _api.ObterProjetoAsync(id);
            if (Projeto is null) return;

            Orcamento = await _api.CalcularOrcamentoAsync(id, Projeto.QtdPecas, Projeto.QtdGavetas);

            var ordens = await _api.ListarOrdensAsync();
            Ordens = ordens.Where(o => o.ProjetoId == id).ToList();

            var ordemComMontador = Ordens.FirstOrDefault(o => !string.IsNullOrEmpty(o.MontadorNome));
            if (ordemComMontador != null)
            {
                var mcId = UiHelper.ParseMarceneiroIdFromOrdem(ordemComMontador.ExternalProviderId);
                if (mcId.HasValue)
                    MontadorAlocado = await _api.ObterMarceneiroAsync(mcId.Value);

                MostrarTrajetoMontador = ordemComMontador.StatusProvedor is "Prestador_Alocado" or "Aceito" or "Concluido"
                    && !string.IsNullOrWhiteSpace(Projeto?.CepObra);
            }
        }
    }
}
