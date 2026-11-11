using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>logs_experiencias</c>: uma experiencia profissional do curriculo.
    /// </summary>
    public class LogExperiencia
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_curriculo")]
        public int IdCurriculo { get; set; }

        [Required]
        [MaxLength(45)]
        [Column("cargo")]
        public string Cargo { get; set; } = string.Empty;

        [MaxLength(45)]
        [Column("nome_empresa")]
        public string? NomeEmpresa { get; set; }

        [Column("data_inicio")]
        public DateOnly DataInicio { get; set; }

        /// <summary><c>null</c> = emprego atual.</summary>
        [Column("data_fim")]
        public DateOnly? DataFim { get; set; }

        /// <summary>O que fazia no cargo.</summary>
        [MaxLength(1000)]
        [Column("descricao")]
        public string? Descricao { get; set; }
    }
}
