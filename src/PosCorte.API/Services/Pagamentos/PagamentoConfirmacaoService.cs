using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services.Pagamentos
{
    public class PagamentoConfirmacaoService : IPagamentoConfirmacaoService
    {
        private readonly ILogger<PagamentoConfirmacaoService> _logger;
        private readonly IRepositorio<Projeto> _projetoRepo;
        private readonly IRepositorio<OrdemServico> _ordemRepo;
        private readonly IPagamentoService _pagamentoService;
        private readonly IProvedorService _provedorService;
        private readonly INotificacaoService _notificacaoService;

        public PagamentoConfirmacaoService(
            ILogger<PagamentoConfirmacaoService> logger,
            IRepositorio<Projeto> projetoRepo,
            IRepositorio<OrdemServico> ordemRepo,
            IPagamentoService pagamentoService,
            IProvedorService provedorService,
            INotificacaoService notificacaoService)
        {
            _logger = logger;
            _projetoRepo = projetoRepo;
            _ordemRepo = ordemRepo;
            _pagamentoService = pagamentoService;
            _provedorService = provedorService;
            _notificacaoService = notificacaoService;
        }

        public async Task<bool> ConfirmarPagamentoAsync(int projetoId, string pixId, decimal valor)
        {
            var projeto = await _projetoRepo.GetByIdAsync(projetoId);
            if (projeto == null)
            {
                _logger.LogWarning("Confirmar pagamento: projeto {Id} não encontrado", projetoId);
                return false;
            }

            if (projeto.StatusProjeto != "Aguardando_Pagamento" &&
                projeto.StatusProjeto != "Pagamento_Confirmado")
            {
                _logger.LogInformation("Projeto {Id} já processado (status {Status})", projetoId, projeto.StatusProjeto);
                return true;
            }

            var ordens = await _ordemRepo.GetAllAsync();
            if (ordens.Any(o => o.ProjetoId == projetoId))
            {
                _logger.LogInformation("Projeto {Id} já possui ordem de serviço", projetoId);
                return true;
            }

            if (!await _pagamentoService.ValidarPagamentoPixAsync(pixId, valor))
                return false;

            if (!await _pagamentoService.ReservarFundosAsync(pixId, valor))
                return false;

            projeto.StatusProjeto = "Pagamento_Confirmado";
            await _notificacaoService.NotificarEventoAsync(NotificacaoEvento.PagamentoConfirmado, projetoId,
                $"Pagamento confirmado para {projeto.NomeProjeto}. Alocando montador.");

            var request = new ProvedorRequest
            {
                NomeProjeto = projeto.NomeProjeto,
                EnderecoCompleto = projeto.EnderecoCompleto,
                Cep = projeto.CepObra,
                DataAgendamento = DateTime.UtcNow.AddDays(1),
                ValorTotal = valor,
                UrlPlano = projeto.UrlArquivoCorteCloud
            };

            string externalId = $"PC-{Guid.NewGuid():N}";
            string statusProvedor = "Aguardando_Provedor";
            string montadorNome = string.Empty;
            string montadorTelefone = string.Empty;
            string novoStatusProjeto = "Aguardando_Provedor";

            if (_provedorService.EstaConfigurado)
            {
                try
                {
                    var provedorResponse = await _provedorService.CriarOrdemServicoAsync(request);

                    if (!string.IsNullOrWhiteSpace(provedorResponse.ExternalProviderId))
                        externalId = provedorResponse.ExternalProviderId;

                    montadorNome = provedorResponse.MontadorNome ?? string.Empty;
                    montadorTelefone = provedorResponse.MontadorTelefone ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(montadorNome))
                    {
                        statusProvedor = "Prestador_Alocado";
                        novoStatusProjeto = "Prestador_Alocado";
                        await _notificacaoService.NotificarMontador(montadorTelefone,
                            $"Nova montagem disponível: {projeto.NomeProjeto}. Plano: {projeto.UrlArquivoCorteCloud}");
                        await _notificacaoService.NotificarEventoAsync(NotificacaoEvento.MontadorAlocado, projetoId,
                            $"Montador {montadorNome} alocado para {projeto.NomeProjeto}.");
                    }
                    else
                    {
                        statusProvedor = string.IsNullOrWhiteSpace(provedorResponse.Status)
                            ? "Aguardando_Provedor" : provedorResponse.Status;
                    }
                }
                catch (Exception ex)
                {
                    statusProvedor = "Erro_Provedor";
                    novoStatusProjeto = "Aguardando_Provedor";
                    _logger.LogError(ex, "Falha ao criar ordem no provedor para projeto {ProjetoId}", projetoId);
                }
            }

            var ordem = new OrdemServico(projeto.Id, externalId)
            {
                StatusProvedor = statusProvedor,
                MontadorNome = montadorNome,
                MontadorTelefone = montadorTelefone,
                DataAgendamento = request.DataAgendamento
            };

            projeto.StatusProjeto = novoStatusProjeto;

            await _ordemRepo.AddAsync(ordem);
            await _ordemRepo.SaveChangesAsync();
            await _projetoRepo.UpdateAsync(projeto);
            await _projetoRepo.SaveChangesAsync();

            _logger.LogInformation("Pagamento confirmado e ordem criada para projeto {ProjetoId}", projetoId);
            return true;
        }
    }
}
