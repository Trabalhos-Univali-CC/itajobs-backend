using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="Candidatura"/>. Toda leitura traz a vaga (com a
    /// empresa) e o candidato.
    /// </summary>
    public interface ICandidaturaRepository
    {
        /// <summary><paramref name="status"/> <c>null</c> traz todas.</summary>
        Task<List<Candidatura>> GetByVagaAsync(int idVaga, StatusCandidatura? status = null);

        Task<List<Candidatura>> GetByCandidatoAsync(int idCandidato);

        Task<Candidatura?> GetAsync(int idVaga, int idCandidato);

        /// <summary>Se o candidato ja se candidatou a alguma vaga da empresa.</summary>
        Task<bool> CandidatouNaEmpresaAsync(int idCandidato, int idEmpresa);

        Task<Candidatura> CreateAsync(Candidatura candidatura);

        Task UpdateAsync(Candidatura candidatura);

        Task DeleteAsync(Candidatura candidatura);
    }
}
