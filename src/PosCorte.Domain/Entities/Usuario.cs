namespace PosCorte.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }

        public Usuario() { }

        public Usuario(string nome, string email, string cpfCnpj, string telefone)
        {
            Nome = nome;
            Email = email;
            CpfCnpj = cpfCnpj;
            Telefone = telefone;
            DataCadastro = DateTime.UtcNow;
        }
    }
}
