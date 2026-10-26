using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Corpo do <c>PATCH /api/empresa/{id}/nota</c>. A faixa permitida e regra de
    /// negocio e fica no <c>EmpresaService</c>.
    /// </summary>
    public class NotaDTO
    {
        [Required(ErrorMessage = "Nota e obrigatoria.")]
        public decimal? Nota { get; set; }
    }
}
