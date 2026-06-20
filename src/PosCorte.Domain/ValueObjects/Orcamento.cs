namespace PosCorte.Domain.ValueObjects
{
    public class Orcamento
    {
        public decimal ValorTotal { get; private set; }
        public decimal CustoPrestador { get; private set; }
        public decimal MargemLucro { get; private set; }
        public decimal TaxaPlataforma { get; private set; }

        public Orcamento(decimal valorTotal, decimal custoPrestador, decimal margemLucro, decimal taxaPlataforma)
        {
            if (valorTotal <= 0) throw new ArgumentException("Valor total deve ser maior que zero.");
            if (custoPrestador <= 0) throw new ArgumentException("Custo do prestador deve ser maior que zero.");

            ValorTotal = valorTotal;
            CustoPrestador = custoPrestador;
            MargemLucro = margemLucro;
            TaxaPlataforma = taxaPlataforma;
        }
    }
}
