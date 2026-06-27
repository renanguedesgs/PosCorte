using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services.Marceneiros
{
    public class MarceneiroService : IMarceneiroService
    {
        private readonly PosCorteDbContext _db;
        private readonly INotificacaoService _notificacao;
        private readonly ILogger<MarceneiroService> _logger;

        public MarceneiroService(PosCorteDbContext db, INotificacaoService notificacao, ILogger<MarceneiroService> logger)
        {
            _db = db;
            _notificacao = notificacao;
            _logger = logger;
        }

        public async Task<(Marceneiro? marceneiro, ResultadoAutoCadastro resultado)> AutoCadastrarAsync(AutoCadastroMarceneiroDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Telefone) || string.IsNullOrWhiteSpace(dto.Cidade))
                return (null, ResultadoAutoCadastro.DadosInvalidos);

            var telefone = OnlyDigits(dto.Telefone);
            var email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;

            // Deduplica por telefone (sempre) e por e-mail (quando informado).
            var jaExiste = await _db.Marceneiros.AnyAsync(m =>
                (telefone.Length >= 8 && m.Telefone == telefone) ||
                (email.Length > 0 && m.Email.ToLower() == email));

            if (jaExiste)
                return (null, ResultadoAutoCadastro.Duplicado);

            var marceneiro = new Marceneiro
            {
                Nome = dto.Nome.Trim(),
                Telefone = telefone,
                Email = email,
                Cidade = dto.Cidade.Trim(),
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "SP" : dto.Estado.Trim().ToUpperInvariant(),
                Bairro = dto.Bairro?.Trim() ?? string.Empty,
                Cep = dto.Cep?.Trim() ?? string.Empty,
                Especialidades = dto.Especialidades?.Trim() ?? string.Empty,
                Bio = dto.Bio?.Trim() ?? string.Empty,
                Verificado = false,
                Disponivel = false,
                OrigemExterna = "autocadastro",
                DataCadastro = DateTime.UtcNow
            };

            _db.Marceneiros.Add(marceneiro);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Auto-cadastro de montador recebido: {Id} {Nome} ({Cidade})", marceneiro.Id, marceneiro.Nome, marceneiro.Cidade);

            var msg = $"Olá {PrimeiroNome(marceneiro.Nome)}! Recebemos seu cadastro na rede PósCorte. " +
                      "Em breve homologamos seu perfil e você começa a receber montagens de móveis planejados pagas com garantia (escrow).";
            await _notificacao.NotificarMontador(marceneiro.Telefone, msg);

            return (marceneiro, ResultadoAutoCadastro.Ok);
        }

        public async Task<IEnumerable<Marceneiro>> ListarParaAdminAsync(bool? verificado)
        {
            var query = _db.Marceneiros.AsNoTracking().AsQueryable();
            if (verificado.HasValue)
                query = query.Where(m => m.Verificado == verificado.Value);

            return await query
                .OrderBy(m => m.Verificado)
                .ThenByDescending(m => m.DataCadastro)
                .ToListAsync();
        }

        public async Task<bool> VerificarAsync(int id)
        {
            var marceneiro = await _db.Marceneiros.FirstOrDefaultAsync(m => m.Id == id);
            if (marceneiro == null) return false;

            var jaEra = marceneiro.Verificado;
            marceneiro.Verificado = true;
            marceneiro.Disponivel = true;
            await _db.SaveChangesAsync();

            if (!jaEra)
            {
                _logger.LogInformation("Montador {Id} ({Nome}) homologado na rede", marceneiro.Id, marceneiro.Nome);
                var msg = $"Parabéns {PrimeiroNome(marceneiro.Nome)}! Seu perfil foi homologado na rede PósCorte. " +
                          "A partir de agora você pode receber montagens pagas com garantia. Fique de olho no WhatsApp.";
                await _notificacao.NotificarMontador(marceneiro.Telefone, msg);
                if (!string.IsNullOrWhiteSpace(marceneiro.Email))
                    await _notificacao.EnviarEmailConfirmacao(marceneiro.Email, msg);
            }

            return true;
        }

        public async Task<(bool ok, bool disponivel)> AlternarDisponibilidadeAsync(int id)
        {
            var marceneiro = await _db.Marceneiros.FirstOrDefaultAsync(m => m.Id == id);
            if (marceneiro == null) return (false, false);

            marceneiro.Disponivel = !marceneiro.Disponivel;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Disponibilidade do montador {Id} alterada para {Estado}", id, marceneiro.Disponivel);
            return (true, marceneiro.Disponivel);
        }

        private static string OnlyDigits(string s) => new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

        private static string PrimeiroNome(string nome)
            => nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? nome;

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
