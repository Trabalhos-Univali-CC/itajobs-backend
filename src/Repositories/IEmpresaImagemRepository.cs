using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="EmpresaArquivo"/> (logo, banner e galeria). As
    /// listagens nunca trazem o conteudo binario; so o download traz.
    /// </summary>
    public interface IEmpresaImagemRepository
    {
        /// <summary>Todas as imagens da empresa, sem conteudo: logo, banner, galeria na ordem.</summary>
        Task<List<EmpresaArquivo>> ListarSemConteudoAsync(int idEmpresa);

        /// <summary>A logo ou o banner, com conteudo.</summary>
        Task<EmpresaArquivo?> GetUnicaAsync(int idEmpresa, TipoImagemEmpresa tipo);

        /// <summary>Uma foto da galeria, com conteudo.</summary>
        Task<EmpresaArquivo?> GetGaleriaAsync(int idEmpresa, int idImagem);

        /// <summary>Cria ou substitui a logo/banner (so existe uma de cada).</summary>
        Task SalvarUnicaAsync(EmpresaArquivo arquivo);

        Task<int> ContarGaleriaAsync(int idEmpresa);

        /// <summary>Maior <c>ordem</c> da galeria; <c>null</c> se esta vazia.</summary>
        Task<int?> MaiorOrdemGaleriaAsync(int idEmpresa);

        Task<EmpresaArquivo> AdicionarAsync(EmpresaArquivo arquivo);

        /// <summary><c>false</c> se a foto nao existe nesta empresa.</summary>
        Task<bool> AlterarOrdemGaleriaAsync(int idEmpresa, int idImagem, int ordem);

        /// <summary><c>false</c> se nao havia logo/banner.</summary>
        Task<bool> DeleteUnicaAsync(int idEmpresa, TipoImagemEmpresa tipo);

        /// <summary><c>false</c> se a foto nao existe nesta empresa.</summary>
        Task<bool> DeleteGaleriaAsync(int idEmpresa, int idImagem);
    }
}
