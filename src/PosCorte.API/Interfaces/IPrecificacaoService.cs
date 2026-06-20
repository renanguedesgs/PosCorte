using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Interfaces
{
    public interface IPrecificacaoService
    {
        OrcamentoResultado ProcessarProjeto(int pecas, int gavetas);
    }
}
