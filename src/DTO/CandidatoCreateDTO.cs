using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST e no PUT de candidato.
    /// </summary>
    public class CandidatoCreateDTO
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sobrenome e obrigatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string Sobrenome { get; set; } = string.Empty;

        /// <summary>Aceita com ou sem mascara (123.456.789-09).</summary>
        [Required(ErrorMessage = "CPF e obrigatorio.")]
        [StringLength(14)]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereco e obrigatorio.")]
        public EnderecoDTO Endereco { get; set; } = new();

        [Required(ErrorMessage = "Contato e obrigatorio.")]
        public ContatoDTO Contato { get; set; } = new();
    }
}
