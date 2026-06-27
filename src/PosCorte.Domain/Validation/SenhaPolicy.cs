using System.Text.RegularExpressions;

namespace PosCorte.Domain.Validation
{
    public static class SenhaPolicy
    {
        public const int MinLength = 8;

        private static readonly Regex TemMinuscula = new(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex TemMaiuscula = new(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex TemNumero = new(@"[0-9]", RegexOptions.Compiled);
        private static readonly Regex TemEspecial = new(@"[^A-Za-z0-9]", RegexOptions.Compiled);

        public static bool EhSenhaForte(string? senha)
        {
            if (string.IsNullOrEmpty(senha) || senha.Length < MinLength) return false;
            return TemMinuscula.IsMatch(senha)
                && TemMaiuscula.IsMatch(senha)
                && TemNumero.IsMatch(senha)
                && TemEspecial.IsMatch(senha);
        }

        public static string? ObterErro(string? senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                return "Informe uma senha.";

            if (senha.Length < MinLength)
                return $"A senha deve ter pelo menos {MinLength} caracteres.";

            if (!TemMinuscula.IsMatch(senha))
                return "A senha deve conter pelo menos uma letra minúscula.";

            if (!TemMaiuscula.IsMatch(senha))
                return "A senha deve conter pelo menos uma letra maiúscula.";

            if (!TemNumero.IsMatch(senha))
                return "A senha deve conter pelo menos um número.";

            if (!TemEspecial.IsMatch(senha))
                return "A senha deve conter pelo menos um símbolo (ex.: ! @ # $).";

            return null;
        }

        public static int CalcularScore(string? senha)
        {
            if (string.IsNullOrEmpty(senha)) return 0;

            var score = 0;
            if (senha.Length >= MinLength) score++;
            if (TemMinuscula.IsMatch(senha) && TemMaiuscula.IsMatch(senha)) score++;
            if (TemNumero.IsMatch(senha)) score++;
            if (TemEspecial.IsMatch(senha)) score++;
            return score;
        }
    }
}
