using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Endereco dentro de empresa e candidato. Serve para entrada e saida: nao tem
    /// <c>Id</c>, porque o endereco nunca e acessado sozinho.
    /// </summary>
    public class EnderecoDTO
    {
        [Required(ErrorMessage = "Cidade e obrigatoria.")]
        [StringLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Bairro { get; set; }

        [Required(ErrorMessage = "Rua e obrigatoria.")]
        [StringLength(100)]
        public string Rua { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numero e obrigatorio.")]
        [StringLength(6)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Complemento { get; set; }
    }
}
