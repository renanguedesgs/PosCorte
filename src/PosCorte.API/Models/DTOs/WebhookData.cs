namespace PosCorte.API.Models.DTOs
{
    public class WebhookData
    {
        public string IdExternalProviderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string NomeMontador { get; set; } = string.Empty;
        public string TelefoneMontador { get; set; } = string.Empty;
        public string FotoMontadorUrl { get; set; } = string.Empty;
        public DateTime DataRetorno { get; set; }
    }

    public class WebhookPagamento
    {
        public int ProjetoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PixId { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }
}
