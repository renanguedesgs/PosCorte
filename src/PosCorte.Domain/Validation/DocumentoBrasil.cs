namespace PosCorte.Domain.Validation
{
    public static class DocumentoBrasil
    {
        public static string SoDigitos(string? valor)
            => string.IsNullOrEmpty(valor) ? string.Empty : string.Concat(valor.Where(char.IsDigit));

        public static bool EhCpfValido(string cpf)
        {
            cpf = SoDigitos(cpf);
            if (cpf.Length != 11) return false;
            if (cpf.All(c => c == cpf[0])) return false;

            var d1 = CalcularDigito(cpf, 9, 10);
            if (cpf[9] - '0' != d1) return false;

            var d2 = CalcularDigito(cpf, 10, 11);
            return cpf[10] - '0' == d2;
        }

        public static bool EhCnpjValido(string cnpj)
        {
            cnpj = SoDigitos(cnpj);
            if (cnpj.Length != 14) return false;
            if (cnpj.All(c => c == cnpj[0])) return false;

            int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var sum = 0;
            for (var i = 0; i < 12; i++)
                sum += (cnpj[i] - '0') * mult1[i];
            var d1 = sum % 11 < 2 ? 0 : 11 - sum % 11;
            if (cnpj[12] - '0' != d1) return false;

            sum = 0;
            for (var i = 0; i < 13; i++)
                sum += (cnpj[i] - '0') * mult2[i];
            var d2 = sum % 11 < 2 ? 0 : 11 - sum % 11;
            return cnpj[13] - '0' == d2;
        }

        public static bool EhCpfOuCnpjValido(string documento)
        {
            var digits = SoDigitos(documento);
            return digits.Length == 11 && EhCpfValido(digits)
                || digits.Length == 14 && EhCnpjValido(digits);
        }

        private static int CalcularDigito(string digits, int length, int pesoInicial)
        {
            var sum = 0;
            for (var i = 0; i < length; i++)
                sum += (digits[i] - '0') * (pesoInicial - i);
            var mod = sum % 11;
            return mod < 2 ? 0 : 11 - mod;
        }
    }
}
