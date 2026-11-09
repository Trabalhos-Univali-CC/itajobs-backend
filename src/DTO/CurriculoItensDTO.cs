using System.ComponentModel.DataAnnotations;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.DTO
{
    /// <summary>Corpo do <c>PUT /api/curriculo/{id}</c>.</summary>
    public class CurriculoUpdateDTO
    {
        /// <summary>Resumo profissional. Vazio apaga.</summary>
        [StringLength(2000)]
        public string? Resumo { get; set; }
    }

    /// <summary>
    /// O que a API recebe no POST e no PUT de formacao. A coerencia das datas fica no
    /// <c>CurriculoService</c>.
    /// </summary>
    public class FormacaoCreateDTO
    {
        [Required(ErrorMessage = "Instituicao e obrigatoria.")]
        [StringLength(100, MinimumLength = 2)]
        public string Instituicao { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Curso { get; set; }

        /// <summary><c>fundamental</c>, <c>medio</c>, <c>tecnico</c>, <c>superior</c> ou <c>posGraduacao</c>.</summary>
        [Required(ErrorMessage = "Nivel e obrigatorio.")]
        public NivelFormacao? Nivel { get; set; }

        [Required(ErrorMessage = "DataInicio e obrigatoria.")]
        public DateOnly? DataInicio { get; set; }

        /// <summary>Vazio = em andamento. Pode ser futura (previsao de conclusao).</summary>
        public DateOnly? DataFim { get; set; }
    }

    public class FormacaoDTO
    {
        public int Id { get; set; }

        public string Instituicao { get; set; } = string.Empty;

        public string? Curso { get; set; }

        public NivelFormacao Nivel { get; set; }

        public DateOnly DataInicio { get; set; }

        public DateOnly? DataFim { get; set; }
    }

    /// <summary>O que a API recebe no POST e no PUT de curso.</summary>
    public class CursoCreateDTO
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Instituicao { get; set; }

        [Range(1, 10_000, ErrorMessage = "CargaHoraria vai de 1 a 10000 horas.")]
        public int? CargaHoraria { get; set; }

        public DateOnly? DataConclusao { get; set; }
    }

    public class CursoDTO
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Instituicao { get; set; }

        public int? CargaHoraria { get; set; }

        public DateOnly? DataConclusao { get; set; }
    }

    /// <summary>O arquivo para download: o Controller devolve como <c>File(...)</c>.</summary>
    public record ArquivoDownloadDTO(string Nome, string ContentType, byte[] Conteudo);

    /// <summary>Descricao de um arquivo do curriculo, sem o conteudo.</summary>
    public class ArquivoDTO
    {
        /// <summary><c>pdf</c> ou <c>foto</c>.</summary>
        public TipoArquivo Tipo { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        /// <summary>Em bytes.</summary>
        public int Tamanho { get; set; }

        public DateTime EnviadoEm { get; set; }
    }
}
