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

        /// <summary>
        /// Salario fixo. Remuneracao: nada ("a combinar"), so <c>salario</c>, ou
        /// <c>salarioMinimo</c> + <c>salarioMaximo</c> (faixa). Nunca fixo e faixa juntos.
        /// </summary>
        [Range(0.01, 99_999_999.99, ErrorMessage = "Salario vai de 0,01 a 99.999.999,99.")]
        public decimal? Salario { get; set; }

        [Range(0.01, 99_999_999.99, ErrorMessage = "SalarioMinimo vai de 0,01 a 99.999.999,99.")]
        public decimal? SalarioMinimo { get; set; }

        [Range(0.01, 99_999_999.99, ErrorMessage = "SalarioMaximo vai de 0,01 a 99.999.999,99.")]
        public decimal? SalarioMaximo { get; set; }

        /// <summary><c>presencial</c>, <c>hibrido</c> ou <c>remoto</c>. Opcional.</summary>
        public Modalidade? Modalidade { get; set; }

        /// <summary>Termos para a busca. Repetidas (sem diferenciar maiuscula) entram uma vez so.</summary>
        [MaxLength(20, ErrorMessage = "No maximo 20 palavras-chave.")]
        public List<string>? PalavrasChave { get; set; }

        /// <summary><c>clt</c>, <c>pj</c>, <c>estagio</c>, <c>temporario</c>, <c>aprendiz</c> ou <c>diaria</c>.</summary>
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
