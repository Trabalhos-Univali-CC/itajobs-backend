using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>empresa_arquivo</c>: logo, banner e fotos de galeria da empresa,
    /// guardadas no banco (bytea), como os arquivos do curriculo.
    /// </summary>
    /// <remarks>
    /// Logo e banner sao unicos por empresa (indice unico parcial no banco). A galeria
    /// aceita varias, na ordem de <see cref="Ordem"/>. Limites no <c>EmpresaImagemService</c>.
    /// </remarks>
    public class EmpresaArquivo
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_empresa")]
        public int IdEmpresa { get; set; }

        [Column("tipo")]
        public TipoImagemEmpresa Tipo { get; set; }

        /// <summary>Posicao na galeria (0 primeiro). Logo e banner ficam em 0.</summary>
        [Column("ordem")]
        public int Ordem { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("content_type")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>Em bytes.</summary>
        [Column("tamanho")]
        public int Tamanho { get; set; }

        [Column("conteudo")]
        public byte[] Conteudo { get; set; } = [];

        /// <summary>UTC (coluna <c>timestamp with time zone</c>).</summary>
        [Column("enviado_em")]
        public DateTime EnviadoEm { get; set; }
    }
}
