using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/ordens-servico")]
    [Produces("application/json")]
    public class OrdensServicoController : ControllerBase
    {
        private readonly ILogger<OrdensServicoController> _logger;
        private readonly IRepositorio<OrdemServico> _ordemRepo;
        private readonly IRepositorio<Projeto> _projetoRepo;

        public OrdensServicoController(
            ILogger<OrdensServicoController> logger,
            IRepositorio<OrdemServico> ordemRepo,
            IRepositorio<Projeto> projetoRepo)
        {
            _logger = logger;
            _ordemRepo = ordemRepo;
            _projetoRepo = projetoRepo;
        }

        /// <summary>
        /// Listar todas as ordens de serviço
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrdemServicoDTO>>> ListarOrdens()
        {
            var ordens = await _ordemRepo.GetAllAsync();
            return Ok(ordens.Select(MapToDTO));
        }

        /// <summary>
        /// Obter ordem de serviço por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdemServicoDTO>> ObterOrdem(int id)
        {
            var ordem = await _ordemRepo.GetByIdAsync(id);

            if (ordem == null)
                return NotFound(new { error = "Ordem de serviço não encontrada" });

            return Ok(MapToDTO(ordem));
        }

        /// <summary>
        /// Listar ordens de serviço de um projeto
        /// </summary>
        [HttpGet("projeto/{projetoId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrdemServicoDTO>>> ListarOrdensPorProjeto(int projetoId)
        {
            var projeto = await _projetoRepo.GetByIdAsync(projetoId);
            if (projeto == null)
                return NotFound(new { error = "Projeto não encontrado" });

            var ordens = await _ordemRepo.GetAllAsync();
            var ordensDoProjeto = ordens.Where(o => o.ProjetoId == projetoId);

            return Ok(ordensDoProjeto.Select(MapToDTO));
        }

        /// <summary>
        /// Cancelar ordem de serviço
        /// </summary>
        [HttpPatch("{id}/cancelar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelarOrdem(int id)
        {
            var ordem = await _ordemRepo.GetByIdAsync(id);
            if (ordem == null)
                return NotFound(new { error = "Ordem de serviço não encontrada" });

            if (ordem.StatusProvedor == "Concluido")
                return BadRequest(new { error = "Não é possível cancelar uma ordem já concluída" });

            ordem.StatusProvedor = "Cancelado";
            ordem.DataAtualizacao = DateTime.UtcNow;

            await _ordemRepo.UpdateAsync(ordem);
            await _ordemRepo.SaveChangesAsync();

            _logger.LogInformation("Ordem {OrdemId} cancelada", id);

            return Ok(new { message = "Ordem cancelada com sucesso" });
        }

        private static OrdemServicoDTO MapToDTO(OrdemServico ordem) => new OrdemServicoDTO
        {
            Id = ordem.Id,
            ProjetoId = ordem.ProjetoId,
            ExternalProviderId = ordem.ExternalProviderId,
            StatusProvedor = ordem.StatusProvedor,
            MontadorNome = ordem.MontadorNome,
            MontadorTelefone = ordem.MontadorTelefone,
            MontadorFotoUrl = ordem.MontadorFotoUrl,
            DataAgendamento = ordem.DataAgendamento,
            DataAtualizacao = ordem.DataAtualizacao
        };
    }

    public class OrdemServicoDTO
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
    }
}
