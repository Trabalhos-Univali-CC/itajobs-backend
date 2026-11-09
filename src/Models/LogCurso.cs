using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>logs_cursos</c>: curso livre ou certificacao (NR-35, empilhadeira,
    /// informatica...) do curriculo.
    /// </summary>
    public class LogCurso
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_curriculo")]
        public int IdCurriculo { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("instituicao")]
        public string? Instituicao { get; set; }

        [Column("carga_horaria")]
        public int? CargaHoraria { get; set; }

        [Column("data_conclusao")]
        public DateOnly? DataConclusao { get; set; }
    }
}
