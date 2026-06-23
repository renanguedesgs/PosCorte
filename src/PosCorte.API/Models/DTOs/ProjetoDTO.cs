namespace PosCorte.API.Models.DTOs
{
    public class ProjetoDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string UrlArquivoCorteCloud { get; set; } = string.Empty;
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
        public string StatusProjeto { get; set; } = string.Empty;
        public DateTime? DataLimiteVistoria { get; set; }
        public string? MotivoDisputa { get; set; }
    }

    public class AbrirDisputaDTO
    {
        public string Motivo { get; set; } = string.Empty;
    }

    public class CreateProjetoDTO
    {
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string UrlArquivoCorteCloud { get; set; } = string.Empty;
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
    }

    public class OrcamentoRequest
    {
        public int? QtdPecas { get; set; }
        public int? QtdGavetas { get; set; }
    }
}
