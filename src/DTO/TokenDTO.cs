namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Resposta do login. O cliente manda <see cref="Token"/> nas proximas requests
    /// no header <c>Authorization: Bearer {token}</c>.
    /// </summary>
    public class TokenDTO
    {
        public string Token { get; set; } = string.Empty;

        /// <summary>Quando o token para de valer (UTC). Depois disso, login de novo.</summary>
        public DateTime ExpiraEm { get; set; }

        public UsuarioLogadoDTO Usuario { get; set; } = new();
    }

    /// <summary>
    /// Quem esta logado. Sai no login e no <c>GET /api/auth/me</c>.
    /// </summary>
    public class UsuarioLogadoDTO
    {
        /// <summary><c>candidato</c> ou <c>empresa</c>.</summary>
        public string Tipo { get; set; } = string.Empty;

        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
    }
}
