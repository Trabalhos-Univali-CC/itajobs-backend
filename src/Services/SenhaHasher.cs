using Microsoft.AspNetCore.Identity;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Hash de senha. Usa o <see cref="PasswordHasher{TUser}"/> do ASP.NET Core
    /// Identity (PBKDF2 com salt aleatorio), sem puxar o resto do Identity.
    /// </summary>
    public static class SenhaHasher
    {
        // O hasher nao usa o "usuario" para nada; o parametro existe so na assinatura.
        private static readonly PasswordHasher<object> Hasher = new();
        private static readonly object Ninguem = new();

        public static string Gerar(string senha)
        {
            return Hasher.HashPassword(Ninguem, senha);
        }

        /// <summary>Hash vazio (conta criada antes da autenticacao) nunca confere.</summary>
        public static bool Confere(string hash, string senha)
        {
            if (string.IsNullOrEmpty(hash))
            {
                return false;
            }

            return Hasher.VerifyHashedPassword(Ninguem, hash, senha) != PasswordVerificationResult.Failed;
        }
    }
}
