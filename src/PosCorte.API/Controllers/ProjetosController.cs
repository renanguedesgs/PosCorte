using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/projetos")]
    [Produces("application/json")]
    public class ProjetosController : ControllerBase
    {
        private readonly ILogger<ProjetosController> _logger;
        private readonly IRepositorio<Projeto> _projetoRepo;
        private readonly IRepositorio<Usuario> _usuarioRepo;
        private readonly IPrecificacaoService _precificacaoService;
        private readonly IPagamentoService _pagamentoService;
        private readonly IVistoriaService _vistoriaService;

        public ProjetosController(
            ILogger<ProjetosController> logger,
            IRepositorio<Projeto> projetoRepo,
            IRepositorio<Usuario> usuarioRepo,
            IPrecificacaoService precificacaoService,
            IPagamentoService pagamentoService,
            IVistoriaService vistoriaService)
        {
            _logger = logger;
            _projetoRepo = projetoRepo;
            _usuarioRepo = usuarioRepo;
            _precificacaoService = precificacaoService;
            _pagamentoService = pagamentoService;
            _vistoriaService = vistoriaService;
        }

        private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        private bool IsAdmin => User.IsInRole("Admin");

        /// <summary>
        /// Criar novo projeto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjetoDTO>> CriarProjeto([FromBody] CreateProjetoDTO dto)
        {
            _logger.LogInformation("Criando projeto: {NomeProjeto}", dto.NomeProjeto);

            // Seguranùa: arquiteto sù cria projeto em nome prùprio (admin pode especificar outro).
            if (!IsAdmin && UsuarioId > 0)
                dto.UsuarioId = UsuarioId;

            var usuario = await _usuarioRepo.GetByIdAsync(dto.UsuarioId);
            if (usuario == null)
                return BadRequest(new { error = "Usuùrio nùo encontrado" });

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
        /// Calcular orùamento de montagem.
        /// Usa fùrmula de Markup Inverso com taxa de 20%.
        /// </summary>
        [HttpPost("{id}/calcular-orcamento")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrcamentoResultado>> CalcularOrcamento(int id, [FromBody] OrcamentoRequest request)
        {
            var projeto = await _projetoRepo.GetByIdAsync(id);
            if (projeto == null)
                return NotFound(new { error = "Projeto nùo encontrado" });

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
                return NotFound(new { error = "Projeto nùo encontrado" });

            if (!IsAdmin && UsuarioId > 0 && projeto.UsuarioId != UsuarioId)
                return Forbid();

            return Ok(MapToDTO(projeto));
        }

        /// <summary>Arquiteto aprova a montagem: libera o escrow (split) e conclui o projeto.</summary>
        [HttpPost("{id}/aprovar-montagem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AprovarMontagem(int id)
        {
            var resultado = await _vistoriaService.AprovarMontagemAsync(id, UsuarioId);
            return MapResultadoVistoria(resultado, "Montagem aprovada e pagamento liberado.");
        }

        /// <summary>Arquiteto abre disputa: congela o escrow atù o admin resolver.</summary>
        [HttpPost("{id}/abrir-disputa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AbrirDisputa(int id, [FromBody] AbrirDisputaDTO dto)
        {
            var resultado = await _vistoriaService.AbrirDisputaAsync(id, UsuarioId, dto?.Motivo ?? "");
            return MapResultadoVistoria(resultado, "Disputa aberta. O valor permanece retido em escrow.");
        }

        /// <summary>DEV/teste: marca a montagem como concluùda e inicia a janela de vistoria.</summary>
        [HttpPost("{id}/simular-conclusao-montagem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SimularConclusaoMontagem(int id, [FromServices] IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
                return BadRequest(new { error = "Disponùvel apenas em Development." });

            var resultado = await _vistoriaService.MarcarMontagemConcluidaAsync(id, UsuarioId);
            return MapResultadoVistoria(resultado, "Montagem marcada como concluùda. Vistoria iniciada.");
        }

        private IActionResult MapResultadoVistoria(ResultadoVistoria r, string okMsg) => r switch
        {
            ResultadoVistoria.Ok => Ok(new { message = okMsg }),
            ResultadoVistoria.ProjetoNaoEncontrado => NotFound(new { error = "Projeto nùo encontrado." }),
            ResultadoVistoria.NaoAutorizado => Forbid(),
            ResultadoVistoria.StatusInvalido => BadRequest(new { error = "Aùùo nùo permitida no status atual do projeto." }),
            ResultadoVistoria.FalhaLiquidacao => BadRequest(new { error = "Nùo foi possùvel liberar o escrow (sem pagamento retido)." }),
            _ => BadRequest(new { error = "Erro desconhecido." })
        };

        /// <summary>Gera cobranùa PIX (Asaas se configurado, senùo stub para dev).</summary>
        [HttpPost("{id}/gerar-pix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GerarPixResponseDTO>> GerarPix(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            try
            {
                var result = await _pagamentoService.GerarPixAsync(id, userId);
                if (result == null) return NotFound(new { error = "Projeto nao encontrado" });
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Status do pagamento PIX do projeto (para polling na tela).</summary>
        [HttpGet("{id}/pagamento")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagamentoStatusDTO>> ObterPagamento(int id)
        {
            var status = await _pagamentoService.ObterStatusPagamentoAsync(id);
            if (status == null) return NotFound();
            return Ok(status);
        }

        /// <summary>
        /// Listar todos os projetos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProjetoDTO>>> ListarProjetos()
        {
            var projetos = await _projetoRepo.GetAllAsync();

            // Arquiteto vù apenas os prùprios projetos; admin vù todos.
            if (!IsAdmin && UsuarioId > 0)
                projetos = projetos.Where(p => p.UsuarioId == UsuarioId).ToList();

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
            StatusProjeto = projeto.StatusProjeto,
            DataLimiteVistoria = projeto.DataLimiteVistoria,
            MotivoDisputa = projeto.MotivoDisputa
        };
    }
}
