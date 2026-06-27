using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Interfaces
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(string email, string senha);
        Task<(bool ok, string? erro)> RegisterAsync(string nome, string email, string cpfCnpj, string telefone, string senha);
        Task<(bool ok, string? erro)> AlterarSenhaAsync(int usuarioId, string senhaAtual, string senhaNova);
        Task<UsuarioPerfilDTO?> ObterPerfilAsync(int usuarioId);
    }
}
