using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>logs_idiomas</c>: um idioma do curriculo e o nivel nele. Cada idioma
    /// aparece uma vez por curriculo.
    /// </summary>
    public class LogIdioma
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_curriculo")]
        public int IdCurriculo { get; set; }

        [Required]
        [MaxLength(45)]
        [Column("idioma")]
        public string Idioma { get; set; } = string.Empty;

        [Column("nivel")]
        public NivelIdioma Nivel { get; set; }
    }
}
