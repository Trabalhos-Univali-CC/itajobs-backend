using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>endereco</c>. Como o <see cref="Contato"/>, pertence a uma empresa ou
    /// a um candidato e nao tem rota propria.
    /// </summary>
    public class Endereco
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("cidade")]
        public string Cidade { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("bairro")]
        public string? Bairro { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("rua")]
        public string Rua { get; set; } = string.Empty;

        [Required]
        [MaxLength(6)]
        [Column("numero")]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("complemento")]
        public string? Complemento { get; set; }
    }
}
