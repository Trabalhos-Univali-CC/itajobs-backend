using Itajobs_API.src.Models;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API devolve de uma vaga. Leva o nome da empresa para o cliente nao
    /// precisar de uma segunda chamada.
    /// </summary>
    public class VagaDTO
    {
        public int Id { get; set; }

        public int IdEmpresa { get; set; }

        public string NomeEmpresa { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public decimal? Salario { get; set; }

        public TipoContrato TipoContrato { get; set; }

        public Jornada Jornada { get; set; }

        public string? Bairro { get; set; }

        public string? Requisitos { get; set; }

        public StatusVaga Status { get; set; }

        public DateTime DataPublicacao { get; set; }
    }
}
