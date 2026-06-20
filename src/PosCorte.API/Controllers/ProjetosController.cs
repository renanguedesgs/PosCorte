using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/projetos")]
    [Produces("application/json")]
    public class ProjetosController : ControllerBase
    {
        private readonly ILogger<ProjetosController> _logger;
        private readonly IRepositorio<Projeto> _projetoRepo;
        private readonly IRepositorio<Usuario> _usuarioRepo;
        private readonly IPrecificacaoService _precificacaoService;

        public ProjetosController(
            ILogger<ProjetosController> logger,
            IRepositorio<Projeto> projetoRepo,
            IRepositorio<Usuario> usuarioRepo,
            IPrecificacaoService precificacaoService)
        {
            _logger = logger;
            _projetoRepo = projetoRepo;
            _usuarioRepo = usuarioRepo;
            _precificacaoService = precificacaoService;
        }

        /// <summary>
        /// Criar novo projeto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjetoDTO>> CriarProjeto([FromBody] CreateProjetoDTO dto)
        {
            _logger.LogInformation("Criando projeto: {NomeProjeto}", dto.NomeProjeto);

            var usuario = await _usuarioRepo.GetByIdAsync(dto.UsuarioId);
            if (usuario == null)
                return BadRequest(new { error = "Usuário não encontrado" });

            try
            {
                var projeto = new Projeto(
                    dto.UsuarioId,
                    dto.NomeProjeto,
                    dto.UrlArquivoCorteCloud,
                    dto.QtdPecas,
                    dto.QtdGavetas,
                    dto.CepObra,
                    dto.EnderecoCompleto
                );

                var projetoCriado = await _projetoRepo.AddAsync(projeto);
                await _projetoRepo.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterProjeto), new { id = projetoCriado.Id }, MapToDTO(projetoCriado));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar projeto");
                return StatusCode(500, new { error = "Erro ao criar projeto" });
            }
        }

        /// <summary>
        /// Calcular orçamento de montagem.
        /// Usa fórmula de Markup Inverso com taxa de 20%.
        /// </summary>
        [HttpPost("{id}/calcular-orcamento")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrcamentoResultado>> CalcularOrcamento(int id, [FromBody] OrcamentoRequest request)
        {
            var projeto = await _projetoRepo.GetByIdAsync(id);
            if (projeto == null)
                return NotFound(new { error = "Projeto não encontrado" });

            try
            {
                var resultado = _precificacaoService.ProcessarProjeto(
                    request.QtdPecas ?? projeto.QtdPecas,
                    request.QtdGavetas ?? projeto.QtdGavetas
                );

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obter projeto por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjetoDTO>> ObterProjeto(int id)
        {
            var projeto = await _projetoRepo.GetByIdAsync(id);

            if (projeto == null)
                return NotFound(new { error = "Projeto não encontrado" });

            return Ok(MapToDTO(projeto));
        }

        /// <summary>
        /// Listar todos os projetos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProjetoDTO>>> ListarProjetos()
        {
            var projetos = await _projetoRepo.GetAllAsync();
            return Ok(projetos.Select(MapToDTO));
        }

        private static ProjetoDTO MapToDTO(Projeto projeto) => new ProjetoDTO
        {
            Id = projeto.Id,
            UsuarioId = projeto.UsuarioId,
            NomeProjeto = projeto.NomeProjeto,
            UrlArquivoCorteCloud = projeto.UrlArquivoCorteCloud,
            QtdPecas = projeto.QtdPecas,
            QtdGavetas = projeto.QtdGavetas,
            CepObra = projeto.CepObra,
            EnderecoCompleto = projeto.EnderecoCompleto,
            StatusProjeto = projeto.StatusProjeto
        };
    }
}
