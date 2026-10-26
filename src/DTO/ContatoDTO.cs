using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Contato dentro de empresa e candidato. Entrada e saida, como o
    /// <see cref="EnderecoDTO"/>.
    /// </summary>
    public class ContatoDTO
    {
        [Required(ErrorMessage = "Email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Email invalido.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone e obrigatorio.")]
        [StringLength(15)]
        public string Telefone { get; set; } = string.Empty;
    }
}
