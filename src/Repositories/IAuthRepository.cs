using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados da autenticacao alem da senha: pedidos de recuperacao de senha
    /// (<see cref="RecuperacaoSenha"/>) e contas externas (<see cref="IdentidadeExterna"/>).
    /// </summary>
    /// <remarks>
    /// Os dois pertencem a um candidato OU a uma empresa; os metodos recebem os dois
    /// ids e so um vem preenchido.
    /// </remarks>
    public interface IAuthRepository
    {
        Task AddRecuperacaoAsync(RecuperacaoSenha recuperacao);

        Task<RecuperacaoSenha?> GetRecuperacaoPorHashAsync(string tokenHash);

        /// <summary>
        /// Marca como usado, se ainda nao foi usado e nao expirou. <c>false</c> = nao
        /// valia mais (ou outra request usou primeiro).
        /// </summary>
        Task<bool> ConsumirRecuperacaoAsync(int id);

        /// <summary>Apaga os pedidos ainda nao usados da conta (o link antigo para de valer).</summary>
        Task ApagarRecuperacoesPendentesAsync(int? idCandidato, int? idEmpresa);

        Task<IdentidadeExterna?> GetIdentidadeAsync(string provedor, string identificadorExterno);

        Task<IdentidadeExterna?> GetIdentidadeDaContaAsync(string provedor, int? idCandidato, int? idEmpresa);

        Task AddIdentidadeAsync(IdentidadeExterna identidade);

        /// <summary><c>false</c> se a conta nao tinha vinculo com o provedor.</summary>
        Task<bool> ApagarIdentidadeDaContaAsync(string provedor, int? idCandidato, int? idEmpresa);
    }
}
