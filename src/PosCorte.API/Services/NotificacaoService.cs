using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosCorte.API.Configuration;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    /// <summary>
    /// Notificações reais com fallback automático para log.
    /// - WhatsApp: Meta Cloud API (oficial) ou Z-API, conforme <c>Notificacao:WhatsApp:Provider</c>.
    /// - E-mail: SMTP (Resend/SendGrid/Brevo/Gmail App Password).
    /// Enquanto as seções estiverem desabilitadas, apenas registra em log (modo stub).
    /// </summary>
    public class NotificacaoService : INotificacaoService
    {
        public const string HttpClientName = "notificacoes";

        private readonly NotificacaoOptions _opt;
        private readonly IHttpClientFactory _httpFactory;
        private readonly PosCorteDbContext _db;
        private readonly ILogger<NotificacaoService> _logger;

        public NotificacaoService(
            IOptions<NotificacaoOptions> opt,
            IHttpClientFactory httpFactory,
            PosCorteDbContext db,
            ILogger<NotificacaoService> logger)
        {
            _opt = opt.Value;
            _httpFactory = httpFactory;
            _db = db;
            _logger = logger;
        }

        public async Task NotificarEventoAsync(NotificacaoEvento evento, int projetoId, string mensagem)
        {
            _logger.LogInformation("[NOTIFICACAO] {Evento} — Projeto {ProjetoId} — {Mensagem}", evento, projetoId, mensagem);
            await Task.CompletedTask;
        }

        public async Task<bool> NotificarArquiteto(int usuarioId, string mensagem)
        {
            var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuarioId);
            if (usuario == null)
            {
                _logger.LogWarning("Notificar arquiteto: usuário {UsuarioId} não encontrado.", usuarioId);
                return false;
            }

            var enviouWhats = false;
            if (!string.IsNullOrWhiteSpace(usuario.Telefone))
                enviouWhats = await EnviarWhatsAppAsync(usuario.Telefone, mensagem);

            var enviouEmail = false;
            if (!string.IsNullOrWhiteSpace(usuario.Email))
                enviouEmail = await EnviarEmailAsync(usuario.Email, "PósCorte — atualização do seu projeto", mensagem);

            if (!enviouWhats && !enviouEmail)
                _logger.LogInformation("[STUB] Arquiteto {UsuarioId}: {Mensagem}", usuarioId, mensagem);

            return true;
        }

        public async Task<bool> NotificarMontador(string telefoneMontador, string mensagem)
        {
            if (await EnviarWhatsAppAsync(telefoneMontador, mensagem))
                return true;

            _logger.LogInformation("[STUB] Montador {Telefone}: {Mensagem}", telefoneMontador, mensagem);
            return true;
        }

        public async Task<bool> EnviarEmailConfirmacao(string email, string conteudo)
        {
            if (await EnviarEmailAsync(email, "PósCorte", conteudo))
                return true;

            _logger.LogInformation("[STUB] E-mail para {Email}: {Conteudo}", email, conteudo);
            return true;
        }

        // ===================== WHATSAPP =====================

        private async Task<bool> EnviarWhatsAppAsync(string telefone, string mensagem)
        {
            var wa = _opt.WhatsApp;
            if (!wa.EstaConfigurado) return false;

            var fone = NormalizarTelefoneBr(telefone);
            if (fone.Length < 10) return false;

            try
            {
                var client = _httpFactory.CreateClient(HttpClientName);

                if (string.Equals(wa.Provider, "zapi", StringComparison.OrdinalIgnoreCase))
                    return await EnviarZapiAsync(client, wa, fone, mensagem);

                return await EnviarMetaAsync(client, wa, fone, mensagem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar WhatsApp para {Telefone}", telefone);
                return false;
            }
        }

        private async Task<bool> EnviarMetaAsync(HttpClient client, WhatsAppOptions wa, string fone, string mensagem)
        {
            var url = $"{wa.MetaBaseUrl.TrimEnd('/')}/{wa.MetaPhoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = fone,
                type = "text",
                text = new { body = mensagem }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", wa.MetaToken);

            var res = await client.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("WhatsApp (Meta) retornou {Status}: {Body}", (int)res.StatusCode, body);
                return false;
            }

            _logger.LogInformation("WhatsApp (Meta) enviado para {Fone}", fone);
            return true;
        }

        private async Task<bool> EnviarZapiAsync(HttpClient client, WhatsAppOptions wa, string fone, string mensagem)
        {
            var url = $"{wa.ZapiBaseUrl.TrimEnd('/')}/instances/{wa.ZapiInstanceId}/token/{wa.ZapiInstanceToken}/send-text";
            var payload = new { phone = fone, message = mensagem };

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrWhiteSpace(wa.ZapiClientToken))
                req.Headers.Add("Client-Token", wa.ZapiClientToken);

            var res = await client.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("WhatsApp (Z-API) retornou {Status}: {Body}", (int)res.StatusCode, body);
                return false;
            }

            _logger.LogInformation("WhatsApp (Z-API) enviado para {Fone}", fone);
            return true;
        }

        // ===================== E-MAIL =====================

        private async Task<bool> EnviarEmailAsync(string destino, string assunto, string corpo)
        {
            var em = _opt.Email;
            if (!em.EstaConfigurado) return false;

            try
            {
                using var msg = new MailMessage
                {
                    From = new MailAddress(em.From, em.FromName),
                    Subject = assunto,
                    Body = corpo,
                    IsBodyHtml = false
                };
                msg.To.Add(destino);

                using var smtp = new SmtpClient(em.SmtpHost, em.SmtpPort)
                {
                    EnableSsl = em.UseSsl,
                    Credentials = string.IsNullOrWhiteSpace(em.Username)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(em.Username, em.Password)
                };

                await smtp.SendMailAsync(msg);
                _logger.LogInformation("E-mail enviado para {Destino}", destino);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar e-mail para {Destino}", destino);
                return false;
            }
        }

        // ===================== HELPERS =====================

        /// <summary>Normaliza para E.164 brasileiro (apenas dígitos com DDI 55).</summary>
        private static string NormalizarTelefoneBr(string telefone)
        {
            var digits = new string((telefone ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length == 0) return string.Empty;
            if (digits.StartsWith("55")) return digits;
            if (digits.Length is 10 or 11) return "55" + digits;
            return digits;
        }
    }
}
