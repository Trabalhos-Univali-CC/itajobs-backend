using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST e no PUT de vaga.
    /// </summary>
    public class VagaCreateDTO
    {
        [Required(ErrorMessage = "IdEmpresa e obrigatorio.")]
        public int? IdEmpresa { get; set; }

        [Required(ErrorMessage = "Cargo e obrigatorio.")]
        [StringLength(45, MinimumLength = 2)]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Descricao e obrigatoria.")]
        public string Descricao { get; set; } = string.Empty;

        public decimal? Salario { get; set; }
    }
}
