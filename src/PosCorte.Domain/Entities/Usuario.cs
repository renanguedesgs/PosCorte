namespace PosCorte.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Arquiteto";
        public bool Ativo { get; set; } = true;
        public DateTime DataCadastro { get; set; }

        public Usuario() { }

        public Usuario(string nome, string email, string cpfCnpj, string telefone)
        {
            Nome = nome;
            Email = email;
            CpfCnpj = cpfCnpj;
            Telefone = telefone;
            Role = "Arquiteto";
            Ativo = true;
            DataCadastro = DateTime.UtcNow;
        }
    }
}
