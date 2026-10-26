namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API devolve de uma experiencia.
    /// </summary>
    public class ExperienciaDTO
    {
        public int Id { get; set; }

        public string Cargo { get; set; } = string.Empty;

        public string? NomeEmpresa { get; set; }

        public DateOnly DataInicio { get; set; }

        public DateOnly? DataFim { get; set; }
    }
}
