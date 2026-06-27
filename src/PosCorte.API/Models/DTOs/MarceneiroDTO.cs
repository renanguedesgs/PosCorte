using System.ComponentModel.DataAnnotations;

namespace PosCorte.API.Models.DTOs
{
    public class MarceneiroDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public List<string> Especialidades { get; set; } = new();
        public string Bio { get; set; } = string.Empty;
        public decimal NotaMedia { get; set; }
        public int TotalAvaliacoes { get; set; }
        public int TotalServicos { get; set; }
        public bool Disponivel { get; set; }
        public bool Verificado { get; set; }
    }

    public class MarceneiroDetalheDTO : MarceneiroDTO
    {
        public List<AvaliacaoDTO> Avaliacoes { get; set; } = new();
    }

    public class AvaliacaoDTO
    {
        public int Id { get; set; }
        public int MarceneiroId { get; set; }
        public int? ProjetoId { get; set; }
        public string AutorNome { get; set; } = string.Empty;
        public int Nota { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }

    public class CreateAvaliacaoDTO
    {
        [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5.")]
        public int Nota { get; set; }

        [MaxLength(1000)]
        public string Comentario { get; set; } = string.Empty;

        public int? ProjetoId { get; set; }
    }

    public class SeedMarceneirosDTO
    {
        [Range(1, 200)]
        public int Quantidade { get; set; } = 30;
    }

    /// <summary>
    /// Auto-cadastro público de montador (a própria pessoa se cadastra pelo link).
    /// Entra na rede como pendente (Verificado=false, Disponivel=false) até a aprovação.
    /// </summary>
    public class AutoCadastroMarceneiroDTO
    {
        [Required(ErrorMessage = "Informe seu nome."), MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu WhatsApp."), MaxLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "E-mail inválido."), MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua cidade."), MaxLength(120)]
        public string Cidade { get; set; } = string.Empty;

        [MaxLength(60)]
        public string Estado { get; set; } = "SP";

        [MaxLength(120)]
        public string Bairro { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Cep { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Especialidades { get; set; } = string.Empty;

        [MaxLength(600)]
        public string Bio { get; set; } = string.Empty;
    }

    public enum ResultadoAutoCadastro
    {
        Ok,
        Duplicado,
        DadosInvalidos
    }

    /// <summary>Visão administrativa do montador, incluindo origem e contato para a fila de aprovação.</summary>
    public class MarceneiroAdminDTO : MarceneiroDTO
    {
        public string Email { get; set; } = string.Empty;
        public string OrigemExterna { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
    }
}
