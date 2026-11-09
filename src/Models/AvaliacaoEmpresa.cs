using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>avaliacao_empresa</c>: a nota que um candidato deu a uma empresa. A
    /// chave e o par (empresa, candidato): cada candidato avalia uma empresa uma vez
    /// e pode mudar a nota depois.
    /// </summary>
    /// <remarks>
    /// <c>usuario_empresa.nota</c> guarda a media, recalculada a cada avaliacao.
    /// </remarks>
    public class AvaliacaoEmpresa
    {
        [Column("id_empresa")]
        public int IdEmpresa { get; set; }

        public UsuarioEmpresa Empresa { get; set; } = null!;

        [Column("id_candidato")]
        public int IdCandidato { get; set; }

        public UsuarioCandidato Candidato { get; set; } = null!;

        /// <summary>De 1 a 5 estrelas.</summary>
        [Column("nota")]
        public int Nota { get; set; }

        [MaxLength(500)]
        [Column("comentario")]
        public string? Comentario { get; set; }

        [Column("data")]
        public DateTime Data { get; set; }
    }
}
