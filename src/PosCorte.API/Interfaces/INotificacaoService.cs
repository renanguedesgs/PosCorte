namespace PosCorte.API.Interfaces
{
    public interface INotificacaoService
    {
        Task<bool> NotificarArquiteto(int usuarioId, string mensagem);
        Task<bool> NotificarMontador(string telefoneMontador, string mensagem);
        Task<bool> EnviarEmailConfirmacao(string email, string conteudo);
    }
}
