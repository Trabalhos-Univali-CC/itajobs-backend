using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="Curriculo"/> e dos itens dele (experiencias,
    /// habilidades, formacoes, cursos e arquivos). Os itens so existem dentro de um
    /// curriculo, por isso ficam aqui e nao num repositorio proprio.
    /// </summary>
    public interface ICurriculoRepository
    {
        /// <summary>Le o curriculo com candidato e itens (sem os arquivos).</summary>
        Task<Curriculo?> GetByIdAsync(int id);

        /// <summary>Igual ao <see cref="GetByIdAsync"/>, buscando pelo candidato.</summary>
        Task<Curriculo?> GetByCandidatoAsync(int idCandidato);

        Task<Curriculo> CreateAsync(Curriculo curriculo);

        /// <summary>Salva os campos do proprio curriculo (o resumo).</summary>
        Task UpdateAsync(Curriculo curriculo);

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

        Task<LogFormacao?> GetFormacaoAsync(int idCurriculo, int idFormacao);

        Task<LogFormacao> AddFormacaoAsync(LogFormacao formacao);

        Task UpdateFormacaoAsync(LogFormacao formacao);

        Task DeleteFormacaoAsync(LogFormacao formacao);

        Task<LogCurso?> GetCursoAsync(int idCurriculo, int idCurso);

        Task<LogCurso> AddCursoAsync(LogCurso curso);

        Task UpdateCursoAsync(LogCurso curso);

        Task DeleteCursoAsync(LogCurso curso);

        /// <summary>Os arquivos do curriculo <b>sem</b> o conteudo (que pode ter megas).</summary>
        Task<List<CurriculoArquivo>> GetArquivosSemConteudoAsync(int idCurriculo);

        /// <summary>O arquivo completo, com o conteudo.</summary>
        Task<CurriculoArquivo?> GetArquivoAsync(int idCurriculo, TipoArquivo tipo);

        /// <summary>Cria ou substitui o arquivo daquele tipo.</summary>
        Task SalvarArquivoAsync(CurriculoArquivo arquivo);

        /// <summary><c>false</c> se nao havia arquivo daquele tipo.</summary>
        Task<bool> DeleteArquivoAsync(int idCurriculo, TipoArquivo tipo);
    }
}
