using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST e no PUT de empresa.
    /// </summary>
    /// <remarks>
    /// Sem <c>Nota</c>: a empresa nao define a propria nota. Ela muda pela rota
    /// <c>PATCH /api/empresa/{id}/nota</c>.
    /// </remarks>
    public class EmpresaCreateDTO
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Aceita com ou sem mascara (12.345.678/0001-95).</summary>
        [Required(ErrorMessage = "CNPJ e obrigatorio.")]
        [StringLength(18)]
        public string Cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereco e obrigatorio.")]
        public EnderecoDTO Endereco { get; set; } = new();

        [Required(ErrorMessage = "Contato e obrigatorio.")]
        public ContatoDTO Contato { get; set; } = new();
    }
}
