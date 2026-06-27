using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosCorte.API.Configuration;
using PosCorte.API.Data;
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;

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
        private readonly IOperacaoManualService _operacao;
        private readonly IMarceneiroService _marceneiros;
        private readonly IAuthService _auth;
        private readonly AsaasOptions _asaas;

        public AdminController(
            PosCorteDbContext db,
            IPrecificacaoService precificacao,
            IOperacaoManualService operacao,
            IMarceneiroService marceneiros,
            IAuthService auth,
            IOptions<AsaasOptions> asaas)
        {
            _db = db;
            _precificacao = precificacao;
            _operacao = operacao;
            _marceneiros = marceneiros;
            _auth = auth;
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
                GatewayPagamento = _asaas.EstaConfigurado ? "Asaas (configurado)" : "Stub — simule pagamento em dev",
                StatusEscrow = _asaas.EstaConfigurado
                    ? "Asaas + tabelas pagamentos/liquidacoes prontas"
                    : "Modo manual — você cadastra arquitetos/montadores e aloca no admin",
                ProjetosPorStatus = projetos.GroupBy(p => p.StatusProjeto).ToDictionary(g => g.Key, g => g.Count()),
                ProjetosRecentes = recentes
            });
        }

        [HttpGet("arquitetos")]
        public async Task<ActionResult<IReadOnlyList<ArquitetoAdminDTO>>> ListarArquitetos()
            => Ok(await _operacao.ListarArquitetosAsync());

        [HttpPost("arquitetos")]
        public async Task<ActionResult<CreateArquitetoAdminResponseDTO>> CadastrarArquiteto([FromBody] CreateArquitetoAdminDTO dto)
        {
            var (response, resultado) = await _operacao.CadastrarArquitetoAsync(dto);
            return resultado switch
            {
                ResultadoOperacaoManual.EmailDuplicado => Conflict(new { erro = "E-mail já cadastrado." }),
                ResultadoOperacaoManual.Ok => CreatedAtAction(nameof(ListarArquitetos), response),
                _ => BadRequest()
            };
        }

        [HttpPost("marceneiros")]
        public async Task<IActionResult> CadastrarMarceneiro([FromBody] CreateMarceneiroAdminDTO dto)
        {
            var (marceneiro, resultado) = await _operacao.CadastrarMarceneiroAsync(dto);
            if (resultado != ResultadoOperacaoManual.Ok || marceneiro == null)
                return BadRequest();

            return Ok(new { id = marceneiro.Id, nome = marceneiro.Nome, mensagem = "Montador cadastrado na rede." });
        }

        /// <summary>Lista montadores para o admin. verificado=false traz a fila de aprovação (auto-cadastros e leads).</summary>
        [HttpGet("marceneiros")]
        public async Task<ActionResult<IEnumerable<MarceneiroAdminDTO>>> ListarMarceneiros([FromQuery] bool? verificado)
        {
            var lista = await _marceneiros.ListarParaAdminAsync(verificado);
            return Ok(lista.Select(MapMarceneiroAdmin));
        }

        /// <summary>Homologa (verifica) um montador: passa a entrar na alocação automática.</summary>
        [HttpPost("marceneiros/{id:int}/verificar")]
        public async Task<IActionResult> VerificarMarceneiro(int id)
        {
            var ok = await _marceneiros.VerificarAsync(id);
            return ok
                ? Ok(new { mensagem = "Montador homologado na rede." })
                : NotFound(new { erro = "Montador não encontrado." });
        }

        /// <summary>Alterna a disponibilidade do montador (Disponível / Ocupado).</summary>
        [HttpPost("marceneiros/{id:int}/disponibilidade")]
        public async Task<IActionResult> AlternarDisponibilidade(int id)
        {
            var (ok, disponivel) = await _marceneiros.AlternarDisponibilidadeAsync(id);
            return ok
                ? Ok(new { disponivel, mensagem = disponivel ? "Montador disponível." : "Montador marcado como ocupado." })
                : NotFound(new { erro = "Montador não encontrado." });
        }

        private static MarceneiroAdminDTO MapMarceneiroAdmin(Marceneiro m) => new()
        {
            Id = m.Id,
            Nome = m.Nome,
            Email = m.Email,
            Telefone = m.Telefone,
            FotoUrl = m.FotoUrl,
            Cidade = m.Cidade,
            Estado = m.Estado,
            Bairro = m.Bairro,
            Especialidades = string.IsNullOrWhiteSpace(m.Especialidades)
                ? new List<string>()
                : m.Especialidades.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Bio = m.Bio,
            NotaMedia = m.NotaMedia,
            TotalAvaliacoes = m.TotalAvaliacoes,
            TotalServicos = m.TotalServicos,
            Disponivel = m.Disponivel,
            Verificado = m.Verificado,
            OrigemExterna = m.OrigemExterna,
            DataCadastro = m.DataCadastro
        };

        [HttpGet("projetos/{id:int}/operacao")]
        public async Task<ActionResult<ProjetoOperacaoAdminDTO>> ObterProjetoOperacao(int id)
        {
            var dto = await _operacao.ObterProjetoOperacaoAsync(id);
            return dto == null ? NotFound() : Ok(dto);
        }

        [HttpPost("projetos/{id:int}/alocar-montador")]
        public async Task<IActionResult> AlocarMontador(int id, [FromBody] AlocarMontadorDTO dto)
        {
            var resultado = await _operacao.AlocarMontadorAsync(id, dto);
            return resultado switch
            {
                ResultadoOperacaoManual.Ok => Ok(new { mensagem = "Montador alocado com sucesso." }),
                ResultadoOperacaoManual.NaoEncontrado => NotFound(new { erro = "Projeto ou montador não encontrado." }),
                ResultadoOperacaoManual.StatusInvalido => BadRequest(new { erro = "Status do projeto não permite alocação." }),
                _ => BadRequest()
            };
        }

        [HttpPost("projetos/{id:int}/marcar-montagem-concluida")]
        public async Task<IActionResult> MarcarMontagemConcluida(int id)
        {
            var resultado = await _operacao.MarcarMontagemConcluidaAsync(id);
            return resultado switch
            {
                ResultadoOperacaoManual.Ok => Ok(new { mensagem = "Montagem marcada como concluída. Arquiteto pode vistoriar." }),
                ResultadoOperacaoManual.NaoEncontrado => NotFound(),
                ResultadoOperacaoManual.StatusInvalido => BadRequest(new { erro = "Projeto precisa estar com montador alocado." }),
                _ => BadRequest()
            };
        }

        [HttpPost("conta/alterar-senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var (ok, erro) = await _auth.AlterarSenhaAsync(userId, dto.SenhaAtual, dto.SenhaNova);
            if (!ok)
                return BadRequest(new { erro });
            return Ok(new { mensagem = "Senha alterada com sucesso." });
        }
    }
}
