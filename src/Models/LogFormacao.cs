using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>logs_formacoes</c>: uma formacao (escola, tecnico, faculdade...) do
    /// curriculo.
    /// </summary>
    public class LogFormacao
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_curriculo")]
        public int IdCurriculo { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("instituicao")]
        public string Instituicao { get; set; } = string.Empty;

        /// <summary>Nome do curso. Vazio para ensino fundamental/medio.</summary>
        [MaxLength(100)]
        [Column("curso")]
        public string? Curso { get; set; }

        [Column("nivel")]
        public NivelFormacao Nivel { get; set; }

        [Column("data_inicio")]
        public DateOnly DataInicio { get; set; }

        /// <summary><c>null</c> = em andamento. Pode ser futura (previsao de conclusao).</summary>
        [Column("data_fim")]
        public DateOnly? DataFim { get; set; }
    }
}
