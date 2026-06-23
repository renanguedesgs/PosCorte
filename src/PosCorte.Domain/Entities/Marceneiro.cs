namespace PosCorte.Domain.Entities
{
    /// <summary>
    /// Marceneiro/montador homologado da rede PósCorte.
    /// A rede própria de profissionais é o principal ativo do marketplace.
    /// </summary>
    public class Marceneiro
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;

        /// <summary>Especialidades separadas por vírgula (ex.: "Cozinha,Dormitório,Home Office").</summary>
        public string Especialidades { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;

        /// <summary>Nota média (0–5), recalculada a cada nova avaliação.</summary>
        public decimal NotaMedia { get; set; }
        public int TotalAvaliacoes { get; set; }
        public int TotalServicos { get; set; }

        public bool Disponivel { get; set; } = true;
        public bool Verificado { get; set; }

        /// <summary>Identificador da origem externa usado para evitar duplicatas no seed (ex.: "randomuser:uuid").</summary>
        public string OrigemExterna { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; }

        public Marceneiro()
        {
            DataCadastro = DateTime.UtcNow;
        }
    }
}
