using System.ComponentModel.DataAnnotations;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Query string do <c>GET /api/vaga</c>. Todo filtro e opcional.
    /// </summary>
    /// <example>/api/vaga?cargo=solda&amp;bairro=Centro&amp;tipoContrato=clt&amp;pagina=2</example>
    public class VagaFiltroDTO
    {
        public int? IdEmpresa { get; set; }

        /// <summary>Busca parcial, sem diferenciar maiuscula.</summary>
        [StringLength(45)]
        public string? Cargo { get; set; }

        /// <summary>Bairro exato, sem diferenciar maiuscula.</summary>
        [StringLength(100)]
        public string? Bairro { get; set; }

        /// <summary>
        /// Paga pelo menos isso: salario fixo >= valor, ou o maximo da faixa >= valor.
        /// Vagas "a combinar" ficam de fora quando um dos filtros de salario vem.
        /// </summary>
        [Range(0, 1_000_000)]
        public decimal? SalarioMinimo { get; set; }

        /// <summary>Paga no maximo isso: salario fixo &lt;= valor, ou o minimo da faixa &lt;= valor.</summary>
        [Range(0, 1_000_000)]
        public decimal? SalarioMaximo { get; set; }

        [EnumDataType(typeof(TipoContrato), ErrorMessage = "TipoContrato invalido.")]
        public TipoContrato? TipoContrato { get; set; }

        [EnumDataType(typeof(Jornada), ErrorMessage = "Jornada invalida.")]
        public Jornada? Jornada { get; set; }

        [EnumDataType(typeof(Modalidade), ErrorMessage = "Modalidade invalida.")]
        public Modalidade? Modalidade { get; set; }

        /// <summary>Palavra-chave exata da vaga, sem diferenciar maiuscula.</summary>
        [StringLength(100)]
        public string? PalavraChave { get; set; }

        /// <summary>
        /// Sem informar: so abertas na busca geral; todas quando filtra por empresa
        /// (o painel da empresa mostra as encerradas tambem).
        /// </summary>
        [EnumDataType(typeof(StatusVaga), ErrorMessage = "Status invalido.")]
        public StatusVaga? Status { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Pagina comeca em 1.")]
        public int Pagina { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "TamanhoPagina vai de 1 a 100.")]
        public int TamanhoPagina { get; set; } = 20;
    }
}
