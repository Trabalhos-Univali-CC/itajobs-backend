using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>vaga</c>. Lado "*" da relacao com <see cref="UsuarioEmpresa"/>.
    /// </summary>
    public class Vaga
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_empresa")]
        public int IdEmpresa { get; set; }

        public UsuarioEmpresa Empresa { get; set; } = null!;

        [Required]
        [MaxLength(45)]
        [Column("cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Required]
        [Column("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Column("salario", TypeName = "decimal(10,2)")]
        public decimal? Salario { get; set; }

        [Column("tipo_contrato")]
        public TipoContrato TipoContrato { get; set; }

        [Column("jornada")]
        public Jornada Jornada { get; set; }

        /// <summary>Onde e o trabalho. Sem informar, vale o bairro da empresa.</summary>
        [MaxLength(100)]
        [Column("bairro")]
        public string? Bairro { get; set; }

        [Column("requisitos")]
        public string? Requisitos { get; set; }

        /// <summary>Vaga encerrada continua visivel, mas nao aceita candidatura.</summary>
        [Column("status")]
        public StatusVaga Status { get; set; } = StatusVaga.Aberta;

        /// <summary>Preenchida pelo banco (DEFAULT CURRENT_TIMESTAMP).</summary>
        [Column("data_publicacao")]
        public DateTime DataPublicacao { get; set; }

        public List<Candidatura> Candidaturas { get; set; } = [];
    }
}
