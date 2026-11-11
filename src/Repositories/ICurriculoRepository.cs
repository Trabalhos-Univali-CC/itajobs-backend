using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="Curriculo"/> e dos itens dele (experiencias,
    /// habilidades, formacoes, cursos, projetos, idiomas e arquivos). Os itens so existem dentro de um
    /// curriculo, por isso ficam aqui e nao num repositorio proprio.
    /// </summary>
    public interface ICurriculoRepository
    {
        /// <summary>Le o curriculo com candidato e itens (sem os arquivos).</summary>
        Task<Curriculo?> GetByIdAsync(int id);

        /// <summary>
        /// So o dono do curriculo, numa consulta leve (para checar permissao sem
        /// carregar os itens). <c>null</c> = curriculo nao existe.
        /// </summary>
        Task<int?> GetIdCandidatoAsync(int id);

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

        Task<LogProjeto?> GetProjetoAsync(int idCurriculo, int idProjeto);

        Task<LogProjeto> AddProjetoAsync(LogProjeto projeto);

        Task UpdateProjetoAsync(LogProjeto projeto);

        Task DeleteProjetoAsync(LogProjeto projeto);

        Task<LogIdioma?> GetIdiomaAsync(int idCurriculo, int idIdioma);

        /// <summary>
        /// Compara sem diferenciar maiuscula. <paramref name="ignorarId"/> deixa o
        /// proprio idioma de fora (no PUT).
        /// </summary>
        Task<bool> IdiomaExisteAsync(int idCurriculo, string idioma, int? ignorarId = null);

        Task<LogIdioma> AddIdiomaAsync(LogIdioma idioma);

        Task UpdateIdiomaAsync(LogIdioma idioma);

        Task DeleteIdiomaAsync(LogIdioma idioma);

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
