namespace PosCorte.Web.Models
{
    public class MarceneiroMapaViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public int? MarceneiroId { get; set; }
        public bool Compact { get; set; }

        /// <summary>CEP da obra — habilita trajeto quando montador aceitou.</summary>
        public string? ObraCep { get; set; }
        public string? ObraEndereco { get; set; }
        public bool MostrarTrajeto { get; set; }
    }
}
