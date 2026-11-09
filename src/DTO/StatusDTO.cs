using System.ComponentModel.DataAnnotations;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.DTO
{
    /// <summary>Corpo do <c>PATCH /api/vaga/{id}/status</c>.</summary>
    public class VagaStatusDTO
    {
        /// <summary><c>aberta</c> ou <c>encerrada</c>.</summary>
        [Required(ErrorMessage = "Status e obrigatorio.")]
        public StatusVaga? Status { get; set; }
    }

    /// <summary>Corpo do <c>PATCH /api/vaga/{idVaga}/candidaturas/{idCandidato}/status</c>.</summary>
    public class CandidaturaStatusDTO
    {
        /// <summary><c>recebida</c>, <c>emAnalise</c>, <c>aprovada</c> ou <c>recusada</c>.</summary>
        [Required(ErrorMessage = "Status e obrigatorio.")]
        public StatusCandidatura? Status { get; set; }
    }
}
