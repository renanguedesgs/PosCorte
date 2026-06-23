using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PosCorte.API.Configuration;

namespace PosCorte.API.Services.Pagamentos.Asaas
{
    public class AsaasClient : IAsaasClient
    {
        private readonly HttpClient _http;
        private readonly AsaasOptions _options;
        private readonly ILogger<AsaasClient> _logger;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public AsaasClient(HttpClient http, IOptions<AsaasOptions> options, ILogger<AsaasClient> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AsaasCustomerResponse> CriarOuObterClienteAsync(AsaasCustomerRequest request, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("customers", request, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Asaas criar cliente falhou: {Status} {Body}", resp.StatusCode, body);
                throw new InvalidOperationException($"Asaas: falha ao criar cliente ({resp.StatusCode})");
            }
            return JsonSerializer.Deserialize<AsaasCustomerResponse>(body, JsonOpts)
                   ?? throw new InvalidOperationException("Resposta inválida do Asaas (cliente)");
        }

        public async Task<AsaasPaymentResponse> CriarCobrancaPixAsync(AsaasPaymentRequest request, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("payments", request, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Asaas criar cobrança falhou: {Status} {Body}", resp.StatusCode, body);
                throw new InvalidOperationException($"Asaas: falha ao criar cobrança PIX ({resp.StatusCode})");
            }
            return JsonSerializer.Deserialize<AsaasPaymentResponse>(body, JsonOpts)
                   ?? throw new InvalidOperationException("Resposta inválida do Asaas (payment)");
        }

        public async Task<AsaasPaymentResponse?> ObterCobrancaAsync(string paymentId, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"payments/{paymentId}", ct);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Asaas obter cobrança falhou: {Status} {Body}", resp.StatusCode, body);
                return null;
            }
            return JsonSerializer.Deserialize<AsaasPaymentResponse>(body, JsonOpts);
        }

        public async Task<AsaasPixQrCodeResponse?> ObterQrCodePixAsync(string paymentId, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"payments/{paymentId}/pixQrCode", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Asaas QR PIX falhou: {Status} {Body}", resp.StatusCode, body);
                return null;
            }
            return JsonSerializer.Deserialize<AsaasPixQrCodeResponse>(body, JsonOpts);
        }
    }

    public static class AsaasHttpClientExtensions
    {
        public static IHttpClientBuilder AddAsaasClient(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AsaasOptions>(config.GetSection(AsaasOptions.SectionName));

            return services.AddHttpClient<IAsaasClient, AsaasClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AsaasOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(30);
                if (!string.IsNullOrWhiteSpace(opts.ApiKey))
                    client.DefaultRequestHeaders.Add("access_token", opts.ApiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });
        }
    }
}
