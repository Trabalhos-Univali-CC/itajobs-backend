using System.ComponentModel.DataAnnotations;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Descricao de uma imagem da empresa, sem o conteudo. O conteudo baixa na
    /// <see cref="Url"/>.
    /// </summary>
    public class EmpresaImagemDTO
    {
        public int Id { get; set; }

        /// <summary><c>logo</c>, <c>banner</c> ou <c>galeria</c>.</summary>
        public TipoImagemEmpresa Tipo { get; set; }

        /// <summary>Posicao na galeria (0 primeiro). Logo e banner: 0.</summary>
        public int Ordem { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        /// <summary>Em bytes.</summary>
        public int Tamanho { get; set; }

        /// <summary>UTC.</summary>
        public DateTime EnviadoEm { get; set; }

        /// <summary>Rota publica para baixar a imagem (serve direto num <c>&lt;img src&gt;</c>).</summary>
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>Corpo do <c>PATCH /api/empresa/{id}/imagens/galeria/{idImagem}</c>.</summary>
    public class ImagemOrdemDTO
    {
        [Required(ErrorMessage = "Ordem e obrigatoria.")]
        [Range(0, 1000, ErrorMessage = "Ordem vai de 0 a 1000.")]
        public int? Ordem { get; set; }
    }

    /// <summary>Uma imagem para download.</summary>
    public record ImagemDownloadDTO(string Nome, string ContentType, byte[] Conteudo, DateTime EnviadoEm);
}
