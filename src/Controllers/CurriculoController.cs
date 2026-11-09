using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Rotas de curriculo e dos itens dele (experiencias, habilidades, formacoes,
    /// cursos) e dos arquivos (PDF e foto).
    /// </summary>
    /// <remarks>
    /// Acesso: ver e baixar arquivos, o dono ou uma empresa; criar, alterar, apagar e
    /// mexer em itens e arquivos, so o candidato dono.
    /// </remarks>
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

            var usuario = User.Usuario();
            UsuarioLogado.Exigir(usuario.EhCandidato(curriculo.IdCandidato) || usuario.EhAlgumaEmpresa);

            return Ok(curriculo);
        }

        /// <summary>GET /api/candidato/5/curriculo</summary>
        [HttpGet("/api/candidato/{idCandidato:int}/curriculo")]
        public async Task<IActionResult> GetByCandidato(int idCandidato)
        {
            var usuario = User.Usuario();
            UsuarioLogado.Exigir(usuario.EhCandidato(idCandidato) || usuario.EhAlgumaEmpresa);

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
            UsuarioLogado.Exigir(User.Usuario().EhCandidato(dto.IdCandidato!.Value));

            var criado = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        }

        /// <summary>PUT /api/curriculo/5 (o resumo)</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CurriculoUpdateDTO dto)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            return Ok(await _service.UpdateAsync(id, dto));
        }

        /// <summary>DELETE /api/curriculo/5</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

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
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

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
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

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
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

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
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

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
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            if (!await _service.DeleteHabilidadeAsync(id, idHabilidade))
            {
                return NotFound(new { mensagem = $"Habilidade {idHabilidade} nao encontrada no curriculo {id}." });
            }

            return NoContent();
        }

        /// <summary>POST /api/curriculo/5/formacoes</summary>
        [HttpPost("{id:int}/formacoes")]
        public async Task<IActionResult> AddFormacao(int id, [FromBody] FormacaoCreateDTO dto)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            var criada = await _service.AddFormacaoAsync(id, dto);
            return CreatedAtAction(nameof(GetById), new { id }, criada);
        }

        /// <summary>PUT /api/curriculo/5/formacoes/2</summary>
        [HttpPut("{id:int}/formacoes/{idFormacao:int}")]
        public async Task<IActionResult> UpdateFormacao(int id, int idFormacao, [FromBody] FormacaoCreateDTO dto)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            var atualizada = await _service.UpdateFormacaoAsync(id, idFormacao, dto);
            if (atualizada is null)
            {
                return NotFound(new { mensagem = $"Formacao {idFormacao} nao encontrada no curriculo {id}." });
            }

            return Ok(atualizada);
        }

        /// <summary>DELETE /api/curriculo/5/formacoes/2</summary>
        [HttpDelete("{id:int}/formacoes/{idFormacao:int}")]
        public async Task<IActionResult> DeleteFormacao(int id, int idFormacao)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            if (!await _service.DeleteFormacaoAsync(id, idFormacao))
            {
                return NotFound(new { mensagem = $"Formacao {idFormacao} nao encontrada no curriculo {id}." });
            }

            return NoContent();
        }

        /// <summary>POST /api/curriculo/5/cursos</summary>
        [HttpPost("{id:int}/cursos")]
        public async Task<IActionResult> AddCurso(int id, [FromBody] CursoCreateDTO dto)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            var criado = await _service.AddCursoAsync(id, dto);
            return CreatedAtAction(nameof(GetById), new { id }, criado);
        }

        /// <summary>PUT /api/curriculo/5/cursos/2</summary>
        [HttpPut("{id:int}/cursos/{idCurso:int}")]
        public async Task<IActionResult> UpdateCurso(int id, int idCurso, [FromBody] CursoCreateDTO dto)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            var atualizado = await _service.UpdateCursoAsync(id, idCurso, dto);
            if (atualizado is null)
            {
                return NotFound(new { mensagem = $"Curso {idCurso} nao encontrado no curriculo {id}." });
            }

            return Ok(atualizado);
        }

        /// <summary>DELETE /api/curriculo/5/cursos/2</summary>
        [HttpDelete("{id:int}/cursos/{idCurso:int}")]
        public async Task<IActionResult> DeleteCurso(int id, int idCurso)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            if (!await _service.DeleteCursoAsync(id, idCurso))
            {
                return NotFound(new { mensagem = $"Curso {idCurso} nao encontrado no curriculo {id}." });
            }

            return NoContent();
        }

        /// <summary>
        /// PUT /api/curriculo/5/arquivos/pdf (ou /foto), multipart/form-data com o campo
        /// <c>arquivo</c>. Substitui o anterior do mesmo tipo.
        /// </summary>
        [HttpPut("{id:int}/arquivos/{tipo}")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> EnviarArquivo(int id, TipoArquivo tipo, IFormFile? arquivo)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            if (arquivo is null)
            {
                return BadRequest(new { mensagem = "Envie o arquivo no campo \"arquivo\" (multipart/form-data)." });
            }

            using var memoria = new MemoryStream();
            await arquivo.CopyToAsync(memoria);

            return Ok(await _service.SalvarArquivoAsync(id, tipo, arquivo.FileName, memoria.ToArray()));
        }

        /// <summary>GET /api/curriculo/5/arquivos/pdf (ou /foto). Devolve o arquivo em si.</summary>
        [HttpGet("{id:int}/arquivos/{tipo}")]
        public async Task<IActionResult> BaixarArquivo(int id, TipoArquivo tipo)
        {
            var curriculo = await _service.GetByIdAsync(id);
            if (curriculo is null)
            {
                return NotFound(new { mensagem = $"Curriculo {id} nao encontrado." });
            }

            var usuario = User.Usuario();
            UsuarioLogado.Exigir(usuario.EhCandidato(curriculo.IdCandidato) || usuario.EhAlgumaEmpresa);

            var arquivo = await _service.GetArquivoAsync(id, tipo);
            if (arquivo is null)
            {
                return NotFound(new { mensagem = $"O curriculo {id} nao tem {(tipo == TipoArquivo.Pdf ? "PDF" : "foto")}." });
            }

            // inline: o navegador mostra o PDF/foto em vez de forcar o download.
            Response.Headers[HeaderNames.ContentDisposition] =
                new ContentDispositionHeaderValue("inline") { FileNameStar = arquivo.Nome }.ToString();
            Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
            return File(arquivo.Conteudo, arquivo.ContentType);
        }

        /// <summary>DELETE /api/curriculo/5/arquivos/pdf (ou /foto)</summary>
        [HttpDelete("{id:int}/arquivos/{tipo}")]
        public async Task<IActionResult> DeleteArquivo(int id, TipoArquivo tipo)
        {
            if (await ExigirDonoAsync(id) is { } naoEncontrado)
            {
                return naoEncontrado;
            }

            if (!await _service.DeleteArquivoAsync(id, tipo))
            {
                return NotFound(new { mensagem = $"O curriculo {id} nao tem {(tipo == TipoArquivo.Pdf ? "PDF" : "foto")}." });
            }

            return NoContent();
        }

        /// <summary>404 se o curriculo nao existe; 403 (via excecao) se e de outro candidato.</summary>
        private async Task<IActionResult?> ExigirDonoAsync(int id)
        {
            var curriculo = await _service.GetByIdAsync(id);
            if (curriculo is null)
            {
                return NotFound(new { mensagem = $"Curriculo {id} nao encontrado." });
            }

            UsuarioLogado.Exigir(User.Usuario().EhCandidato(curriculo.IdCandidato));
            return null;
        }
    }
}
