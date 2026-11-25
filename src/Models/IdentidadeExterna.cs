using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>identidade_externa</c>: uma conta de outro provedor (por enquanto so
    /// o Google) ligada a um candidato <b>ou</b> a uma empresa.
    /// </summary>
    /// <remarks>
    /// O vinculo e pelo <c>sub</c> do provedor, que nunca muda; o e-mail do Google
    /// pode mudar. Um mesmo <c>sub</c> so entra uma vez (UNIQUE no banco).
    /// </remarks>
    public class IdentidadeExterna
    {
        public const string Google = "google";

        [Column("id")]
        public int Id { get; set; }

        [Column("id_candidato")]
        public int? IdCandidato { get; set; }

        [Column("id_empresa")]
        public int? IdEmpresa { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("provedor")]
        public string Provedor { get; set; } = Google;

        /// <summary>O <c>sub</c> do token do provedor.</summary>
        [Required]
        [MaxLength(255)]
        [Column("identificador_externo")]
        public string IdentificadorExterno { get; set; } = string.Empty;

        /// <summary>UTC (coluna <c>timestamp with time zone</c>).</summary>
        [Column("criado_em")]
        public DateTime CriadoEm { get; set; }
    }
}
