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

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeProjeto))
            {
                Erro = "Informe o nome do projeto.";
                return Page();
            }

            if (QtdPecas <= 0 && QtdGavetas <= 0)
            {
                Erro = "Informe ao menos uma peça ou gaveta.";
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
                NomeProjeto = NomeProjeto,
                UrlArquivoCorteCloud = UrlArquivoCorteCloud,
                QtdPecas = QtdPecas,
                QtdGavetas = QtdGavetas,
                CepObra = CepObra,
                EnderecoCompleto = EnderecoCompleto
            };

            var (ok, erro) = await _api.CriarProjetoAsync(input);
            if (!ok)
            {
                Erro = "Não foi possível criar o projeto. " + erro;
                return Page();
            }

            TempData["sucesso"] = "Projeto criado com sucesso!";
            return RedirectToPage("/Projetos/Index");
        }
    }
}
