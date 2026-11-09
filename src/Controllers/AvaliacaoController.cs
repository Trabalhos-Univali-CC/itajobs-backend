using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas de avaliacao de empresa. Ficam penduradas na empresa e a avaliacao e
    /// identificada pelo par (empresa, candidato), como a candidatura.
    /// </summary>
    /// <remarks>
    /// Acesso: ver exige login; avaliar e apagar a avaliacao, so o proprio candidato.
    /// </remarks>
    [ApiController]
    public class AvaliacaoController : ControllerBase
    {
        private readonly AvaliacaoService _service;

        public AvaliacaoController(AvaliacaoService service)
        {
            _service = service;
        }

        /// <summary>GET /api/empresa/5/avaliacoes</summary>
        [HttpGet("api/empresa/{idEmpresa:int}/avaliacoes")]
        public async Task<IActionResult> GetByEmpresa(int idEmpresa)
        {
            var avaliacoes = await _service.GetByEmpresaAsync(idEmpresa);
            if (avaliacoes is null)
            {
                return NotFound(new { mensagem = $"Empresa {idEmpresa} nao encontrada." });
            }

            return Ok(avaliacoes);
        }

        /// <summary>PUT /api/empresa/5/avaliacoes/3 (cria ou substitui)</summary>
        [HttpPut("api/empresa/{idEmpresa:int}/avaliacoes/{idCandidato:int}")]
        public async Task<IActionResult> Avaliar(int idEmpresa, int idCandidato, [FromBody] AvaliacaoCreateDTO dto)
        {
            UsuarioLogado.Exigir(User.Usuario().EhCandidato(idCandidato));

            var avaliacao = await _service.AvaliarAsync(idEmpresa, idCandidato, dto);
            if (avaliacao is null)
            {
                return NotFound(new { mensagem = $"Empresa {idEmpresa} nao encontrada." });
            }

            return Ok(avaliacao);
        }

        /// <summary>DELETE /api/empresa/5/avaliacoes/3</summary>
        [HttpDelete("api/empresa/{idEmpresa:int}/avaliacoes/{idCandidato:int}")]
        public async Task<IActionResult> Delete(int idEmpresa, int idCandidato)
        {
            UsuarioLogado.Exigir(User.Usuario().EhCandidato(idCandidato));

            if (!await _service.DeleteAsync(idEmpresa, idCandidato))
            {
                return NotFound(new { mensagem = $"O candidato {idCandidato} nao avaliou a empresa {idEmpresa}." });
            }

            return NoContent();
        }
    }
}
