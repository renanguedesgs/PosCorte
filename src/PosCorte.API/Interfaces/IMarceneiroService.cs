using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Interfaces
{
    public interface IMarceneiroService
    {
        Task<IEnumerable<Marceneiro>> ListarAsync(string? cidade, string? especialidade, decimal? notaMin, bool? disponivel);
        Task<Marceneiro?> ObterAsync(int id);
        Task<IEnumerable<Avaliacao>> ListarAvaliacoesAsync(int marceneiroId);
        Task<Avaliacao?> AvaliarAsync(int marceneiroId, int nota, string comentario, string autorNome, int? projetoId);

        /// <summary>
        /// Auto-cadastro público: o próprio montador entra na rede como pendente
        /// (Verificado=false, Disponivel=false). Deduplica por telefone/e-mail.
        /// </summary>
        Task<(Marceneiro? marceneiro, ResultadoAutoCadastro resultado)> AutoCadastrarAsync(AutoCadastroMarceneiroDTO dto);

        /// <summary>Lista para o admin (inclui contato, origem e pendentes). filtroVerificado=null traz todos.</summary>
        Task<IEnumerable<Marceneiro>> ListarParaAdminAsync(bool? verificado);

        /// <summary>Homologa o montador (Verificado=true, Disponivel=true). Dispara boas-vindas.</summary>
        Task<bool> VerificarAsync(int id);

        /// <summary>Alterna a disponibilidade (Disponível/Ocupado). Retorna o novo estado.</summary>
        Task<(bool ok, bool disponivel)> AlternarDisponibilidadeAsync(int id);

        /// <summary>
        /// Escolhe o melhor marceneiro disponível priorizando mesma cidade,
        /// especialidade compatível e maior nota média.
        /// </summary>
        Task<Marceneiro?> EscolherMelhorAsync(string? cidade, string? especialidade);

        /// <summary>
        /// Aloca o melhor marceneiro para um serviço, registrando a contratação
        /// (incrementa o total de serviços). Usado na confirmação de pagamento.
        /// </summary>
        Task<Marceneiro?> AlocarParaProjetoAsync(string? cidade, string? especialidade);
    }
}
