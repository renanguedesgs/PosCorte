namespace PosCorte.Domain.Entities
{
    /// <summary>
    /// Avaliação de um marceneiro feita pelo arquiteto após a montagem.
    /// </summary>
    public class Avaliacao
    {
        public int Id { get; set; }
        public int MarceneiroId { get; set; }
        public int? ProjetoId { get; set; }
        public string AutorNome { get; set; } = string.Empty;

        /// <summary>Nota de 1 a 5 estrelas.</summary>
        public int Nota { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }

        public Avaliacao()
        {
            DataCriacao = DateTime.UtcNow;
        }
    }
}
