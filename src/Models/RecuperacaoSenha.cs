using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Tabela <c>recuperacao_senha</c>: um pedido de "esqueci minha senha".
    /// </summary>
    /// <remarks>
    /// O token vai so no e-mail; o banco guarda o hash SHA-256 dele, para um vazamento
    /// do banco nao virar acesso as contas. Pertence a um candidato <b>ou</b> a uma
    /// empresa (CHECK no banco). Datas em UTC (<c>timestamp with time zone</c>).
    /// </remarks>
    public class RecuperacaoSenha
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_candidato")]
        public int? IdCandidato { get; set; }

        [Column("id_empresa")]
        public int? IdEmpresa { get; set; }

        /// <summary>SHA-256 do token, em hexadecimal minusculo (64 caracteres).</summary>
        [Required]
        [MaxLength(64)]
        [Column("token_hash")]
        public string TokenHash { get; set; } = string.Empty;

        [Column("criado_em")]
        public DateTime CriadoEm { get; set; }

        [Column("expira_em")]
        public DateTime ExpiraEm { get; set; }

        /// <summary><c>null</c> enquanto o link nao foi usado.</summary>
        [Column("utilizado_em")]
        public DateTime? UtilizadoEm { get; set; }
    }
}
