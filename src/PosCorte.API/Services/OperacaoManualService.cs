using Microsoft.EntityFrameworkCore;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services
{
    public class OperacaoManualService : IOperacaoManualService
    {
        private readonly PosCorteDbContext _db;
        private readonly IPrecificacaoService _precificacao;
        private readonly INotificacaoService _notificacao;
        private readonly ILogger<OperacaoManualService> _logger;

        private static readonly HashSet<string> StatusAlocacaoPermitidos = new(StringComparer.Ordinal)
        {
            "Aguardando_Provedor",
            "Pagamento_Confirmado",
            "Prestador_Alocado"
        };

        public OperacaoManualService(
            PosCorteDbContext db,
            IPrecificacaoService precificacao,
            INotificacaoService notificacao,
            ILogger<OperacaoManualService> logger)
        {
            _db = db;
            _precificacao = precificacao;
            _notificacao = notificacao;
            _logger = logger;
        }

        public async Task<(Marceneiro? marceneiro, ResultadoOperacaoManual resultado)> CadastrarMarceneiroAsync(CreateMarceneiroAdminDTO dto)
        {
            var marceneiro = new Marceneiro
            {
                Nome = dto.Nome.Trim(),
                Telefone = dto.Telefone.Trim(),
                Email = dto.Email?.Trim() ?? string.Empty,
                Cidade = dto.Cidade.Trim(),
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "SP" : dto.Estado.Trim(),
                Bairro = dto.Bairro?.Trim() ?? string.Empty,
                Cep = dto.Cep?.Trim() ?? string.Empty,
                Especialidades = dto.Especialidades?.Trim() ?? string.Empty,
                Bio = dto.Bio?.Trim() ?? string.Empty,
                Verificado = dto.Verificado,
                Disponivel = dto.Disponivel,
                OrigemExterna = $"manual:{Guid.NewGuid():N}",
                DataCadastro = DateTime.UtcNow
            };

            _db.Marceneiros.Add(marceneiro);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Marceneiro manual cadastrado: {Id} {Nome}", marceneiro.Id, marceneiro.Nome);
            return (marceneiro, ResultadoOperacaoManual.Ok);
        }

        public async Task<(CreateArquitetoAdminResponseDTO? response, ResultadoOperacaoManual resultado)> CadastrarArquitetoAsync(CreateArquitetoAdminDTO dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == email))
                return (null, ResultadoOperacaoManual.EmailDuplicado);

            var senha = string.IsNullOrWhiteSpace(dto.Senha)
                ? $"Pos{dto.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Corte"}2026!"
                : dto.Senha!;

            var usuario = new Usuario(dto.Nome.Trim(), email, dto.CpfCnpj.Trim(), dto.Telefone?.Trim() ?? string.Empty)
            {
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                Role = "Arquiteto",
                Ativo = true
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Arquiteto cadastrado manualmente: {Email}", email);

            return (new CreateArquitetoAdminResponseDTO
            {
                Arquiteto = MapArquiteto(usuario),
                SenhaInicial = senha
            }, ResultadoOperacaoManual.Ok);
        }

        public async Task<IReadOnlyList<ArquitetoAdminDTO>> ListarArquitetosAsync()
        {
            var lista = await _db.Usuarios.AsNoTracking()
                .Where(u => u.Role == "Arquiteto")
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            return lista.Select(MapArquiteto).ToList();
        }

        public async Task<ProjetoOperacaoAdminDTO?> ObterProjetoOperacaoAsync(int projetoId)
        {
            var projeto = await _db.Projetos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return null;

            var arquiteto = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == projeto.UsuarioId);
            var ordem = await _db.OrdensServico.AsNoTracking()
                .Where(o => o.ProjetoId == projetoId)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();
            var pagamento = await _db.Pagamentos.AsNoTracking()
                .Where(p => p.ProjetoId == projetoId)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            var orc = _precificacao.ProcessarProjeto(projeto.QtdPecas, projeto.QtdGavetas);

            return new ProjetoOperacaoAdminDTO
            {
                Projeto = MapProjeto(projeto),
                ArquitetoNome = arquiteto?.Nome ?? "—",
                ArquitetoEmail = arquiteto?.Email ?? "",
                ArquitetoTelefone = arquiteto?.Telefone ?? "",
                Orcamento = orc,
                Ordem = ordem == null ? null : new OrdemOperacaoAdminDTO
                {
                    Id = ordem.Id,
                    StatusProvedor = ordem.StatusProvedor,
                    MontadorNome = ordem.MontadorNome,
                    MontadorTelefone = ordem.MontadorTelefone,
                    DataAgendamento = ordem.DataAgendamento,
                    DataAtualizacao = ordem.DataAtualizacao
                },
                Pagamento = pagamento == null ? null : new PagamentoResumoAdminDTO
                {
                    Status = pagamento.Status,
                    ValorTotal = pagamento.ValorTotal,
                    ValorMarceneiro = pagamento.ValorMarceneiro
                }
            };
        }

        public async Task<ResultadoOperacaoManual> AlocarMontadorAsync(int projetoId, AlocarMontadorDTO dto)
        {
            var projeto = await _db.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return ResultadoOperacaoManual.NaoEncontrado;

            if (!StatusAlocacaoPermitidos.Contains(projeto.StatusProjeto))
                return ResultadoOperacaoManual.StatusInvalido;

            var marceneiro = await _db.Marceneiros.FirstOrDefaultAsync(m => m.Id == dto.MarceneiroId);
            if (marceneiro == null) return ResultadoOperacaoManual.NaoEncontrado;

            var ordem = await _db.OrdensServico
                .Where(o => o.ProjetoId == projetoId)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

            var agendamento = dto.DataAgendamento?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(1);
            var jaAlocado = ordem != null && !string.IsNullOrEmpty(ordem.MontadorNome);

            if (ordem == null)
            {
                ordem = new OrdemServico(projetoId, $"MC-{marceneiro.Id}")
                {
                    StatusProvedor = "Prestador_Alocado",
                    MontadorNome = marceneiro.Nome,
                    MontadorTelefone = marceneiro.Telefone,
                    MontadorFotoUrl = marceneiro.FotoUrl,
                    DataAgendamento = agendamento
                };
                _db.OrdensServico.Add(ordem);
            }
            else
            {
                ordem.ExternalProviderId = $"MC-{marceneiro.Id}";
                ordem.StatusProvedor = "Prestador_Alocado";
                ordem.MontadorNome = marceneiro.Nome;
                ordem.MontadorTelefone = marceneiro.Telefone;
                ordem.MontadorFotoUrl = marceneiro.FotoUrl;
                ordem.DataAgendamento = agendamento;
                ordem.DataAtualizacao = DateTime.UtcNow;
            }

            projeto.StatusProjeto = "Prestador_Alocado";

            if (!jaAlocado)
                marceneiro.TotalServicos += 1;

            await _db.SaveChangesAsync();

            var msgMontador = $"Nova montagem PósCorte: {projeto.NomeProjeto}. Endereço: {projeto.EnderecoCompleto}. Plano: {projeto.UrlArquivoCorteCloud}";
            await _notificacao.NotificarMontador(marceneiro.Telefone, msgMontador);
            await _notificacao.NotificarEventoAsync(NotificacaoEvento.MontadorAlocado, projetoId,
                $"Montador {marceneiro.Nome} alocado manualmente para {projeto.NomeProjeto}.");

            _logger.LogInformation("Montador {MarceneiroId} alocado manualmente ao projeto {ProjetoId}", marceneiro.Id, projetoId);
            return ResultadoOperacaoManual.Ok;
        }

        public async Task<ResultadoOperacaoManual> MarcarMontagemConcluidaAsync(int projetoId)
        {
            var projeto = await _db.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId);
            if (projeto == null) return ResultadoOperacaoManual.NaoEncontrado;

            if (projeto.StatusProjeto != "Prestador_Alocado")
                return ResultadoOperacaoManual.StatusInvalido;

            var ordem = await _db.OrdensServico
                .Where(o => o.ProjetoId == projetoId)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

            if (ordem != null)
            {
                ordem.StatusProvedor = "Concluido";
                ordem.DataAtualizacao = DateTime.UtcNow;
            }

            projeto.StatusProjeto = "Aguardando_Vistoria";
            projeto.DataLimiteVistoria = DateTime.UtcNow.AddHours(VistoriaService.HorasJanelaVistoria);

            await _db.SaveChangesAsync();

            await _notificacao.NotificarEventoAsync(NotificacaoEvento.MontagemConcluida, projetoId,
                $"Montagem concluída (admin). Projeto {projeto.NomeProjeto} aguardando vistoria do arquiteto.");

            return ResultadoOperacaoManual.Ok;
        }

        private static ArquitetoAdminDTO MapArquiteto(Usuario u) => new()
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Telefone = u.Telefone,
            CpfCnpj = u.CpfCnpj,
            Ativo = u.Ativo,
            DataCadastro = u.DataCadastro
        };

        private static ProjetoDTO MapProjeto(Projeto p) => new()
        {
            Id = p.Id,
            UsuarioId = p.UsuarioId,
            NomeProjeto = p.NomeProjeto,
            UrlArquivoCorteCloud = p.UrlArquivoCorteCloud,
            QtdPecas = p.QtdPecas,
            QtdGavetas = p.QtdGavetas,
            CepObra = p.CepObra,
            EnderecoCompleto = p.EnderecoCompleto,
            StatusProjeto = p.StatusProjeto,
            DataLimiteVistoria = p.DataLimiteVistoria,
            MotivoDisputa = p.MotivoDisputa
        };
    }
}
