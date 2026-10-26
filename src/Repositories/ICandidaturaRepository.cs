using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="Candidatura"/>. Toda leitura traz a vaga (com a
    /// empresa) e o candidato.
    /// </summary>
    public interface ICandidaturaRepository
    {
        Task<List<Candidatura>> GetByVagaAsync(int idVaga);

        Task<List<Candidatura>> GetByCandidatoAsync(int idCandidato);

        Task<Candidatura?> GetAsync(int idVaga, int idCandidato);

        Task<Candidatura> CreateAsync(Candidatura candidatura);

        Task DeleteAsync(Candidatura candidatura);
    }
}
