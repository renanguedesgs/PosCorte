using PosCorte.API.Models.DTOs;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Interfaces
{
    public enum ResultadoOperacaoManual
    {
        Ok,
        NaoEncontrado,
        StatusInvalido,
        EmailDuplicado,
        MarceneiroIndisponivel
    }

    /// <summary>Cadastro manual e alocação de montadores (modelo operação fundador).</summary>
    public interface IOperacaoManualService
    {
        Task<(Marceneiro? marceneiro, ResultadoOperacaoManual resultado)> CadastrarMarceneiroAsync(CreateMarceneiroAdminDTO dto);
        Task<(CreateArquitetoAdminResponseDTO? response, ResultadoOperacaoManual resultado)> CadastrarArquitetoAsync(CreateArquitetoAdminDTO dto);
        Task<IReadOnlyList<ArquitetoAdminDTO>> ListarArquitetosAsync();
        Task<ProjetoOperacaoAdminDTO?> ObterProjetoOperacaoAsync(int projetoId);
        Task<ResultadoOperacaoManual> AlocarMontadorAsync(int projetoId, AlocarMontadorDTO dto);
        Task<ResultadoOperacaoManual> MarcarMontagemConcluidaAsync(int projetoId);
    }
}
