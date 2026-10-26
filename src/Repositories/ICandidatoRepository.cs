using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="UsuarioCandidato"/>. Endereco, contato e o
    /// curriculo (sem os itens) vem juntos em toda leitura.
    /// </summary>
    public interface ICandidatoRepository
    {
        Task<List<UsuarioCandidato>> GetAllAsync();

        Task<UsuarioCandidato?> GetByIdAsync(int id);

        /// <summary><paramref name="ignorarId"/> deixa o proprio candidato de fora no PUT.</summary>
        Task<bool> CpfExisteAsync(string cpf, int? ignorarId = null);

        Task<UsuarioCandidato> CreateAsync(UsuarioCandidato candidato);

        Task UpdateAsync(UsuarioCandidato candidato);

        /// <summary>Remove o candidato com o endereco e o contato dele.</summary>
        Task DeleteAsync(UsuarioCandidato candidato);
    }
}
