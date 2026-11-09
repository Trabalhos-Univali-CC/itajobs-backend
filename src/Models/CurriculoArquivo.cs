using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>curriculo_arquivo</c>: o PDF e a foto do curriculo, guardados no
    /// proprio banco (bytea). A chave e (curriculo, tipo): um arquivo de cada tipo.
    /// </summary>
    /// <remarks>
    /// No banco e nao em disco para sair junto no backup e no ON DELETE CASCADE, sem
    /// arquivo orfao. Os limites de tamanho estao no <c>CurriculoService</c>.
    /// </remarks>
    public class CurriculoArquivo
    {
        [Column("id_curriculo")]
        public int IdCurriculo { get; set; }

        [Column("tipo")]
        public TipoArquivo Tipo { get; set; }

        /// <summary>Nome original, so para exibir e para o download.</summary>
        [Required]
        [MaxLength(255)]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("content_type")]
        public string ContentType { get; set; } = string.Empty;

        [Column("tamanho")]
        public int Tamanho { get; set; }

        [Column("conteudo")]
        public byte[] Conteudo { get; set; } = [];

        [Column("enviado_em")]
        public DateTime EnviadoEm { get; set; }
    }
}
