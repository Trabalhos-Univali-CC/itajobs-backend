using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>curriculo</c>. 1:1 com o candidato (<c>id_candidato</c> e UNIQUE) e
    /// 1:* com experiencias, habilidades, formacoes, cursos, projetos, idiomas e arquivos.
    /// </summary>
    public class Curriculo
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_candidato")]
        public int IdCandidato { get; set; }

        public UsuarioCandidato Candidato { get; set; } = null!;

        /// <summary>Resumo profissional / objetivo, texto livre.</summary>
        [Column("resumo")]
        public string? Resumo { get; set; }

        /// <summary>Link do portfolio (site, GitHub, Behance...). So http/https.</summary>
        [MaxLength(255)]
        [Column("portfolio")]
        public string? Portfolio { get; set; }

        public List<LogExperiencia> Experiencias { get; set; } = [];

        public List<LogHabilidade> Habilidades { get; set; } = [];

        public List<LogFormacao> Formacoes { get; set; } = [];

        public List<LogCurso> Cursos { get; set; } = [];

        public List<LogProjeto> Projetos { get; set; } = [];

        public List<LogIdioma> Idiomas { get; set; } = [];

        /// <summary>Nunca carregado com <c>Include</c>: tem o conteudo binario.</summary>
        public List<CurriculoArquivo> Arquivos { get; set; } = [];
    }
}
