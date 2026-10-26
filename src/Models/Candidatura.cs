using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>candidatura</c>: a associativa do *:* entre <see cref="Vaga"/> e
    /// <see cref="UsuarioCandidato"/>. A chave e o par (vaga, candidato).
    /// </summary>
    public class Candidatura
    {
        [Column("id_vaga")]
        public int IdVaga { get; set; }

        public Vaga Vaga { get; set; } = null!;

        [Column("id_candidato")]
        public int IdCandidato { get; set; }

        public UsuarioCandidato Candidato { get; set; } = null!;

        /// <summary>Preenchida pelo banco (DEFAULT CURRENT_TIMESTAMP).</summary>
        [Column("data_candidatura")]
        public DateTime DataCandidatura { get; set; }
    }
}
