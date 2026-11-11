using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>logs_projetos</c>: um projeto (pessoal, academico, voluntario...) do
    /// curriculo.
    /// </summary>
    public class LogProjeto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_curriculo")]
        public int IdCurriculo { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("descricao")]
        public string? Descricao { get; set; }
    }
}
