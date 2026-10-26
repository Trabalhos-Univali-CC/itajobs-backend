using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST e no PUT de experiencia. A coerencia das datas e
    /// regra de negocio e fica no <c>CurriculoService</c>.
    /// </summary>
    public class ExperienciaCreateDTO
    {
        [Required(ErrorMessage = "Cargo e obrigatorio.")]
        [StringLength(45, MinimumLength = 2)]
        public string Cargo { get; set; } = string.Empty;

        [StringLength(45)]
        public string? NomeEmpresa { get; set; }

        [Required(ErrorMessage = "DataInicio e obrigatoria.")]
        public DateOnly? DataInicio { get; set; }

        /// <summary>Vazio = emprego atual.</summary>
        public DateOnly? DataFim { get; set; }
    }
}
