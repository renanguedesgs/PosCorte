using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PosCorte.Web.Services;

namespace PosCorte.Web.Pages.Projetos
{
    [Authorize]
    public class CriarModel : PageModel
    {
        private readonly ApiService _api;

        public CriarModel(ApiService api) => _api = api;

        [BindProperty] public string NomeProjeto { get; set; } = string.Empty;
        [BindProperty] public string UrlArquivoCorteCloud { get; set; } = string.Empty;
        [BindProperty] public int QtdPecas { get; set; }
        [BindProperty] public int QtdGavetas { get; set; }
        [BindProperty] public string CepObra { get; set; } = string.Empty;
        [BindProperty] public string EnderecoCompleto { get; set; } = string.Empty;

        public string? Erro { get; set; }
        public bool Onboarding { get; set; }

        public void OnGet(bool? onboarding) => Onboarding = onboarding == true;

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeProjeto))
            {
                Erro = "Informe o nome do projeto.";
                return Page();
            }

            if (QtdPecas <= 0 && QtdGavetas <= 0)
            {
                Erro = "Informe ao menos uma peça ou uma gaveta do plano de corte.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(UrlArquivoCorteCloud) ||
                !Uri.TryCreate(UrlArquivoCorteCloud.Trim(), UriKind.Absolute, out var uri) ||
                uri.Scheme is not ("http" or "https"))
            {
                Erro = "Cole o link de compartilhamento válido do Corte Cloud (https://...).";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(CepObra) || string.IsNullOrWhiteSpace(EnderecoCompleto))
            {
                Erro = "CEP e endereço da obra são obrigatórios para alocar o montador.";
                return Page();
            }

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var uid))
            {
                Erro = "Sessão inválida. Faça login novamente.";
                return Page();
            }

            var input = new CriarProjetoInput
            {
                UsuarioId = uid,
                NomeProjeto = NomeProjeto.Trim(),
                UrlArquivoCorteCloud = UrlArquivoCorteCloud.Trim(),
                QtdPecas = QtdPecas,
                QtdGavetas = QtdGavetas,
                CepObra = CepObra.Trim(),
                EnderecoCompleto = EnderecoCompleto.Trim()
            };

            var (ok, erro, projetoId) = await _api.CriarProjetoAsync(input);
            if (!ok)
            {
                Erro = "Não foi possível criar o projeto. " + erro;
                return Page();
            }

            if (projetoId.HasValue)
                return RedirectToPage("/Projetos/Pagar", new { id = projetoId.Value });

            TempData["sucesso"] = "Projeto criado! Agora pague com PIX para despachar o montador.";
            return RedirectToPage("/Projetos/Index");
        }
    }
}
