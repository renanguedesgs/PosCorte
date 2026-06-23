namespace PosCorte.Domain.Entities
{
    /// <summary>
    /// Registro de split/liquidação após vistoria (80% marceneiro, 20% plataforma).
    /// </summary>
    public class Liquidacao
    {
        public int Id { get; set; }
        public int PagamentoId { get; set; }
        public int ProjetoId { get; set; }
        public decimal ValorMarceneiro { get; set; }
        public decimal ValorPlataforma { get; set; }
        public string Status { get; set; } = "Pendente"; // Pendente, Processando, Concluida, Falha
        public string? AsaasSplitId { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }
    }
}
