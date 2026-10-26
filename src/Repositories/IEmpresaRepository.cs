using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="UsuarioEmpresa"/>. Endereco e contato vem juntos
    /// em toda leitura.
    /// </summary>
    public interface IEmpresaRepository
    {
        Task<List<UsuarioEmpresa>> GetAllAsync();

        Task<UsuarioEmpresa?> GetByIdAsync(int id);

        /// <summary><paramref name="ignorarId"/> deixa a propria empresa de fora no PUT.</summary>
        Task<bool> CnpjExisteAsync(string cnpj, int? ignorarId = null);

        Task<UsuarioEmpresa> CreateAsync(UsuarioEmpresa empresa);

        Task UpdateAsync(UsuarioEmpresa empresa);

        /// <summary>Remove a empresa com o endereco e o contato dela.</summary>
        Task DeleteAsync(UsuarioEmpresa empresa);
    }
}
