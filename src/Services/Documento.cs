namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de CPF e CNPJ: tirar a mascara e conferir os digitos verificadores.
    /// </summary>
    public static class Documento
    {
        public static string NormalizarCpf(string cpf)
        {
            return new string(cpf.Where(char.IsAsciiDigit).ToArray());
        }

        /// <summary>
        /// Mantem letras: desde julho de 2026 a Receita emite CNPJ alfanumerico
        /// (12 primeiras posicoes podem ser letras, os 2 verificadores seguem numericos).
        /// </summary>
        public static string NormalizarCnpj(string cnpj)
        {
            return new string(cnpj.ToUpperInvariant().Where(char.IsAsciiLetterOrDigit).ToArray());
        }

        public static bool CpfValido(string cpf)
        {
            if (cpf.Length != 11 || !cpf.All(char.IsAsciiDigit) || cpf.Distinct().Count() == 1)
            {
                return false;
            }

            return DigitoVerificador(cpf[..9], [10, 9, 8, 7, 6, 5, 4, 3, 2]) == cpf[9] - '0'
                && DigitoVerificador(cpf[..10], [11, 10, 9, 8, 7, 6, 5, 4, 3, 2]) == cpf[10] - '0';
        }

        public static bool CnpjValido(string cnpj)
        {
            if (cnpj.Length != 14
                || !cnpj[..12].All(c => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c))
                || !cnpj[12..].All(char.IsAsciiDigit)
                || cnpj.Distinct().Count() == 1)
            {
                return false;
            }

            return DigitoVerificador(cnpj[..12], [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]) == cnpj[12] - '0'
                && DigitoVerificador(cnpj[..13], [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]) == cnpj[13] - '0';
        }

        /// <summary>
        /// Modulo 11. O valor de cada caractere e o codigo ASCII menos 48: da 0-9 para
        /// digitos e 17-42 para letras, que e a regra do CNPJ alfanumerico.
        /// </summary>
        private static int DigitoVerificador(string caracteres, int[] pesos)
        {
            var soma = 0;
            for (var i = 0; i < pesos.Length; i++)
            {
                soma += (caracteres[i] - '0') * pesos[i];
            }

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
}
