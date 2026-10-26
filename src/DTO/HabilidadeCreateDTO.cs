using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST de habilidade.
    /// </summary>
    public class HabilidadeCreateDTO
    {
        [Required(ErrorMessage = "Descricao e obrigatoria.")]
        [StringLength(45, MinimumLength = 1)]
        public string Descricao { get; set; } = string.Empty;
    }
}
