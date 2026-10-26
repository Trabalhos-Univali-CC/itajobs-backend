using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
    /// </remarks>
    public class RegraNegocioFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not RegraNegocioException erro)
            {
                return;
            }

            var status = erro.Tipo switch
            {
                TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
                TipoErro.Conflito => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            context.Result = new ObjectResult(new { mensagem = erro.Message }) { StatusCode = status };
            context.ExceptionHandled = true;
        }
    }
}
