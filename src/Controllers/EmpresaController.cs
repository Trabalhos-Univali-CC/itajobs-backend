using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas de empresa. As vagas de uma empresa saem em
    /// <c>GET /api/vaga?idEmpresa={id}</c>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresaController : ControllerBase
    {
        private readonly EmpresaService _service;

        public EmpresaController(EmpresaService service)
        {
            _service = service;
        }

        /// <summary>GET /api/empresa</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        /// <summary>GET /api/empresa/5</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var empresa = await _service.GetByIdAsync(id);
            if (empresa is null)
            {
                return NotFound(new { mensagem = $"Empresa {id} nao encontrada." });
            }

            return Ok(empresa);
        }

        /// <summary>POST /api/empresa</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmpresaCreateDTO dto)
        {
            var criada = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
        }

        /// <summary>PUT /api/empresa/5</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmpresaCreateDTO dto)
        {
            var atualizada = await _service.UpdateAsync(id, dto);
            if (atualizada is null)
            {
                return NotFound(new { mensagem = $"Empresa {id} nao encontrada." });
            }

            return Ok(atualizada);
        }

        /// <summary>PATCH /api/empresa/5/nota</summary>
        [HttpPatch("{id:int}/nota")]
        public async Task<IActionResult> AtualizarNota(int id, [FromBody] NotaDTO dto)
        {
            var atualizada = await _service.AtualizarNotaAsync(id, dto.Nota!.Value);
            if (atualizada is null)
            {
                return NotFound(new { mensagem = $"Empresa {id} nao encontrada." });
            }

            return Ok(atualizada);
        }

        /// <summary>DELETE /api/empresa/5</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _service.DeleteAsync(id))
            {
                return NotFound(new { mensagem = $"Empresa {id} nao encontrada." });
            }

            return NoContent();
        }
    }
}
