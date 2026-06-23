namespace PosCorte.API.Interfaces
{
    public enum ResultadoVistoria
    {
        Ok,
        ProjetoNaoEncontrado,
        NaoAutorizado,
        StatusInvalido,
        FalhaLiquidacao
    }

    /// <summary>
    /// Vistoria e liquidação do escrow após a montagem (aprovação do arquiteto,
    /// disputa e liberação automática por prazo).
    /// </summary>
    public interface IVistoriaService
    {
        /// <summary>Arquiteto aprova a montagem → libera o escrow e conclui o projeto.</summary>
        Task<ResultadoVistoria> AprovarMontagemAsync(int projetoId, int usuarioId);

        /// <summary>Arquiteto abre disputa → congela o escrow até resolução do admin.</summary>
        Task<ResultadoVistoria> AbrirDisputaAsync(int projetoId, int usuarioId, string motivo);

        /// <summary>Marca a montagem como concluída e inicia a janela de vistoria (uso dev/teste).</summary>
        Task<ResultadoVistoria> MarcarMontagemConcluidaAsync(int projetoId, int usuarioId);

        /// <summary>Libera automaticamente projetos cuja janela de vistoria expirou. Retorna quantos foram liquidados.</summary>
        Task<int> LiquidarVencidosAsync(CancellationToken ct = default);
    }
}
