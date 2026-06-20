using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>Login — retorna JWT Bearer token</summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO dto)
        {
            var token = await _authService.LoginAsync(dto.Email, dto.Senha);
            if (token == null)
                return Unauthorized(new { error = "Email ou senha inválidos" });

            return Ok(new AuthResponseDTO
            {
                Token = token,
                Email = dto.Email,
                Expiracao = DateTime.UtcNow.AddHours(8)
            });
        }

        /// <summary>Cadastro de novo usuário</summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var ok = await _authService.RegisterAsync(dto.Nome, dto.Email, dto.CpfCnpj, dto.Telefone, dto.Senha);
            if (!ok)
                return BadRequest(new { error = "Email já cadastrado" });

            return StatusCode(201, new { message = "Usuário criado com sucesso" });
        }
    }
}
