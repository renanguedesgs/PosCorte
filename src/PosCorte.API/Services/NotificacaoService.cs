using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly ILogger<NotificacaoService> _logger;

        public NotificacaoService(ILogger<NotificacaoService> logger)
        {
            _logger = logger;
        }

        public async Task NotificarEventoAsync(NotificacaoEvento evento, int projetoId, string mensagem)
        {
            // STUB: hoje apenas registra. Para ativar WhatsApp (Z-API/Twilio) e e-mail (Resend/SendGrid),
            // implemente o envio aqui usando as credenciais documentadas em docs/INTEGRACAO_NOTIFICACOES.md.
            _logger.LogInformation("[NOTIFICACAO] {Evento} � Projeto {ProjetoId} � {Mensagem}", evento, projetoId, mensagem);
            await Task.CompletedTask;
        }

        public async Task<bool> NotificarArquiteto(int usuarioId, string mensagem)
        {
            _logger.LogInformation("Notificando arquiteto {UsuarioId}: {Mensagem}", usuarioId, mensagem);

            try
            {
                // TODO: Integrar com servi�o de notifica��o (Push, Email, SMS)
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar arquiteto");
                return false;
            }
        }

        public async Task<bool> NotificarMontador(string telefoneMontador, string mensagem)
        {
            _logger.LogInformation("Notificando montador {Telefone}: {Mensagem}", telefoneMontador, mensagem);

            try
            {
                // TODO: Integrar com servi�o de SMS/WhatsApp
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar montador");
                return false;
            }
        }

        public async Task<bool> EnviarEmailConfirmacao(string email, string conteudo)
        {
            _logger.LogInformation("Enviando email para: {Email}", email);

            try
            {
                // TODO: Integrar com servi�o de email (SendGrid, Mailgun, etc)
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email");
                return false;
            }
        }
    }
}
