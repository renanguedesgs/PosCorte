using System.ComponentModel.DataAnnotations;

namespace PosCorte.API.Models.DTOs
{
    public class LoginDTO
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Senha { get; set; } = string.Empty;
    }

    public class RegisterDTO
    {
        [Required] public string Nome { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string CpfCnpj { get; set; } = string.Empty;
        [Required] public string Telefone { get; set; } = string.Empty;
        [Required, MinLength(8)] public string Senha { get; set; } = string.Empty;
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Expiracao { get; set; }
    }

    public class UsuarioPerfilDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
    }
}
