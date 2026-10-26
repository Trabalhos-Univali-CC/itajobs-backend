using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas de curriculo e dos itens dele (experiencias e habilidades).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CurriculoController : ControllerBase
    {
        private readonly CurriculoService _service;

        public CurriculoController(CurriculoService service)
        {
            _service = service;
        }

        /// <summary>GET /api/curriculo/5</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var curriculo = await _service.GetByIdAsync(id);
            if (curriculo is null)
            {
                return NotFound(new { mensagem = $"Curriculo {id} nao encontrado." });
            }

            return Ok(curriculo);
        }

        /// <summary>GET /api/candidato/5/curriculo</summary>
        [HttpGet("/api/candidato/{idCandidato:int}/curriculo")]
        public async Task<IActionResult> GetByCandidato(int idCandidato)
        {
            var curriculo = await _service.GetByCandidatoAsync(idCandidato);
            if (curriculo is null)
            {
                return NotFound(new { mensagem = $"O candidato {idCandidato} nao tem curriculo." });
            }

            return Ok(curriculo);
        }

        /// <summary>POST /api/curriculo</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CurriculoCreateDTO dto)
        {
            var criado = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        }

        /// <summary>DELETE /api/curriculo/5</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _service.DeleteAsync(id))
            {
                return NotFound(new { mensagem = $"Curriculo {id} nao encontrado." });
            }

            return NoContent();
        }

        /// <summary>POST /api/curriculo/5/experiencias</summary>
        [HttpPost("{id:int}/experiencias")]
        public async Task<IActionResult> AddExperiencia(int id, [FromBody] ExperienciaCreateDTO dto)
        {
            var criada = await _service.AddExperienciaAsync(id, dto);
            if (criada is null)
            {
                return NotFound(new { mensagem = $"Curriculo {id} nao encontrado." });
            }

            return CreatedAtAction(nameof(GetById), new { id }, criada);
        }

        /// <summary>PUT /api/curriculo/5/experiencias/2</summary>
        [HttpPut("{id:int}/experiencias/{idExperiencia:int}")]
        public async Task<IActionResult> UpdateExperiencia(int id, int idExperiencia, [FromBody] ExperienciaCreateDTO dto)
        {
            var atualizada = await _service.UpdateExperienciaAsync(id, idExperiencia, dto);
            if (atualizada is null)
            {
                return NotFound(new { mensagem = $"Experiencia {idExperiencia} nao encontrada no curriculo {id}." });
            }

            return Ok(atualizada);
        }

        /// <summary>DELETE /api/curriculo/5/experiencias/2</summary>
        [HttpDelete("{id:int}/experiencias/{idExperiencia:int}")]
        public async Task<IActionResult> DeleteExperiencia(int id, int idExperiencia)
        {
            if (!await _service.DeleteExperienciaAsync(id, idExperiencia))
            {
                return NotFound(new { mensagem = $"Experiencia {idExperiencia} nao encontrada no curriculo {id}." });
            }

            return NoContent();
        }

        /// <summary>POST /api/curriculo/5/habilidades</summary>
        [HttpPost("{id:int}/habilidades")]
        public async Task<IActionResult> AddHabilidade(int id, [FromBody] HabilidadeCreateDTO dto)
        {
            var criada = await _service.AddHabilidadeAsync(id, dto);
            if (criada is null)
            {
                return NotFound(new { mensagem = $"Curriculo {id} nao encontrado." });
            }

            return CreatedAtAction(nameof(GetById), new { id }, criada);
        }

        /// <summary>DELETE /api/curriculo/5/habilidades/3</summary>
        [HttpDelete("{id:int}/habilidades/{idHabilidade:int}")]
        public async Task<IActionResult> DeleteHabilidade(int id, int idHabilidade)
        {
            if (!await _service.DeleteHabilidadeAsync(id, idHabilidade))
            {
                return NotFound(new { mensagem = $"Habilidade {idHabilidade} nao encontrada no curriculo {id}." });
            }

            return NoContent();
        }
    }
}
