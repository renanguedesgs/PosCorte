namespace PosCorte.Domain.Validation
{
    public static class CepBrasil
    {
        public static bool EhValido(string? cep)
        {
            var digits = DocumentoBrasil.SoDigitos(cep);
            return digits.Length == 8 && digits != "00000000";
        }
    }
}
