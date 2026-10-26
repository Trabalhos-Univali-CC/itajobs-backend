namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API devolve de uma candidatura: os dois lados do *:* resumidos.
    /// </summary>
    public class CandidaturaDTO
    {
        public int IdVaga { get; set; }

        public string Cargo { get; set; } = string.Empty;

        public int IdEmpresa { get; set; }

        public string NomeEmpresa { get; set; } = string.Empty;

        public int IdCandidato { get; set; }

        public string NomeCandidato { get; set; } = string.Empty;

        public DateTime DataCandidatura { get; set; }
    }
}
