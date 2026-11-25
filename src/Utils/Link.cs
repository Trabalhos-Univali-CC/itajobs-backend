using Itajobs_API.src.Services;

namespace Itajobs_API.src.Utils
{
    /// <summary>
    /// Validacao de links informados pelo usuario (portfolio, site, LinkedIn). So
    /// http/https: bloqueia <c>javascript:</c>, <c>data:</c> e afins, que virariam XSS
    /// quando o front renderizar o link.
    /// </summary>
    public static class Link
    {
        /// <summary>Vazio = <c>null</c>. Senao, o link limpo, ou 400 se nao for http/https.</summary>
        /// <param name="campo">Nome do campo na mensagem de erro.</param>
        /// <param name="dominio">Se informado, o host precisa ser esse dominio ou um subdominio dele.</param>
        public static string? Validar(string? link, string campo, string? dominio = null)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return null;
            }

            var limpo = link.Trim();

            if (!Uri.TryCreate(limpo, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrEmpty(uri.Host))
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    $"O {campo} precisa ser um link comecando com http:// ou https://.");
            }

            if (dominio is not null
                && !uri.Host.Equals(dominio, StringComparison.OrdinalIgnoreCase)
                && !uri.Host.EndsWith("." + dominio, StringComparison.OrdinalIgnoreCase))
            {
                throw new RegraNegocioException(TipoErro.Invalido, $"O {campo} precisa ser um link do {dominio}.");
            }

            return limpo;
        }
    }
}
