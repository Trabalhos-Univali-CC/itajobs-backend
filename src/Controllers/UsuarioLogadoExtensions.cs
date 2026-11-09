using System.Security.Claims;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Le o <see cref="UsuarioLogado"/> das claims do JWT ja validado.
    /// </summary>
    public static class UsuarioLogadoExtensions
    {
        /// <remarks>So chame em rota com <c>[Authorize]</c> (o padrao da API).</remarks>
        public static UsuarioLogado Usuario(this ClaimsPrincipal principal)
        {
            var perfil = principal.FindFirstValue(JwtOptions.ClaimPerfil);
            var id = principal.FindFirstValue(JwtOptions.ClaimId);

            if (perfil is null || !int.TryParse(id, out var idUsuario))
            {
                throw new RegraNegocioException(TipoErro.NaoAutenticado, "Token sem os dados do usuario. Faca login de novo.");
            }

            return new UsuarioLogado(perfil, idUsuario, principal.FindFirstValue(JwtOptions.ClaimNome) ?? string.Empty);
        }
    }
}
