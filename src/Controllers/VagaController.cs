using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas de vaga. As candidaturas de uma vaga ficam no
    /// <see cref="CandidaturaController"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VagaController : ControllerBase
    {
        private readonly VagaService _service;

        public VagaController(VagaService service)
        {
            _service = service;
        }

        /// <summary>GET /api/vaga?idEmpresa=1&amp;cargo=dev</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? idEmpresa, [FromQuery] string? cargo)
        {
            return Ok(await _service.GetAllAsync(idEmpresa, cargo));
        }

        /// <summary>GET /api/vaga/5</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vaga = await _service.GetByIdAsync(id);
            if (vaga is null)
            {
                return NotFound(new { mensagem = $"Vaga {id} nao encontrada." });
            }

            return Ok(vaga);
        }

        /// <summary>POST /api/vaga</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VagaCreateDTO dto)
        {
            var criada = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
        }

        /// <summary>PUT /api/vaga/5</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] VagaCreateDTO dto)
        {
            var atualizada = await _service.UpdateAsync(id, dto);
            if (atualizada is null)
            {
                return NotFound(new { mensagem = $"Vaga {id} nao encontrada." });
            }

            return Ok(atualizada);
        }

        /// <summary>DELETE /api/vaga/5</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _service.DeleteAsync(id))
            {
                return NotFound(new { mensagem = $"Vaga {id} nao encontrada." });
            }

            return NoContent();
        }
    }
}
