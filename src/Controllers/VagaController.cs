using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas de vaga. As candidaturas de uma vaga ficam no
    /// <see cref="CandidaturaController"/>.
    /// </summary>
    /// <remarks>
    /// Acesso: buscar e ver sao publicos; criar, alterar, encerrar e apagar, so a
    /// empresa dona da vaga.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class VagaController : ControllerBase
    {
        private readonly VagaService _service;

        public VagaController(VagaService service)
        {
            _service = service;
        }

        /// <summary>GET /api/vaga?cargo=solda&amp;bairro=Centro&amp;tipoContrato=clt&amp;pagina=1 (ver <see cref="VagaFiltroDTO"/>)</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] VagaFiltroDTO filtro)
        {
            return Ok(await _service.BuscarAsync(filtro));
        }

        /// <summary>GET /api/vaga/5</summary>
        [AllowAnonymous]
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
            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(dto.IdEmpresa!.Value));

            var criada = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
        }

        /// <summary>PUT /api/vaga/5</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] VagaCreateDTO dto)
        {
            // O VagaService ja recusa trocar a empresa da vaga, entao basta conferir o corpo.
            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(dto.IdEmpresa!.Value));

            var atualizada = await _service.UpdateAsync(id, dto);
            if (atualizada is null)
            {
                return NotFound(new { mensagem = $"Vaga {id} nao encontrada." });
            }

            return Ok(atualizada);
        }

        /// <summary>PATCH /api/vaga/5/status</summary>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> AlterarStatus(int id, [FromBody] VagaStatusDTO dto)
        {
            var vaga = await _service.GetByIdAsync(id);
            if (vaga is null)
            {
                return NotFound(new { mensagem = $"Vaga {id} nao encontrada." });
            }

            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(vaga.IdEmpresa));

            return Ok(await _service.AlterarStatusAsync(id, dto.Status!.Value));
        }

        /// <summary>DELETE /api/vaga/5</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vaga = await _service.GetByIdAsync(id);
            if (vaga is null)
            {
                return NotFound(new { mensagem = $"Vaga {id} nao encontrada." });
            }

            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(vaga.IdEmpresa));

            if (!await _service.DeleteAsync(id))
            {
                return NotFound(new { mensagem = $"Vaga {id} nao encontrada." });
            }

            return NoContent();
        }
    }
}
