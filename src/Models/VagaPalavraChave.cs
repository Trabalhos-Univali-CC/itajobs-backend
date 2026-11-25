using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>vaga_palavra_chave</c>: termos para achar a vaga na busca ("solda",
    /// "empilhadeira"). Nao se repetem na mesma vaga, sem diferenciar maiuscula.
    /// </summary>
    public class VagaPalavraChave
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_vaga")]
        public int IdVaga { get; set; }

        /// <summary>Sem espacos nas pontas e nunca vazia (CHECK no banco).</summary>
        [Required]
        [MaxLength(100)]
        [Column("descricao")]
        public string Descricao { get; set; } = string.Empty;
    }
}
