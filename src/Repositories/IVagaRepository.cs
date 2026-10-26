using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="Vaga"/>. A empresa vem junto em toda leitura.
    /// </summary>
    public interface IVagaRepository
    {
        /// <summary>Filtros opcionais: <c>null</c> nao filtra.</summary>
        Task<List<Vaga>> GetAllAsync(int? idEmpresa, string? cargo);

        Task<Vaga?> GetByIdAsync(int id);

        Task<Vaga> CreateAsync(Vaga vaga);

        Task UpdateAsync(Vaga vaga);

        Task DeleteAsync(Vaga vaga);
    }
}
