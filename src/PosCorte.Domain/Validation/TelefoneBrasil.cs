namespace PosCorte.Domain.Validation
{
    public static class TelefoneBrasil
    {
        public static bool EhValido(string? telefone)
        {
            var digits = DocumentoBrasil.SoDigitos(telefone);
            if (digits.Length is not (10 or 11)) return false;

            // DDD válido: 11–99 (sem 00)
            var ddd = int.Parse(digits[..2]);
            return ddd >= 11 && ddd <= 99;
        }
    }
}
