using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Logo, banner e galeria de fotos da empresa.
    /// </summary>
    /// <remarks>
    /// Acesso: ver e baixar sao publicos (a URL serve direto num <c>&lt;img src&gt;</c>);
    /// enviar, reordenar e apagar, so a propria empresa.
    /// </remarks>
    [ApiController]
    [Route("api/empresa/{idEmpresa:int}/imagens")]
    public class EmpresaImagemController : ControllerBase
    {
        private const int LimiteUpload = 6 * 1024 * 1024;

        private readonly EmpresaImagemService _service;

        public EmpresaImagemController(EmpresaImagemService service)
        {
            _service = service;
        }

        /// <summary>GET /api/empresa/5/imagens (so a descricao, sem o conteudo)</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Listar(int idEmpresa)
        {
            var imagens = await _service.ListarAsync(idEmpresa);
            if (imagens is null)
            {
                return EmpresaNaoEncontrada(idEmpresa);
            }

            return Ok(imagens);
        }

        /// <summary>PUT /api/empresa/5/imagens/logo (ou /banner), multipart com o campo <c>arquivo</c></summary>
        [HttpPut("{tipo}")]
        [RequestSizeLimit(LimiteUpload)]
        public async Task<IActionResult> EnviarUnica(int idEmpresa, TipoImagemEmpresa tipo, IFormFile? arquivo)
        {
            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(idEmpresa));

            if (arquivo is null)
            {
                return ArquivoAusente();
            }

            var salva = await _service.SalvarUnicaAsync(idEmpresa, tipo, arquivo.FileName, await LerAsync(arquivo));
            if (salva is null)
            {
                return EmpresaNaoEncontrada(idEmpresa);
            }

            return Ok(salva);
        }

        /// <summary>GET /api/empresa/5/imagens/logo (ou /banner). Devolve a imagem em si.</summary>
        [AllowAnonymous]
        [HttpGet("{tipo}")]
        public async Task<IActionResult> BaixarUnica(int idEmpresa, TipoImagemEmpresa tipo)
        {
            var imagem = await _service.BaixarUnicaAsync(idEmpresa, tipo);
            if (imagem is null)
            {
                return NotFound(new { mensagem = $"A empresa {idEmpresa} nao tem {NomeDo(tipo)}." });
            }

            return Imagem(imagem);
        }

        /// <summary>DELETE /api/empresa/5/imagens/logo (ou /banner)</summary>
        [HttpDelete("{tipo}")]
        public async Task<IActionResult> DeleteUnica(int idEmpresa, TipoImagemEmpresa tipo)
        {
            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(idEmpresa));

            if (!await _service.DeleteUnicaAsync(idEmpresa, tipo))
            {
                return NotFound(new { mensagem = $"A empresa {idEmpresa} nao tem {NomeDo(tipo)}." });
            }

            return NoContent();
        }

        /// <summary>
        /// POST /api/empresa/5/imagens/galeria, multipart com <c>arquivo</c> e, opcional,
        /// <c>ordem</c> (sem ela, a foto vai para o fim).
        /// </summary>
        [HttpPost("galeria")]
        [RequestSizeLimit(LimiteUpload)]
        public async Task<IActionResult> AdicionarGaleria(int idEmpresa, IFormFile? arquivo, [FromForm] int? ordem)
        {
            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(idEmpresa));

            if (arquivo is null)
            {
                return ArquivoAusente();
            }

            var criada = await _service.AdicionarGaleriaAsync(idEmpresa, arquivo.FileName, await LerAsync(arquivo), ordem);
            if (criada is null)
            {
                return EmpresaNaoEncontrada(idEmpresa);
            }

            return CreatedAtAction(nameof(BaixarGaleria), new { idEmpresa, idImagem = criada.Id }, criada);
        }

        /// <summary>GET /api/empresa/5/imagens/galeria/3. Devolve a imagem em si.</summary>
        [AllowAnonymous]
        [HttpGet("galeria/{idImagem:int}")]
        public async Task<IActionResult> BaixarGaleria(int idEmpresa, int idImagem)
        {
            var imagem = await _service.BaixarGaleriaAsync(idEmpresa, idImagem);
            if (imagem is null)
            {
                return FotoNaoEncontrada(idEmpresa, idImagem);
            }

            return Imagem(imagem);
        }

        /// <summary>PATCH /api/empresa/5/imagens/galeria/3 com <c>{ "ordem": 0 }</c></summary>
        [HttpPatch("galeria/{idImagem:int}")]
        public async Task<IActionResult> AlterarOrdem(int idEmpresa, int idImagem, [FromBody] ImagemOrdemDTO dto)
        {
            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(idEmpresa));

            if (!await _service.AlterarOrdemAsync(idEmpresa, idImagem, dto.Ordem!.Value))
            {
                return FotoNaoEncontrada(idEmpresa, idImagem);
            }

            return NoContent();
        }

        /// <summary>DELETE /api/empresa/5/imagens/galeria/3</summary>
        [HttpDelete("galeria/{idImagem:int}")]
        public async Task<IActionResult> DeleteGaleria(int idEmpresa, int idImagem)
        {
            UsuarioLogado.Exigir(User.Usuario().EhEmpresa(idEmpresa));

            if (!await _service.DeleteGaleriaAsync(idEmpresa, idImagem))
            {
                return FotoNaoEncontrada(idEmpresa, idImagem);
            }

            return NoContent();
        }

        /// <summary>
        /// A imagem com o content-type real. Cache publico curto: a URL da listagem muda
        /// (<c>?v=</c>) quando a imagem e trocada.
        /// </summary>
        private FileContentResult Imagem(ImagemDownloadDTO imagem)
        {
            Response.Headers[HeaderNames.ContentDisposition] =
                new ContentDispositionHeaderValue("inline") { FileNameStar = imagem.Nome }.ToString();
            Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
            Response.Headers[HeaderNames.CacheControl] = "public, max-age=300";
            return File(imagem.Conteudo, imagem.ContentType);
        }

        private static async Task<byte[]> LerAsync(IFormFile arquivo)
        {
            using var memoria = new MemoryStream();
            await arquivo.CopyToAsync(memoria);
            return memoria.ToArray();
        }

        private static string NomeDo(TipoImagemEmpresa tipo) => tipo == TipoImagemEmpresa.Logo ? "logo" : "banner";

        private BadRequestObjectResult ArquivoAusente() =>
            BadRequest(new { mensagem = "Envie a imagem no campo \"arquivo\" (multipart/form-data)." });

        private NotFoundObjectResult EmpresaNaoEncontrada(int idEmpresa) =>
            NotFound(new { mensagem = $"Empresa {idEmpresa} nao encontrada." });

        private NotFoundObjectResult FotoNaoEncontrada(int idEmpresa, int idImagem) =>
            NotFound(new { mensagem = $"Foto {idImagem} nao encontrada na galeria da empresa {idEmpresa}." });
    }
}
