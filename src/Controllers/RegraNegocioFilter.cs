using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Converte <see cref="RegraNegocioException"/> em resposta HTTP, para nenhum
    /// Controller precisar de try/catch.
    /// </summary>
    /// <remarks>
    /// Registrado para todos os controllers no <c>Program.cs</c>. A resposta tem o
    /// mesmo formato dos 404 dos controllers: <c>{ "mensagem": "..." }</c>.
    /// <para>
    /// Tambem cobre a corrida que os Services nao pegam: duas requests ao mesmo tempo
    /// passam pela checagem ("e-mail livre?") e a segunda bate no indice unico do banco.
    /// Em vez de 500, sai 409 (UNIQUE) ou 400 (CHECK).
    /// </para>
    /// </remarks>
    public class RegraNegocioFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var (status, mensagem) = context.Exception switch
            {
                RegraNegocioException erro => (StatusDe(erro.Tipo), erro.Message),

                DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
                    (StatusCodes.Status409Conflict, "Ja existe um registro com estes dados."),

                DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.CheckViolation } } =>
                    (StatusCodes.Status400BadRequest, "Os dados enviados nao respeitam as regras do cadastro."),

                _ => (0, string.Empty)
            };

            if (status == 0)
            {
                return;
            }

            context.Result = new ObjectResult(new { mensagem }) { StatusCode = status };
            context.ExceptionHandled = true;
        }

        private static int StatusDe(TipoErro tipo) => tipo switch
        {
            TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErro.Conflito => StatusCodes.Status409Conflict,
            TipoErro.NaoAutenticado => StatusCodes.Status401Unauthorized,
            TipoErro.Proibido => StatusCodes.Status403Forbidden,
            TipoErro.Indisponivel => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
