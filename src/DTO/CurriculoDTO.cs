namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API devolve de um curriculo, ja com experiencias e habilidades.
    /// </summary>
    public class CurriculoDTO
    {
        public int Id { get; set; }

        public int IdCandidato { get; set; }

        public string NomeCandidato { get; set; } = string.Empty;

        public List<ExperienciaDTO> Experiencias { get; set; } = [];

        public List<HabilidadeDTO> Habilidades { get; set; } = [];
    }
}
