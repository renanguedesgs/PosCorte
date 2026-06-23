using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/usuarios")]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly IRepositorio<Usuario> _usuarioRepo;

        public UsuariosController(
            ILogger<UsuariosController> logger,
            IRepositorio<Usuario> usuarioRepo)
        {
            _logger = logger;
            _usuarioRepo = usuarioRepo;
        }

        /// <summary>
        /// Criar novo usuário (Arquiteto/Designer)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioDTO>> CriarUsuario([FromBody] CreateUsuarioDTO dto)
        {
            _logger.LogInformation("Criando usuário: {Email}", dto.Email);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var usuario = new Usuario(dto.Nome, dto.Email, dto.CpfCnpj, dto.Telefone);

                var usuarioCriado = await _usuarioRepo.AddAsync(usuario);
                await _usuarioRepo.SaveChangesAsync();

                var usuarioRetorno = new UsuarioDTO
                {
                    Id = usuarioCriado.Id,
                    Nome = usuarioCriado.Nome,
                    Email = usuarioCriado.Email,
                    Telefone = usuarioCriado.Telefone
                };

                return CreatedAtAction(nameof(ObterUsuario), new { id = usuarioCriado.Id }, usuarioRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário");
                return StatusCode(500, new { error = "Erro ao criar usuário" });
            }
        }

        /// <summary>
        /// Obter usuário por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDTO>> ObterUsuario(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);

            if (usuario == null)
                return NotFound(new { error = "Usuário não encontrado" });

            return Ok(new UsuarioDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Telefone = usuario.Telefone
            });
        }

        /// <summary>
        /// Listar todos os usuários
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> ListarUsuarios()
        {
            var usuarios = await _usuarioRepo.GetAllAsync();

            var dtos = usuarios.Select(u => new UsuarioDTO
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Telefone = u.Telefone
            });

            return Ok(dtos);
        }
    }
}
