namespace PosCorte.API.Models.DTOs
{
    public class AdminDashboardDTO
    {
        public int TotalArquitetos { get; set; }
        public int TotalMarceneiros { get; set; }
        public int TotalProjetos { get; set; }
        public int TotalOrdens { get; set; }
        public decimal ReceitaPlataformaEstimada { get; set; }
        public decimal VolumeTransacionadoEstimado { get; set; }
        public string GatewayPagamento { get; set; } = "Não integrado (stub)";
        public string StatusEscrow { get; set; } = "Simulado";
        public Dictionary<string, int> ProjetosPorStatus { get; set; } = new();
        public List<ProjetoResumoAdminDTO> ProjetosRecentes { get; set; } = new();
    }

    public class ProjetoResumoAdminDTO
    {
        public int Id { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string StatusProjeto { get; set; } = string.Empty;
        public string ArquitetoNome { get; set; } = string.Empty;
        public decimal ValorEstimado { get; set; }
        public decimal MargemEstimada { get; set; }
        public string? MontadorNome { get; set; }
    }
}
