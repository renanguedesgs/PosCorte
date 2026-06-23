using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosCorte.API.Configuration;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/admin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly PosCorteDbContext _db;
        private readonly IPrecificacaoService _precificacao;
        private readonly AsaasOptions _asaas;

        public AdminController(PosCorteDbContext db, IPrecificacaoService precificacao, IOptions<AsaasOptions> asaas)
        {
            _db = db;
            _precificacao = precificacao;
            _asaas = asaas.Value;
        }

        /// <summary>Painel administrativo: KPIs, financeiro estimado e projetos recentes.</summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDTO>> Dashboard()
        {
            var usuarios = await _db.Usuarios.AsNoTracking().ToListAsync();
            var projetos = await _db.Projetos.AsNoTracking().ToListAsync();
            var ordens = await _db.OrdensServico.AsNoTracking().ToListAsync();
            var marceneiros = await _db.Marceneiros.AsNoTracking().CountAsync();

            decimal volume = 0, margem = 0;
            var recentes = new List<ProjetoResumoAdminDTO>();

            foreach (var p in projetos.OrderByDescending(x => x.Id).Take(10))
            {
                var orc = _precificacao.ProcessarProjeto(p.QtdPecas, p.QtdGavetas);
                volume += orc.ValorTotal;
                margem += orc.MargemLucro;

                var arq = usuarios.FirstOrDefault(u => u.Id == p.UsuarioId);
                var ordem = ordens.FirstOrDefault(o => o.ProjetoId == p.Id);

                recentes.Add(new ProjetoResumoAdminDTO
                {
                    Id = p.Id,
                    NomeProjeto = p.NomeProjeto,
                    StatusProjeto = p.StatusProjeto,
                    ArquitetoNome = arq?.Nome ?? "—",
                    ValorEstimado = orc.ValorTotal,
                    MargemEstimada = orc.MargemLucro,
                    MontadorNome = string.IsNullOrEmpty(ordem?.MontadorNome) ? null : ordem.MontadorNome
                });
            }

            foreach (var p in projetos.Skip(10))
            {
                var orc = _precificacao.ProcessarProjeto(p.QtdPecas, p.QtdGavetas);
                volume += orc.ValorTotal;
                margem += orc.MargemLucro;
            }

            var pagamentosConfirmados = await _db.Pagamentos.CountAsync(p => p.Status == "Confirmado" || p.Status == "Retido_Escrow" || p.Status == "Liquidado");
            decimal margemReal = await _db.Pagamentos
                .Where(p => p.Status == "Confirmado" || p.Status == "Retido_Escrow" || p.Status == "Liquidado")
                .SumAsync(p => p.ValorPlataforma);

            return Ok(new AdminDashboardDTO
            {
                TotalArquitetos = usuarios.Count(u => u.Role == "Arquiteto"),
                TotalMarceneiros = marceneiros,
                TotalProjetos = projetos.Count,
                TotalOrdens = ordens.Count,
                ReceitaPlataformaEstimada = pagamentosConfirmados > 0 ? Math.Round(margemReal, 2) : Math.Round(margem, 2),
                VolumeTransacionadoEstimado = pagamentosConfirmados > 0
                    ? Math.Round(await _db.Pagamentos.Where(p => p.Status != "Cancelado").SumAsync(p => p.ValorTotal), 2)
                    : Math.Round(volume, 2),
                GatewayPagamento = _asaas.EstaConfigurado ? "Asaas (configurado)" : "Stub — preencha Asaas:ApiKey + Enabled=true",
                StatusEscrow = _asaas.EstaConfigurado
                    ? "Asaas + tabelas pagamentos/liquidacoes prontas"
                    : "Modo desenvolvimento — sem cobranca real (use Simular pagamento)",
                ProjetosPorStatus = projetos.GroupBy(p => p.StatusProjeto).ToDictionary(g => g.Key, g => g.Count()),
                ProjetosRecentes = recentes
            });
        }
    }
}
