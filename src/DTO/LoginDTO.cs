using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Corpo do <c>POST /api/auth/login</c>.
    /// </summary>
    /// <remarks>
    /// O tipo de conta vai junto porque o e-mail so e unico dentro de cada tipo: a
    /// mesma pessoa pode ter conta de candidato e de empresa com o mesmo e-mail.
    /// </remarks>
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Email invalido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha e obrigatoria.")]
        public string Senha { get; set; } = string.Empty;

        /// <summary><c>candidato</c> ou <c>empresa</c>.</summary>
        [Required(ErrorMessage = "Tipo e obrigatorio.")]
        [RegularExpression("^(candidato|empresa)$", ErrorMessage = "Tipo deve ser \"candidato\" ou \"empresa\".")]
        public string Tipo { get; set; } = string.Empty;
    }
}
