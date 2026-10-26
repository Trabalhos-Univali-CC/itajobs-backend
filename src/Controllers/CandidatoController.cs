using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas de candidato. O curriculo e as candidaturas dele ficam nos controllers
    /// proprios (<c>/api/candidato/{id}/curriculo</c> e <c>/api/candidato/{id}/candidaturas</c>).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CandidatoController : ControllerBase
    {
        private readonly CandidatoService _service;

        public CandidatoController(CandidatoService service)
        {
            _service = service;
        }

        /// <summary>GET /api/candidato</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        /// <summary>GET /api/candidato/5</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var candidato = await _service.GetByIdAsync(id);
            if (candidato is null)
            {
                return NotFound(new { mensagem = $"Candidato {id} nao encontrado." });
            }

            return Ok(candidato);
        }

        /// <summary>POST /api/candidato</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CandidatoCreateDTO dto)
        {
            var criado = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        }

        /// <summary>PUT /api/candidato/5</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CandidatoCreateDTO dto)
        {
            var atualizado = await _service.UpdateAsync(id, dto);
            if (atualizado is null)
            {
                return NotFound(new { mensagem = $"Candidato {id} nao encontrado." });
            }

            return Ok(atualizado);
        }

        /// <summary>DELETE /api/candidato/5</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _service.DeleteAsync(id))
            {
                return NotFound(new { mensagem = $"Candidato {id} nao encontrado." });
            }

            return NoContent();
        }
    }
}
