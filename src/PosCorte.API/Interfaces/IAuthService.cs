namespace PosCorte.API.Interfaces
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(string email, string senha);
        Task<bool> RegisterAsync(string nome, string email, string cpfCnpj, string telefone, string senha);
    }
}
