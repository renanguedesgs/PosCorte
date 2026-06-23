namespace PosCorte.Domain.Entities
{
    /// <summary>
    /// Cobrança PIX vinculada a um projeto (Asaas ou modo stub em desenvolvimento).
    /// </summary>
    public class Pagamento
    {
        public int Id { get; set; }
        public int ProjetoId { get; set; }
        public string Modo { get; set; } = "Stub"; // Stub | Asaas
        public string Status { get; set; } = "Pendente"; // Pendente, Aguardando_Pix, Confirmado, Retido_Escrow, Liquidado, Cancelado, Expirado
        public string? AsaasPaymentId { get; set; }
        public string? AsaasCustomerId { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorMarceneiro { get; set; }
        public decimal ValorPlataforma { get; set; }
        public string? PixCopiaECola { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? InvoiceUrl { get; set; }
        public DateTime? ExpiraEm { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConfirmacao { get; set; }
    }
}
