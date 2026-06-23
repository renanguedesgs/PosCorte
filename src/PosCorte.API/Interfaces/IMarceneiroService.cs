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
