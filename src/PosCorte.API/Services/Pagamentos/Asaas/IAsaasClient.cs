using System.Text.Json.Serialization;

namespace PosCorte.API.Services.Pagamentos.Asaas
{
    public interface IAsaasClient
    {
        Task<AsaasCustomerResponse> CriarOuObterClienteAsync(AsaasCustomerRequest request, CancellationToken ct = default);
        Task<AsaasPaymentResponse> CriarCobrancaPixAsync(AsaasPaymentRequest request, CancellationToken ct = default);
        Task<AsaasPaymentResponse?> ObterCobrancaAsync(string paymentId, CancellationToken ct = default);
        Task<AsaasPixQrCodeResponse?> ObterQrCodePixAsync(string paymentId, CancellationToken ct = default);
    }

    public class AsaasCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? MobilePhone { get; set; }
        public string? ExternalReference { get; set; }
    }

    public class AsaasCustomerResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class AsaasPaymentRequest
    {
        public string Customer { get; set; } = string.Empty;
        public string BillingType { get; set; } = "PIX";
        public decimal Value { get; set; }
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }
        public string? ExternalReference { get; set; }
    }

    public class AsaasPaymentResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("invoiceUrl")]
        public string? InvoiceUrl { get; set; }

        [JsonPropertyName("dueDate")]
        public string? DueDate { get; set; }
    }

    public class AsaasPixQrCodeResponse
    {
        [JsonPropertyName("encodedImage")]
        public string? EncodedImage { get; set; }

        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        [JsonPropertyName("expirationDate")]
        public string? ExpirationDate { get; set; }
    }

    public class AsaasWebhookPayload
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("payment")]
        public AsaasWebhookPayment? Payment { get; set; }
    }

    public class AsaasWebhookPayment
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("externalReference")]
        public string? ExternalReference { get; set; }
    }
}
