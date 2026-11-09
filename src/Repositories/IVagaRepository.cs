using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Filtros da busca de vagas. <c>null</c> nao filtra. Fica aqui (e nao no DTO)
    /// porque o Repository nao conhece DTO.
    /// </summary>
    public record VagaFiltro(
        int? IdEmpresa,
        string? Cargo,
        string? Bairro,
        decimal? SalarioMinimo,
        decimal? SalarioMaximo,
        TipoContrato? TipoContrato,
        Jornada? Jornada,
        StatusVaga? Status,
        int Pagina,
        int TamanhoPagina);

    /// <summary>
    /// Acesso a dados de <see cref="Vaga"/>. A empresa vem junto em toda leitura.
    /// </summary>
    public interface IVagaRepository
    {
        /// <summary>Uma pagina da busca, das mais novas para as mais antigas, e o total.</summary>
        Task<(List<Vaga> Itens, int Total)> GetPaginaAsync(VagaFiltro filtro);

        Task<Vaga?> GetByIdAsync(int id);

        Task<Vaga> CreateAsync(Vaga vaga);

        Task UpdateAsync(Vaga vaga);

        Task DeleteAsync(Vaga vaga);
    }
}
