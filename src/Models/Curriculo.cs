using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>curriculo</c>. 1:1 com o candidato (<c>id_candidato</c> e UNIQUE) e
    /// 1:* com experiencias e habilidades.
    /// </summary>
    public class Curriculo
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_candidato")]
        public int IdCandidato { get; set; }

        public UsuarioCandidato Candidato { get; set; } = null!;

        public List<LogExperiencia> Experiencias { get; set; } = [];

        public List<LogHabilidade> Habilidades { get; set; } = [];
    }
}
