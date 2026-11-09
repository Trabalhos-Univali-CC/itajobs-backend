using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>usuario_empresa</c>. Lado "1" da relacao 1:* com <see cref="Vaga"/>.
    /// </summary>
    public class UsuarioEmpresa
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Guardado sem mascara: 14 caracteres.</summary>
        [Required]
        [MaxLength(14)]
        [Column("cnpj")]
        public string Cnpj { get; set; } = string.Empty;

        [Column("id_endereco")]
        public int IdEndereco { get; set; }

        public Endereco Endereco { get; set; } = null!;

        [Column("id_contato")]
        public int IdContato { get; set; }

        public Contato Contato { get; set; } = null!;

        /// <summary>Hash PBKDF2 da senha (nunca a senha em texto). Ver <c>SenhaHasher</c>.</summary>
        [Required]
        [MaxLength(255)]
        [Column("senha_hash")]
        public string SenhaHash { get; set; } = string.Empty;

        /// <summary>Media das <see cref="Avaliacoes"/>; <c>null</c> sem avaliacao.</summary>
        [Column("nota", TypeName = "decimal(5,2)")]
        public decimal? Nota { get; set; }

        public List<Vaga> Vagas { get; set; } = [];

        public List<AvaliacaoEmpresa> Avaliacoes { get; set; } = [];
    }
}
