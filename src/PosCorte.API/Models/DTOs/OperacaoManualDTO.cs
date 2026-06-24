using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PosCorte.API.Models.DTOs
{
    public class CreateMarceneiroAdminDTO
    {
        [Required, MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Cidade { get; set; } = string.Empty;

        [MaxLength(60)]
        public string Estado { get; set; } = "SP";

        [MaxLength(120)]
        public string Bairro { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Cep { get; set; } = string.Empty;

        /// <summary>Ex.: "Cozinha,Dormitório" ou texto livre.</summary>
        [MaxLength(300)]
        public string Especialidades { get; set; } = string.Empty;

        [MaxLength(600)]
        public string Bio { get; set; } = string.Empty;

        public bool Verificado { get; set; } = true;
        public bool Disponivel { get; set; } = true;
    }

    public class CreateArquitetoAdminDTO
    {
        [Required, MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(18)]
        public string CpfCnpj { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefone { get; set; } = string.Empty;

        /// <summary>Se vazio, o sistema gera uma senha temporária.</summary>
        [MinLength(6)]
        public string? Senha { get; set; }
    }

    public class ArquitetoAdminDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }

    public class CreateArquitetoAdminResponseDTO
    {
        public ArquitetoAdminDTO Arquiteto { get; set; } = new();
        public string SenhaInicial { get; set; } = string.Empty;
    }

    public class AlocarMontadorDTO
    {
        [Required]
        public int MarceneiroId { get; set; }

        public DateTime? DataAgendamento { get; set; }
    }

    public class ProjetoOperacaoAdminDTO
    {
        public ProjetoDTO Projeto { get; set; } = new();
        public string ArquitetoNome { get; set; } = string.Empty;
        public string ArquitetoEmail { get; set; } = string.Empty;
        public string ArquitetoTelefone { get; set; } = string.Empty;
        public OrcamentoResultado? Orcamento { get; set; }
        public OrdemOperacaoAdminDTO? Ordem { get; set; }
        public PagamentoResumoAdminDTO? Pagamento { get; set; }
    }

    public class OrdemOperacaoAdminDTO
    {
        public int Id { get; set; }
        public string StatusProvedor { get; set; } = string.Empty;
        public string MontadorNome { get; set; } = string.Empty;
        public string MontadorTelefone { get; set; } = string.Empty;
        public DateTime DataAgendamento { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }

    public class PagamentoResumoAdminDTO
    {
        public string Status { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public decimal ValorMarceneiro { get; set; }
    }

    public class AlterarSenhaDTO
    {
        [Required]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string SenhaNova { get; set; } = string.Empty;
    }
}
