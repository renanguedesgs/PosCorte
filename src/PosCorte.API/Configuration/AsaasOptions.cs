namespace PosCorte.API.Configuration
{
    /// <summary>
    /// Configuração do gateway Asaas. Deixe Enabled=false e ApiKey vazio até ter CNPJ/conta própria.
    /// </summary>
    public class AsaasOptions
    {
        public const string SectionName = "Asaas";

        /// <summary>Sandbox: https://sandbox.asaas.com/api/v3 | Produção: https://api.asaas.com/api/v3</summary>
        public string BaseUrl { get; set; } = "https://sandbox.asaas.com/api/v3";

        /// <summary>Chave API do painel Asaas (NUNCA commitar valor real).</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Token para validar webhooks (configurar no painel Asaas).</summary>
        public string WebhookToken { get; set; } = string.Empty;

        /// <summary>true = chama API Asaas. false = modo stub (sem cobrança real).</summary>
        public bool Enabled { get; set; }

        /// <summary>Dias até vencimento da cobrança PIX gerada.</summary>
        public int DiasVencimentoPix { get; set; } = 1;

        public bool EstaConfigurado =>
            Enabled && !string.IsNullOrWhiteSpace(ApiKey);
    }
}
