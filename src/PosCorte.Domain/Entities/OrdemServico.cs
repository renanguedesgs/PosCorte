namespace PosCorte.Domain.Entities
{
    public class OrdemServico
    {
        public int Id { get; set; }
        public int ProjetoId { get; set; }
        public string ExternalProviderId { get; set; } = string.Empty;
        public string StatusProvedor { get; set; } = string.Empty;
        public string MontadorNome { get; set; } = string.Empty;
        public string MontadorTelefone { get; set; } = string.Empty;
        public string MontadorFotoUrl { get; set; } = string.Empty;
        public DateTime DataAgendamento { get; set; }
        public DateTime DataAtualizacao { get; set; }

        public OrdemServico() { }

        public OrdemServico(int projetoId, string externalProviderId)
        {
            ProjetoId = projetoId;
            ExternalProviderId = externalProviderId;
            StatusProvedor = "Pendente";
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
