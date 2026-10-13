using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Contrato do repositorio. A interface existe para o Service depender de uma
    /// abstracao e nao do EF Core direto — troca de banco ou fake em teste nao
    /// obriga a mexer no Service.
    /// </summary>
    public interface IExemploRepository
    {
        Task<List<Exemplo>> GetAllAsync(bool apenasAtivos);

        Task<Exemplo?> GetByIdAsync(int id);

        Task<Exemplo> CreateAsync(Exemplo exemplo);

        Task<Exemplo?> UpdateAsync(Exemplo exemplo);

        Task<bool> DeleteAsync(int id);
    }
}
