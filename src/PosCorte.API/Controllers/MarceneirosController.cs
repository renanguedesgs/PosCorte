using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/marceneiros")]
    [Produces("application/json")]
    public class MarceneirosController : ControllerBase
    {
        private readonly IMarceneiroService _service;
        private readonly ILogger<MarceneirosController> _logger;

        public MarceneirosController(
            IMarceneiroService service,
            ILogger<MarceneirosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Lista marceneiros com filtros opcionais (cidade, especialidade, nota mínima, disponibilidade).</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MarceneiroDTO>>> Listar(
            [FromQuery] string? cidade,
            [FromQuery] string? especialidade,
            [FromQuery] decimal? notaMin,
            [FromQuery] bool? disponivel)
        {
            var marceneiros = await _service.ListarAsync(cidade, especialidade, notaMin, disponivel);
            return Ok(marceneiros.Select(MapToDTO));
        }

        /// <summary>Detalhe de um marceneiro com suas avaliações.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MarceneiroDetalheDTO>> Obter(int id)
        {
            var marceneiro = await _service.ObterAsync(id);
            if (marceneiro == null)
                return NotFound(new { error = "Marceneiro não encontrado" });

            var avaliacoes = await _service.ListarAvaliacoesAsync(id);

            var dto = new MarceneiroDetalheDTO
            {
                Id = marceneiro.Id,
                Nome = marceneiro.Nome,
                Telefone = marceneiro.Telefone,
                FotoUrl = marceneiro.FotoUrl,
                Cidade = marceneiro.Cidade,
                Estado = marceneiro.Estado,
                Bairro = marceneiro.Bairro,
                Especialidades = SplitEspecialidades(marceneiro.Especialidades),
                Bio = marceneiro.Bio,
                NotaMedia = marceneiro.NotaMedia,
                TotalAvaliacoes = marceneiro.TotalAvaliacoes,
                TotalServicos = marceneiro.TotalServicos,
                Disponivel = marceneiro.Disponivel,
                Verificado = marceneiro.Verificado,
                Avaliacoes = avaliacoes.Select(MapAvaliacao).ToList()
            };

            return Ok(dto);
        }

        /// <summary>Lista as avaliações de um marceneiro.</summary>
        [HttpGet("{id}/avaliacoes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AvaliacaoDTO>>> ListarAvaliacoes(int id)
        {
            var avaliacoes = await _service.ListarAvaliacoesAsync(id);
            return Ok(avaliacoes.Select(MapAvaliacao));
        }

        /// <summary>Cria uma avaliação para o marceneiro e recalcula a nota média.</summary>
        [HttpPost("{id}/avaliacoes")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AvaliacaoDTO>> Avaliar(int id, [FromBody] CreateAvaliacaoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var autor = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "Arquiteto";

            try
            {
                var avaliacao = await _service.AvaliarAsync(id, dto.Nota, dto.Comentario, autor, dto.ProjetoId);
                if (avaliacao == null)
                    return NotFound(new { error = "Marceneiro não encontrado" });

                return CreatedAtAction(nameof(ListarAvaliacoes), new { id }, MapAvaliacao(avaliacao));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private static MarceneiroDTO MapToDTO(Marceneiro m) => new()
        {
            Id = m.Id,
            Nome = m.Nome,
            Telefone = m.Telefone,
            FotoUrl = m.FotoUrl,
            Cidade = m.Cidade,
            Estado = m.Estado,
            Bairro = m.Bairro,
            Especialidades = SplitEspecialidades(m.Especialidades),
            Bio = m.Bio,
            NotaMedia = m.NotaMedia,
            TotalAvaliacoes = m.TotalAvaliacoes,
            TotalServicos = m.TotalServicos,
            Disponivel = m.Disponivel,
            Verificado = m.Verificado
        };

        private static AvaliacaoDTO MapAvaliacao(Avaliacao a) => new()
        {
            Id = a.Id,
            MarceneiroId = a.MarceneiroId,
            ProjetoId = a.ProjetoId,
            AutorNome = a.AutorNome,
            Nota = a.Nota,
            Comentario = a.Comentario,
            DataCriacao = a.DataCriacao
        };

        private static List<string> SplitEspecialidades(string csv)
            => string.IsNullOrWhiteSpace(csv)
                ? new List<string>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
