using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Services
{
    public class PrecificacaoService : IPrecificacaoService
    {
        private const decimal CUSTO_FIXO_PECA = 12.50m;
        private const decimal CUSTO_FIXO_GAVETA = 40.00m;
        private const decimal MARKUP_PLATAFORMA = 0.20m;

        private readonly ILogger<PrecificacaoService> _logger;

        public PrecificacaoService(ILogger<PrecificacaoService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processa orçamento utilizando fórmula de Markup Inverso
        /// Preço Final = Custo / (1 - Taxa)
        /// </summary>
        public OrcamentoResultado ProcessarProjeto(int pecas, int gavetas)
        {
            _logger.LogInformation("Processando orçamento: {Pecas} peças, {Gavetas} gavetas", pecas, gavetas);

            if (pecas < 0 || gavetas < 0)
                throw new ArgumentException("Quantidade de peças e gavetas não podem ser negativas.");

            if (pecas == 0 && gavetas == 0)
                throw new ArgumentException("Projeto deve ter pelo menos uma peça ou gaveta.");

            decimal custoMaoDeObra = (pecas * CUSTO_FIXO_PECA) + (gavetas * CUSTO_FIXO_GAVETA);

            // Markup Inverso: Preço Final = Custo / (1 - Taxa)
            decimal precoFinal = custoMaoDeObra / (1 - MARKUP_PLATAFORMA);
            decimal margemLucro = precoFinal - custoMaoDeObra;

            var resultado = new OrcamentoResultado
            {
                ValorTotal = Math.Round(precoFinal, 2),
                CustoPrestador = Math.Round(custoMaoDeObra, 2),
                MargemLucro = Math.Round(margemLucro, 2),
                TaxaPlataforma = Math.Round(MARKUP_PLATAFORMA * 100, 2)
            };

            _logger.LogInformation("Orçamento calculado: Total R${ValorTotal}, Margem R${MargemLucro}", resultado.ValorTotal, resultado.MargemLucro);

            return resultado;
        }
    }
}
