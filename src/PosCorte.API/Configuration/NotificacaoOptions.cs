namespace PosCorte.API.Configuration
{
    /// <summary>
    /// Configuração dos canais reais de notificação (WhatsApp + e-mail).
    /// Enquanto desabilitado, o <c>NotificacaoService</c> apenas registra em log.
    /// Nunca commitar tokens reais — preencher via variáveis de ambiente.
    /// </summary>
    public class NotificacaoOptions
    {
        public const string SectionName = "Notificacao";

        /// <summary>URL base do app web — usada para montar links em mensagens (ex.: onboarding).</summary>
        public string AppBaseUrl { get; set; } = "https://pos-corte.vercel.app";

        public WhatsAppOptions WhatsApp { get; set; } = new();
        public EmailOptions Email { get; set; } = new();
    }

    /// <summary>
    /// WhatsApp. Suporta a Cloud API oficial da Meta (provider="meta") e a Z-API (provider="zapi").
    /// </summary>
    public class WhatsAppOptions
    {
        public bool Enabled { get; set; }

        /// <summary>"meta" (WhatsApp Cloud API) ou "zapi".</summary>
        public string Provider { get; set; } = "meta";

        // ---- Meta WhatsApp Cloud API ----
        public string MetaBaseUrl { get; set; } = "https://graph.facebook.com/v21.0";
        public string MetaPhoneNumberId { get; set; } = string.Empty;
        public string MetaToken { get; set; } = string.Empty;

        // ---- Z-API (alternativa nacional) ----
        public string ZapiBaseUrl { get; set; } = "https://api.z-api.io";
        public string ZapiInstanceId { get; set; } = string.Empty;
        public string ZapiInstanceToken { get; set; } = string.Empty;
        public string ZapiClientToken { get; set; } = string.Empty;

        public bool EstaConfigurado =>
            Enabled &&
            (string.Equals(Provider, "meta", StringComparison.OrdinalIgnoreCase)
                ? (!string.IsNullOrWhiteSpace(MetaPhoneNumberId) && !string.IsNullOrWhiteSpace(MetaToken))
                : (!string.IsNullOrWhiteSpace(ZapiInstanceId) && !string.IsNullOrWhiteSpace(ZapiInstanceToken)));
    }

    /// <summary>E-mail transacional via SMTP (Resend, SendGrid, Brevo, Gmail App Password etc.).</summary>
    public class EmailOptions
    {
        public bool Enabled { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = "nao-responda@poscorte.com.br";
        public string FromName { get; set; } = "PósCorte";
        public bool UseSsl { get; set; } = true;

        public bool EstaConfigurado =>
            Enabled && !string.IsNullOrWhiteSpace(SmtpHost) && !string.IsNullOrWhiteSpace(From);
    }
}
