using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas da candidatura. Ficam penduradas na vaga e no candidato porque a
    /// candidatura nao tem id proprio: e o par (vaga, candidato).
    /// </summary>
    /// <remarks>
    /// Acesso: as candidaturas de uma vaga e a mudanca de status, so a empresa dona;
    /// as de um candidato, so ele; candidatar, so o proprio candidato; desistir, o
    /// candidato ou a empresa dona da vaga.
    /// </remarks>
    [ApiController]
    public class CandidaturaController : ControllerBase
    {
        private readonly CandidaturaService _service;
        private readonly VagaService _vagaService;

        public CandidaturaController(CandidaturaService service, VagaService vagaService)
        {
            _service = service;
            _vagaService = vagaService;
        }

        /// <summary>GET /api/vaga/5/candidaturas?status=emAnalise</summary>
        [HttpGet("api/vaga/{idVaga:int}/candidaturas")]
        public async Task<IActionResult> GetByVaga(int idVaga, [FromQuery] StatusCandidatura? status)
        {
            var vaga = await _vagaService.GetByIdAsync(idVaga);
            if (vaga is null)
            {
                return NotFound(new { mensagem = $"Vaga {idVaga} nao encontrada." });
            }

            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(vaga.IdEmpresa));

            var candidaturas = await _service.GetByVagaAsync(idVaga, status);
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
            UsuarioLogado.Exigir(User.Usuario().EhCandidato(idCandidato));

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
            UsuarioLogado.Exigir(User.Usuario().EhCandidato(dto.IdCandidato!.Value));

            var criada = await _service.CreateAsync(idVaga, dto);
            if (criada is null)
            {
                return NotFound(new { mensagem = $"Vaga {idVaga} nao encontrada." });
            }

            return CreatedAtAction(nameof(GetByVaga), new { idVaga }, criada);
        }

        /// <summary>PATCH /api/vaga/5/candidaturas/3/status</summary>
        [HttpPatch("api/vaga/{idVaga:int}/candidaturas/{idCandidato:int}/status")]
        public async Task<IActionResult> AlterarStatus(int idVaga, int idCandidato, [FromBody] CandidaturaStatusDTO dto)
        {
            var vaga = await _vagaService.GetByIdAsync(idVaga);
            if (vaga is null)
            {
                return NotFound(new { mensagem = $"Vaga {idVaga} nao encontrada." });
            }

            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(vaga.IdEmpresa));

            var atualizada = await _service.AlterarStatusAsync(idVaga, idCandidato, dto.Status!.Value);
            if (atualizada is null)
            {
                return NotFound(new { mensagem = $"O candidato {idCandidato} nao se candidatou a vaga {idVaga}." });
            }

            return Ok(atualizada);
        }

        /// <summary>DELETE /api/vaga/5/candidaturas/3</summary>
        [HttpDelete("api/vaga/{idVaga:int}/candidaturas/{idCandidato:int}")]
        public async Task<IActionResult> Delete(int idVaga, int idCandidato)
        {
            var usuario = User.Usuario();
            var vaga = await _vagaService.GetByIdAsync(idVaga);
            UsuarioLogado.Exigir(usuario.EhCandidato(idCandidato) || (vaga is not null && usuario.EhEmpresa(vaga.IdEmpresa)));

            if (!await _service.DeleteAsync(idVaga, idCandidato))
            {
                return NotFound(new { mensagem = $"O candidato {idCandidato} nao se candidatou a vaga {idVaga}." });
            }

            return NoContent();
        }
    }
}
