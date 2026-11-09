using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Corpo do <c>PUT /api/empresa/{idEmpresa}/avaliacoes/{idCandidato}</c>: cria
    /// ou troca a avaliacao do candidato.
    /// </summary>
    public class AvaliacaoCreateDTO
    {
        [Required(ErrorMessage = "Nota e obrigatoria.")]
        [Range(1, 5, ErrorMessage = "A nota vai de 1 a 5.")]
        public int? Nota { get; set; }

        [StringLength(500)]
        public string? Comentario { get; set; }
    }

    /// <summary>
    /// O que a API devolve de uma avaliacao. So o primeiro nome do candidato sai.
    /// </summary>
    public class AvaliacaoDTO
    {
        public int IdEmpresa { get; set; }

        public int IdCandidato { get; set; }

        public string NomeCandidato { get; set; } = string.Empty;

        public int Nota { get; set; }

        public string? Comentario { get; set; }

        public DateTime Data { get; set; }
    }
}
