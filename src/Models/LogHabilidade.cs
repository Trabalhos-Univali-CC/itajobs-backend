using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>logs_habilidades</c>: uma habilidade do curriculo.
    /// </summary>
    public class LogHabilidade
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_curriculo")]
        public int IdCurriculo { get; set; }

        [Required]
        [MaxLength(45)]
        [Column("descricao")]
        public string Descricao { get; set; } = string.Empty;
    }
}
