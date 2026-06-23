using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    public class VistoriaService : IVistoriaService
    {
        /// <summary>Janela (horas) que o arquiteto tem para aprovar/contestar antes da liberação automática.</summary>
        public const int HorasJanelaVistoria = 72;

        private readonly ILogger<VistoriaService> _logger;
        private readonly PosCorteDbContext _db;
        private readonly IPagamentoService _pagamento;
        private readonly INotificacaoService _notificacao;

        public VistoriaService(
            ILogger<VistoriaService> logger,
            PosCorteDbContext db,
            IPagamentoService pagamento,
            INotificacaoService notificacao)
        {
            _logger = logger;
            _db = db;
            _pagamento = pagamento;
            _notificacao = notificacao;
        }

        public async Task<ResultadoVistoria> AprovarMontagemAsync(int projetoId, int usuarioId)
        {
            var projeto = await _db.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return ResultadoVistoria.ProjetoNaoEncontrado;
            if (projeto.UsuarioId != usuarioId) return ResultadoVistoria.NaoAutorizado;
            if (projeto.StatusProjeto is not ("Aguardando_Vistoria" or "Em_Disputa"))
                return ResultadoVistoria.StatusInvalido;

            return await ConcluirComLiquidacaoAsync(projetoId, "aprovação do arquiteto");
        }

        public async Task<ResultadoVistoria> AbrirDisputaAsync(int projetoId, int usuarioId, string motivo)
        {
            var projeto = await _db.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return ResultadoVistoria.ProjetoNaoEncontrado;
            if (projeto.UsuarioId != usuarioId) return ResultadoVistoria.NaoAutorizado;
            if (projeto.StatusProjeto != "Aguardando_Vistoria")
                return ResultadoVistoria.StatusInvalido;

            projeto.StatusProjeto = "Em_Disputa";
            projeto.MotivoDisputa = string.IsNullOrWhiteSpace(motivo) ? "Não informado" : motivo.Trim();
            projeto.DataLimiteVistoria = null; // congela: não libera automático em disputa
            await _db.SaveChangesAsync();

            _logger.LogInformation("Disputa aberta no projeto {ProjetoId}: {Motivo}", projetoId, projeto.MotivoDisputa);
            return ResultadoVistoria.Ok;
        }

        public async Task<ResultadoVistoria> MarcarMontagemConcluidaAsync(int projetoId, int usuarioId)
        {
            var projeto = await _db.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return ResultadoVistoria.ProjetoNaoEncontrado;
            if (projeto.UsuarioId != usuarioId) return ResultadoVistoria.NaoAutorizado;
            if (projeto.StatusProjeto is not ("Prestador_Alocado" or "Aguardando_Provedor"))
                return ResultadoVistoria.StatusInvalido;

            projeto.StatusProjeto = "Aguardando_Vistoria";
            projeto.DataLimiteVistoria = DateTime.UtcNow.AddHours(HorasJanelaVistoria);

            var ordem = await _db.OrdensServico
                .Where(o => o.ProjetoId == projetoId)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();
            if (ordem != null)
            {
                ordem.StatusProvedor = "Concluido";
                ordem.DataAtualizacao = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Montagem marcada como concluída no projeto {ProjetoId}. Vistoria até {Prazo}",
                projetoId, projeto.DataLimiteVistoria);
            return ResultadoVistoria.Ok;
        }

        public async Task<int> LiquidarVencidosAsync(CancellationToken ct = default)
        {
            var agora = DateTime.UtcNow;
            var vencidos = await _db.Projetos
                .Where(p => p.StatusProjeto == "Aguardando_Vistoria"
                            && p.DataLimiteVistoria != null
                            && p.DataLimiteVistoria <= agora)
                .Select(p => p.Id)
                .ToListAsync(ct);

            var total = 0;
            foreach (var id in vencidos)
            {
                var r = await ConcluirComLiquidacaoAsync(id, "liberação automática (prazo de vistoria expirado)");
                if (r == ResultadoVistoria.Ok) total++;
            }

            if (total > 0)
                _logger.LogInformation("Liquidação automática: {Total} projeto(s) liberados por prazo", total);

            return total;
        }

        private async Task<ResultadoVistoria> ConcluirComLiquidacaoAsync(int projetoId, string motivo)
        {
            var liquidou = await _pagamento.LiquidarPorProjetoAsync(projetoId);
            if (!liquidou) return ResultadoVistoria.FalhaLiquidacao;

            var projeto = await _db.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return ResultadoVistoria.ProjetoNaoEncontrado;

            projeto.StatusProjeto = "Concluido";
            projeto.DataLimiteVistoria = null;
            await _db.SaveChangesAsync();

            await _notificacao.NotificarEventoAsync(NotificacaoEvento.MontagemConcluida, projetoId,
                $"Projeto {projeto.NomeProjeto} concluído ({motivo}). Escrow liberado.");

            _logger.LogInformation("Projeto {ProjetoId} concluído via {Motivo}", projetoId, motivo);
            return ResultadoVistoria.Ok;
        }
    }
}
