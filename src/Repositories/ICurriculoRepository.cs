using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="Curriculo"/> e dos itens dele (experiencias e
    /// habilidades). Os itens so existem dentro de um curriculo, por isso ficam aqui
    /// e nao num repositorio proprio.
    /// </summary>
    public interface ICurriculoRepository
    {
        /// <summary>Le o curriculo com candidato, experiencias e habilidades.</summary>
        Task<Curriculo?> GetByIdAsync(int id);

        /// <summary>Igual ao <see cref="GetByIdAsync"/>, buscando pelo candidato.</summary>
        Task<Curriculo?> GetByCandidatoAsync(int idCandidato);

        Task<Curriculo> CreateAsync(Curriculo curriculo);

        Task DeleteAsync(Curriculo curriculo);

        Task<LogExperiencia?> GetExperienciaAsync(int idCurriculo, int idExperiencia);

        Task<LogExperiencia> AddExperienciaAsync(LogExperiencia experiencia);

        Task UpdateExperienciaAsync(LogExperiencia experiencia);

        Task DeleteExperienciaAsync(LogExperiencia experiencia);

        Task<LogHabilidade?> GetHabilidadeAsync(int idCurriculo, int idHabilidade);

        /// <summary>Compara sem diferenciar maiuscula de minuscula.</summary>
        Task<bool> HabilidadeExisteAsync(int idCurriculo, string descricao);

        Task<LogHabilidade> AddHabilidadeAsync(LogHabilidade habilidade);

        Task DeleteHabilidadeAsync(LogHabilidade habilidade);
    }
}
