using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Endereco dentro de empresa e candidato. Serve para entrada e saida: nao tem
    /// <c>Id</c>, porque o endereco nunca e acessado sozinho.
    /// </summary>
    /// <remarks>
    /// Na entrada, o CEP manda: ele e consultado no ViaCEP, precisa ser de Itajai/SC e
    /// a cidade sempre sai de la (o que vier em <see cref="Cidade"/> e ignorado). Rua e
    /// bairro em branco sao completados pelo ViaCEP.
    /// </remarks>
    public class EnderecoDTO
    {
        /// <summary>Aceita com ou sem mascara (88301-001).</summary>
        [Required(ErrorMessage = "CEP e obrigatorio.")]
        [StringLength(9)]
        public string Cep { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Cidade { get; set; }

        [StringLength(100)]
        public string? Bairro { get; set; }

        [StringLength(100)]
        public string? Rua { get; set; }

        [Required(ErrorMessage = "Numero e obrigatorio.")]
        [StringLength(6)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Complemento { get; set; }
    }
}
