using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    /// <summary>
    /// Job em segundo plano: a cada intervalo, libera o escrow de projetos cuja
    /// janela de vistoria (72h) expirou sem aprovação nem disputa do arquiteto.
    /// </summary>
    public class LiquidacaoBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(30);

        private readonly IServiceProvider _services;
        private readonly ILogger<LiquidacaoBackgroundService> _logger;

        public LiquidacaoBackgroundService(IServiceProvider services, ILogger<LiquidacaoBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LiquidacaoBackgroundService iniciado (intervalo {Min} min).", Intervalo.TotalMinutes);

            // pequeno atraso inicial para a aplicação subir/migrar
            try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var vistoria = scope.ServiceProvider.GetRequiredService<IVistoriaService>();
                    await vistoria.LiquidarVencidosAsync(stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha no ciclo de liquidação automática");
                }

                try { await Task.Delay(Intervalo, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
