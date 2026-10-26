using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>contato</c>. Pertence a uma empresa ou a um candidato — nao tem
    /// rota propria, entra e sai junto com o dono.
    /// </summary>
    public class Contato
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        [Column("telefone")]
        public string Telefone { get; set; } = string.Empty;
    }
}
