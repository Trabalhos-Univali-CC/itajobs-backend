using System.ComponentModel.DataAnnotations;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST e no PUT de vaga. O status muda pela rota propria
    /// (<c>PATCH /api/vaga/{id}/status</c>).
    /// </summary>
    public class VagaCreateDTO
    {
        [Required(ErrorMessage = "IdEmpresa e obrigatorio.")]
        public int? IdEmpresa { get; set; }

        [Required(ErrorMessage = "Cargo e obrigatorio.")]
        [StringLength(45, MinimumLength = 2)]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Descricao e obrigatoria.")]
        public string Descricao { get; set; } = string.Empty;

        public decimal? Salario { get; set; }

        /// <summary><c>clt</c>, <c>pj</c>, <c>estagio</c>, <c>temporario</c> ou <c>aprendiz</c>.</summary>
        [Required(ErrorMessage = "TipoContrato e obrigatorio.")]
        public TipoContrato? TipoContrato { get; set; }

        /// <summary><c>integral</c>, <c>meioPeriodo</c>, <c>noturno</c> ou <c>escala</c>.</summary>
        [Required(ErrorMessage = "Jornada e obrigatoria.")]
        public Jornada? Jornada { get; set; }

        /// <summary>Onde e o trabalho. Vazio = bairro da empresa.</summary>
        [StringLength(100)]
        public string? Bairro { get; set; }

        [StringLength(4000)]
        public string? Requisitos { get; set; }
    }
}
