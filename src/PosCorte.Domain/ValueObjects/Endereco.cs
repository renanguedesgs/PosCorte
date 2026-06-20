namespace PosCorte.Domain.ValueObjects
{
    public class Endereco
    {
        public string Logradouro { get; private set; }
        public string Numero { get; private set; }
        public string Complemento { get; private set; }
        public string Bairro { get; private set; }
        public string Cidade { get; private set; }
        public string Estado { get; private set; }
        public string Cep { get; private set; }

        public Endereco(string logradouro, string numero, string bairro, string cidade, string estado, string cep, string complemento = "")
        {
            if (string.IsNullOrWhiteSpace(logradouro)) throw new ArgumentException("Logradouro é obrigatório.");
            if (string.IsNullOrWhiteSpace(cep)) throw new ArgumentException("CEP é obrigatório.");

            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            Cep = cep.Replace("-", "").Replace(".", "");
        }

        public override string ToString() =>
            $"{Logradouro}, {Numero}{(string.IsNullOrEmpty(Complemento) ? "" : $" - {Complemento}")}, {Bairro}, {Cidade}/{Estado} - CEP: {Cep}";
    }
}
