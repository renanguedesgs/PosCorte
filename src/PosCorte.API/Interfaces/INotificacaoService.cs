namespace PosCorte.API.Interfaces
{
    /// <summary>Eventos de negócio que disparam notificações (WhatsApp/e-mail quando configurado).</summary>
    public enum NotificacaoEvento
    {
        ProjetoCriado,
        PagamentoConfirmado,
        MontadorAlocado,
        MontagemConcluida,
        DisputaAberta
    }

    public interface INotificacaoService
    {
        /// <summary>
        /// Ponto único de notificação por evento de negócio. Hoje registra em log (stub);
        /// ao configurar WhatsApp/e-mail, só este método precisa ganhar a integração real.
        /// </summary>
        Task NotificarEventoAsync(NotificacaoEvento evento, int projetoId, string mensagem);

        Task<bool> NotificarArquiteto(int usuarioId, string mensagem);
        Task<bool> NotificarMontador(string telefoneMontador, string mensagem);
        Task<bool> EnviarEmailConfirmacao(string email, string conteudo);
    }
}
