using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using System.Security.Claims;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("login")]
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var (ok, erro) = await _authService.RegisterAsync(dto.Nome, dto.Email, dto.CpfCnpj, dto.Telefone, dto.Senha);
            if (!ok)
                return BadRequest(new { error = erro ?? "Não foi possível criar a conta." });

            return StatusCode(201, new { message = "Usuário criado com sucesso" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UsuarioPerfilDTO>> Me()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var perfil = await _authService.ObterPerfilAsync(userId);
            if (perfil == null)
                return NotFound(new { error = "Usuário não encontrado" });
            return Ok(perfil);
        }

        [HttpPost("alterar-senha")]
        [Authorize]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var (ok, erro) = await _authService.AlterarSenhaAsync(userId, dto.SenhaAtual, dto.SenhaNova);
            if (!ok)
                return BadRequest(new { error = erro });
            return Ok(new { message = "Senha alterada com sucesso" });
        }
    }
}
