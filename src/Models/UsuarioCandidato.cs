using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>usuario_candidato</c>. Tem no maximo um <see cref="Models.Curriculo"/>
    /// (1:1) e se candidata a varias vagas pela <see cref="Candidatura"/> (*:*).
    /// </summary>
    public class UsuarioCandidato
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("sobrenome")]
        public string Sobrenome { get; set; } = string.Empty;

        /// <summary>Guardado sem mascara: 11 digitos.</summary>
        [Required]
        [MaxLength(11)]
        [Column("cpf")]
        public string Cpf { get; set; } = string.Empty;

        [Column("id_endereco")]
        public int IdEndereco { get; set; }

        public Endereco Endereco { get; set; } = null!;

        [Column("id_contato")]
        public int IdContato { get; set; }

        public Contato Contato { get; set; } = null!;

        public Curriculo? Curriculo { get; set; }

        public List<Candidatura> Candidaturas { get; set; } = [];
    }
}
