using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas da candidatura. Ficam penduradas na vaga e no candidato porque a
    /// candidatura nao tem id proprio: e o par (vaga, candidato).
    /// </summary>
    [ApiController]
    public class CandidaturaController : ControllerBase
    {
        private readonly CandidaturaService _service;

        public CandidaturaController(CandidaturaService service)
        {
            _service = service;
        }

        /// <summary>GET /api/vaga/5/candidaturas</summary>
        [HttpGet("api/vaga/{idVaga:int}/candidaturas")]
        public async Task<IActionResult> GetByVaga(int idVaga)
        {
            var candidaturas = await _service.GetByVagaAsync(idVaga);
            if (candidaturas is null)
            {
                return NotFound(new { mensagem = $"Vaga {idVaga} nao encontrada." });
            }

            return Ok(candidaturas);
        }

        /// <summary>GET /api/candidato/5/candidaturas</summary>
        [HttpGet("api/candidato/{idCandidato:int}/candidaturas")]
        public async Task<IActionResult> GetByCandidato(int idCandidato)
        {
            var candidaturas = await _service.GetByCandidatoAsync(idCandidato);
            if (candidaturas is null)
            {
                return NotFound(new { mensagem = $"Candidato {idCandidato} nao encontrado." });
            }

            return Ok(candidaturas);
        }

        /// <summary>POST /api/vaga/5/candidaturas</summary>
        [HttpPost("api/vaga/{idVaga:int}/candidaturas")]
        public async Task<IActionResult> Create(int idVaga, [FromBody] CandidaturaCreateDTO dto)
        {
            var criada = await _service.CreateAsync(idVaga, dto);
            if (criada is null)
            {
                return NotFound(new { mensagem = $"Vaga {idVaga} nao encontrada." });
            }

            return CreatedAtAction(nameof(GetByVaga), new { idVaga }, criada);
        }

        /// <summary>DELETE /api/vaga/5/candidaturas/3</summary>
        [HttpDelete("api/vaga/{idVaga:int}/candidaturas/{idCandidato:int}")]
        public async Task<IActionResult> Delete(int idVaga, int idCandidato)
        {
            if (!await _service.DeleteAsync(idVaga, idCandidato))
            {
                return NotFound(new { mensagem = $"O candidato {idCandidato} nao se candidatou a vaga {idVaga}." });
            }

            return NoContent();
        }
    }
}
