using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST de curriculo. Experiencias e habilidades entram
    /// depois, pelas rotas proprias.
    /// </summary>
    public class CurriculoCreateDTO
    {
        [Required(ErrorMessage = "IdCandidato e obrigatorio.")]
        public int? IdCandidato { get; set; }
    }
}
