using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API <b>recebe</b> no POST e no PUT.
    /// </summary>
    /// <remarks>
    /// Nao tem <c>Id</c> nem <c>CriadoEm</c> de proposito: quem decide esses valores e
    /// o servidor. Se o Model fosse aceito direto no corpo, o cliente poderia mandar
    /// qualquer um dos dois.
    /// </remarks>
    public class ExemploCreateDTO
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        [StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantidade nao pode ser negativa.")]
        public int Quantidade { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
