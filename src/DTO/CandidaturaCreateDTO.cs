using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Corpo do <c>POST /api/vaga/{idVaga}/candidaturas</c>. A vaga vem da rota.
    /// </summary>
    public class CandidaturaCreateDTO
    {
        [Required(ErrorMessage = "IdCandidato e obrigatorio.")]
        public int? IdCandidato { get; set; }
    }
}
