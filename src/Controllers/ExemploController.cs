using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Porta de entrada HTTP. So cuida de request e response: le a rota, chama o
    /// Service e escolhe o status code.
    /// </summary>
    /// <remarks>
    /// Sem regra de negocio e sem EF Core aqui. <c>[ApiController]</c> ja devolve 400
    /// sozinho quando o DTO fura os DataAnnotations, entao nao ha validacao manual.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class ExemploController : ControllerBase
    {
        private readonly ExemploService _service;

        public ExemploController(ExemploService service)
        {
            _service = service;
        }

        /// <summary>GET /api/exemplo</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool apenasAtivos = false)
        {
            var exemplos = await _service.GetAllAsync(apenasAtivos);
            return Ok(exemplos);
        }

        /// <summary>GET /api/exemplo/5</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var exemplo = await _service.GetByIdAsync(id);
            if (exemplo is null)
            {
                return NotFound(new { mensagem = $"Exemplo {id} nao encontrado." });
            }

            return Ok(exemplo);
        }

        /// <summary>POST /api/exemplo</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExemploCreateDTO dto)
        {
            var criado = await _service.CreateAsync(dto);

            // 201 + header Location apontando para o recurso recem-criado.
            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        }

        /// <summary>PUT /api/exemplo/5</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExemploCreateDTO dto)
        {
            var atualizado = await _service.UpdateAsync(id, dto);
            if (atualizado is null)
            {
                return NotFound(new { mensagem = $"Exemplo {id} nao encontrado." });
            }

            return Ok(atualizado);
        }

        /// <summary>DELETE /api/exemplo/5</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _service.DeleteAsync(id))
            {
                return NotFound(new { mensagem = $"Exemplo {id} nao encontrado." });
            }

            return NoContent();
        }
    }
}
