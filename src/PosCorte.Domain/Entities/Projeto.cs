namespace PosCorte.Domain.Entities
{
    public class Projeto
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
        public DateTime DataCriacao { get; set; }

        public Projeto() { }

        public Projeto(int usuarioId, string nomeProjeto, string urlArquivo,
                       int qtdPecas, int qtdGavetas, string cep, string endereco)
        {
            UsuarioId = usuarioId;
            NomeProjeto = nomeProjeto;
            UrlArquivoCorteCloud = urlArquivo;
            QtdPecas = qtdPecas;
            QtdGavetas = qtdGavetas;
            CepObra = cep;
            EnderecoCompleto = endereco;
            StatusProjeto = "Aguardando_Pagamento";
            DataCriacao = DateTime.UtcNow;
        }
    }
}
