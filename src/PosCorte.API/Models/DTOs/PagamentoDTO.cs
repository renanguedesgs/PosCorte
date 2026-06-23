namespace PosCorte.API.Models.DTOs
{
    public class GerarPixResponseDTO
    {
        public int PagamentoId { get; set; }
        public int ProjetoId { get; set; }
        public string Modo { get; set; } = "Stub";
        public string Status { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public decimal ValorMarceneiro { get; set; }
        public decimal ValorPlataforma { get; set; }
        public string? PixCopiaECola { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? InvoiceUrl { get; set; }
        public DateTime? ExpiraEm { get; set; }
        public bool GatewayConfigurado { get; set; }
        public string? Aviso { get; set; }
    }

    public class PagamentoStatusDTO
    {
        public int PagamentoId { get; set; }
        public int ProjetoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Modo { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public string StatusProjeto { get; set; } = string.Empty;
    }
}
