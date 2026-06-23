using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services.Marceneiros
{
    public class MarceneiroService : IMarceneiroService
    {
        private readonly PosCorteDbContext _db;
        private readonly ILogger<MarceneiroService> _logger;

        public MarceneiroService(PosCorteDbContext db, ILogger<MarceneiroService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<Marceneiro>> ListarAsync(string? cidade, string? especialidade, decimal? notaMin, bool? disponivel)
        {
            var query = _db.Marceneiros.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(m => EF.Functions.ILike(m.Cidade, $"%{cidade}%"));

            if (!string.IsNullOrWhiteSpace(especialidade))
                query = query.Where(m => EF.Functions.ILike(m.Especialidades, $"%{especialidade}%"));

            if (notaMin.HasValue)
                query = query.Where(m => m.NotaMedia >= notaMin.Value);

            if (disponivel.HasValue)
                query = query.Where(m => m.Disponivel == disponivel.Value);

            return await query
                .OrderByDescending(m => m.Verificado)
                .ThenByDescending(m => m.NotaMedia)
                .ThenByDescending(m => m.TotalServicos)
                .ToListAsync();
        }

        public async Task<Marceneiro?> ObterAsync(int id)
            => await _db.Marceneiros.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        public async Task<IEnumerable<Avaliacao>> ListarAvaliacoesAsync(int marceneiroId)
            => await _db.Avaliacoes.AsNoTracking()
                .Where(a => a.MarceneiroId == marceneiroId)
                .OrderByDescending(a => a.DataCriacao)
                .ToListAsync();

        public async Task<Avaliacao?> AvaliarAsync(int marceneiroId, int nota, string comentario, string autorNome, int? projetoId)
        {
            if (nota < 1 || nota > 5)
                throw new ArgumentException("A nota deve ser entre 1 e 5.");

            var marceneiro = await _db.Marceneiros.FirstOrDefaultAsync(m => m.Id == marceneiroId);
            if (marceneiro == null) return null;

            var avaliacao = new Avaliacao
            {
                MarceneiroId = marceneiroId,
                ProjetoId = projetoId,
                AutorNome = string.IsNullOrWhiteSpace(autorNome) ? "Arquiteto" : autorNome,
                Nota = nota,
                Comentario = comentario ?? string.Empty,
                DataCriacao = DateTime.UtcNow
            };

            _db.Avaliacoes.Add(avaliacao);

            // Média incremental considerando a reputação já existente.
            var somaAtual = marceneiro.NotaMedia * marceneiro.TotalAvaliacoes;
            marceneiro.TotalAvaliacoes += 1;
            marceneiro.NotaMedia = Math.Round((somaAtual + nota) / marceneiro.TotalAvaliacoes, 2);

            await _db.SaveChangesAsync();

            _logger.LogInformation("Marceneiro {Id} avaliado com nota {Nota}. Nova média: {Media}",
                marceneiroId, nota, marceneiro.NotaMedia);

            return avaliacao;
        }

        public async Task<Marceneiro?> EscolherMelhorAsync(string? cidade, string? especialidade)
        {
            var query = _db.Marceneiros.Where(m => m.Disponivel);

            // 1ª tentativa: mesma cidade + especialidade compatível.
            if (!string.IsNullOrWhiteSpace(cidade))
            {
                var porCidade = query.Where(m => EF.Functions.ILike(m.Cidade, $"%{cidade}%"));
                if (!string.IsNullOrWhiteSpace(especialidade))
                    porCidade = porCidade.Where(m => EF.Functions.ILike(m.Especialidades, $"%{especialidade}%"));

                var melhorLocal = await porCidade
                    .OrderByDescending(m => m.NotaMedia)
                    .ThenByDescending(m => m.TotalServicos)
                    .FirstOrDefaultAsync();

                if (melhorLocal != null) return melhorLocal;
            }

            // Fallback: melhor avaliado disponível em qualquer lugar.
            return await query
                .OrderByDescending(m => m.Verificado)
                .ThenByDescending(m => m.NotaMedia)
                .ThenByDescending(m => m.TotalServicos)
                .FirstOrDefaultAsync();
        }

        public async Task<Marceneiro?> AlocarParaProjetoAsync(string? cidade, string? especialidade)
        {
            var escolhido = await EscolherMelhorAsync(cidade, especialidade);
            if (escolhido == null) return null;

            var tracked = await _db.Marceneiros.FirstOrDefaultAsync(m => m.Id == escolhido.Id);
            if (tracked != null)
            {
                tracked.TotalServicos += 1;
                await _db.SaveChangesAsync();
                _logger.LogInformation("Marceneiro {Id} ({Nome}) alocado para novo serviço", tracked.Id, tracked.Nome);
                return tracked;
            }

            return escolhido;
        }
    }
}
