using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="AvaliacaoEmpresa"/>. O candidato vem junto em
    /// toda leitura.
    /// </summary>
    public interface IAvaliacaoRepository
    {
        /// <summary>Das mais novas para as mais antigas.</summary>
        Task<List<AvaliacaoEmpresa>> GetByEmpresaAsync(int idEmpresa);

        Task<AvaliacaoEmpresa?> GetAsync(int idEmpresa, int idCandidato);

        /// <summary>Media das notas da empresa; <c>null</c> sem avaliacao.</summary>
        Task<decimal?> MediaAsync(int idEmpresa);

        Task<AvaliacaoEmpresa> CreateAsync(AvaliacaoEmpresa avaliacao);

        Task UpdateAsync(AvaliacaoEmpresa avaliacao);

        Task DeleteAsync(AvaliacaoEmpresa avaliacao);
    }
}
