using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PosCorte.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        private void SetAuthHeader()
        {
            var token = _ctx.HttpContext?.Session.GetString("jwt");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // ?? AUTH ????????????????????????????????????????????????
        public async Task<(bool ok, string? token, string? erro)> LoginAsync(string email, string senha)
        {
            var body = JsonSerializer.Serialize(new { email, senha });
            var res = await _http.PostAsync("api/v1/auth/login",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!res.IsSuccessStatusCode)
                return (false, null, "Email ou senha inválidos");

            var json = await res.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            return (true, obj.GetProperty("token").GetString(), null);
        }

        public async Task<(bool ok, string? erro)> RegisterAsync(string nome, string email, string cpfCnpj, string telefone, string senha)
        {
            var body = JsonSerializer.Serialize(new { nome, email, cpfCnpj, telefone, senha });
            var res = await _http.PostAsync("api/v1/auth/register",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                return (false, "Email já cadastrado");
            }
            return (true, null);
        }

        // ?? USUÁRIOS ?????????????????????????????????????????????
        public async Task<List<UsuarioViewModel>> ListarUsuariosAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/usuarios");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<UsuarioViewModel>>(json, _json) ?? new();
        }

        // ?? PROJETOS ?????????????????????????????????????????????
        public async Task<List<ProjetoViewModel>> ListarProjetosAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/projetos");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProjetoViewModel>>(json, _json) ?? new();
        }

        public async Task<ProjetoViewModel?> ObterProjetoAsync(int id)
        {
            SetAuthHeader();
            var res = await _http.GetAsync($"api/v1/projetos/{id}");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProjetoViewModel>(json, _json);
        }

        public async Task<(bool ok, string? erro)> CriarProjetoAsync(CriarProjetoInput input)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(input);
            var res = await _http.PostAsync("api/v1/projetos",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                return (false, await res.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<OrcamentoViewModel?> CalcularOrcamentoAsync(int projetoId, int pecas, int gavetas)
        {
            SetAuthHeader();
            var body = JsonSerializer.Serialize(new { qtdPecas = pecas, qtdGavetas = gavetas });
            var res = await _http.PostAsync($"api/v1/projetos/{projetoId}/calcular-orcamento",
                new StringContent(body, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OrcamentoViewModel>(json, _json);
        }

        // ?? ORDENS ???????????????????????????????????????????????
        public async Task<List<OrdemViewModel>> ListarOrdensAsync()
        {
            SetAuthHeader();
            var res = await _http.GetAsync("api/v1/ordens-servico");
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<OrdemViewModel>>(json, _json) ?? new();
        }
    }

    // ?? VIEW MODELS ???????????????????????????????????????????
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }

    public class ProjetoViewModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string UrlArquivoCorteCloud { get; set; } = string.Empty;
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
        public string StatusProjeto { get; set; } = string.Empty;
    }

    public class CriarProjetoInput
    {
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string UrlArquivoCorteCloud { get; set; } = string.Empty;
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
    }

    public class OrcamentoViewModel
    {
        public decimal ValorTotal { get; set; }
        public decimal CustoPrestador { get; set; }
        public decimal MargemLucro { get; set; }
        public decimal TaxaPlataforma { get; set; }
    }

    public class OrdemViewModel
    {
        public int Id { get; set; }
        public int ProjetoId { get; set; }
        public string ExternalProviderId { get; set; } = string.Empty;
        public string StatusProvedor { get; set; } = string.Empty;
        public string MontadorNome { get; set; } = string.Empty;
        public string MontadorTelefone { get; set; } = string.Empty;
        public DateTime DataAtualizacao { get; set; }
    }
}
