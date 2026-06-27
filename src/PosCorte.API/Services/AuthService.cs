using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;
using PosCorte.Domain.Validation;

namespace PosCorte.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly PosCorteDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(PosCorteDbContext context, IConfiguration config, ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task<string?> LoginAsync(string email, string senha)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
            {
                _logger.LogWarning("Tentativa de login falhou para: {Email}", email);
                return null;
            }

            return GerarToken(usuario);
        }

        public async Task<(bool ok, string? erro)> RegisterAsync(string nome, string email, string cpfCnpj, string telefone, string senha)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return (false, "Informe o nome.");

            if (string.IsNullOrWhiteSpace(email))
                return (false, "Informe o e-mail.");

            var doc = DocumentoBrasil.SoDigitos(cpfCnpj);
            if (!DocumentoBrasil.EhCpfOuCnpjValido(doc))
                return (false, "CPF ou CNPJ inválido.");

            if (!TelefoneBrasil.EhValido(telefone))
                return (false, "Telefone inválido. Use DDD + número (10 ou 11 dígitos).");

            var erroSenha = SenhaPolicy.ObterErro(senha);
            if (erroSenha is not null)
                return (false, erroSenha);

            var emailNorm = email.Trim().ToLowerInvariant();
            if (await _context.Usuarios.AnyAsync(u => u.Email == emailNorm))
                return (false, "E-mail já cadastrado.");

            var usuario = new Usuario(nome.Trim(), emailNorm, doc, DocumentoBrasil.SoDigitos(telefone))
            {
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Novo usuario registrado: {Email}", emailNorm);
            return (true, null);
        }

        public async Task<UsuarioPerfilDTO?> ObterPerfilAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);
            if (usuario == null) return null;

            return new UsuarioPerfilDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                CpfCnpj = usuario.CpfCnpj,
                Telefone = usuario.Telefone,
                Role = usuario.Role,
                DataCadastro = usuario.DataCadastro
            };
        }

        public async Task<(bool ok, string? erro)> AlterarSenhaAsync(int usuarioId, string senhaAtual, string senhaNova)
        {
            var erroSenha = SenhaPolicy.ObterErro(senhaNova);
            if (erroSenha is not null)
                return (false, erroSenha);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null || !usuario.Ativo)
                return (false, "Usuário não encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.SenhaHash))
                return (false, "Senha atual incorreta.");

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaNova);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Senha alterada para usuario {Id}", usuarioId);
            return (true, null);
        }

        private string GerarToken(Usuario usuario)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiracao = DateTime.UtcNow.AddHours(8);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiracao,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
