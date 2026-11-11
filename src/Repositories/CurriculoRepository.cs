using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class CurriculoRepository : ICurriculoRepository
    {
        private readonly AppDbContext _context;

        public CurriculoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Curriculo?> GetByIdAsync(int id)
        {
            return await ComItens().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<int?> GetIdCandidatoAsync(int id)
        {
            return await _context.Curriculos.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => (int?)c.IdCandidato)
                .FirstOrDefaultAsync();
        }

        public async Task<Curriculo?> GetByCandidatoAsync(int idCandidato)
        {
            return await ComItens().FirstOrDefaultAsync(c => c.IdCandidato == idCandidato);
        }

        public async Task<Curriculo> CreateAsync(Curriculo curriculo)
        {
            _context.Curriculos.Add(curriculo);
            await _context.SaveChangesAsync();
            return curriculo;
        }

        public async Task UpdateAsync(Curriculo curriculo)
        {
            _context.Curriculos.Update(curriculo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Curriculo curriculo)
        {
            // Itens e arquivos saem pelo ON DELETE CASCADE do banco.
            _context.Curriculos.Remove(curriculo);
            await _context.SaveChangesAsync();
        }

        public async Task<LogExperiencia?> GetExperienciaAsync(int idCurriculo, int idExperiencia)
        {
            return await _context.Experiencias
                .FirstOrDefaultAsync(e => e.Id == idExperiencia && e.IdCurriculo == idCurriculo);
        }

        public async Task<LogExperiencia> AddExperienciaAsync(LogExperiencia experiencia)
        {
            _context.Experiencias.Add(experiencia);
            await _context.SaveChangesAsync();
            return experiencia;
        }

        public async Task UpdateExperienciaAsync(LogExperiencia experiencia)
        {
            _context.Experiencias.Update(experiencia);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExperienciaAsync(LogExperiencia experiencia)
        {
            _context.Experiencias.Remove(experiencia);
            await _context.SaveChangesAsync();
        }

        public async Task<LogHabilidade?> GetHabilidadeAsync(int idCurriculo, int idHabilidade)
        {
            return await _context.Habilidades
                .FirstOrDefaultAsync(h => h.Id == idHabilidade && h.IdCurriculo == idCurriculo);
        }

        public async Task<bool> HabilidadeExisteAsync(int idCurriculo, string descricao)
        {
            var normalizada = descricao.ToLower();
            return await _context.Habilidades
                .AnyAsync(h => h.IdCurriculo == idCurriculo && h.Descricao.ToLower() == normalizada);
        }

        public async Task<LogHabilidade> AddHabilidadeAsync(LogHabilidade habilidade)
        {
            _context.Habilidades.Add(habilidade);
            await _context.SaveChangesAsync();
            return habilidade;
        }

        public async Task DeleteHabilidadeAsync(LogHabilidade habilidade)
        {
            _context.Habilidades.Remove(habilidade);
            await _context.SaveChangesAsync();
        }

        public async Task<LogFormacao?> GetFormacaoAsync(int idCurriculo, int idFormacao)
        {
            return await _context.Formacoes
                .FirstOrDefaultAsync(f => f.Id == idFormacao && f.IdCurriculo == idCurriculo);
        }

        public async Task<LogFormacao> AddFormacaoAsync(LogFormacao formacao)
        {
            _context.Formacoes.Add(formacao);
            await _context.SaveChangesAsync();
            return formacao;
        }

        public async Task UpdateFormacaoAsync(LogFormacao formacao)
        {
            _context.Formacoes.Update(formacao);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFormacaoAsync(LogFormacao formacao)
        {
            _context.Formacoes.Remove(formacao);
            await _context.SaveChangesAsync();
        }

        public async Task<LogCurso?> GetCursoAsync(int idCurriculo, int idCurso)
        {
            return await _context.Cursos
                .FirstOrDefaultAsync(c => c.Id == idCurso && c.IdCurriculo == idCurriculo);
        }

        public async Task<LogCurso> AddCursoAsync(LogCurso curso)
        {
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
            return curso;
        }

        public async Task UpdateCursoAsync(LogCurso curso)
        {
            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCursoAsync(LogCurso curso)
        {
            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();
        }

        public async Task<LogProjeto?> GetProjetoAsync(int idCurriculo, int idProjeto)
        {
            return await _context.Projetos
                .FirstOrDefaultAsync(p => p.Id == idProjeto && p.IdCurriculo == idCurriculo);
        }

        public async Task<LogProjeto> AddProjetoAsync(LogProjeto projeto)
        {
            _context.Projetos.Add(projeto);
            await _context.SaveChangesAsync();
            return projeto;
        }

        public async Task UpdateProjetoAsync(LogProjeto projeto)
        {
            _context.Projetos.Update(projeto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjetoAsync(LogProjeto projeto)
        {
            _context.Projetos.Remove(projeto);
            await _context.SaveChangesAsync();
        }

        public async Task<LogIdioma?> GetIdiomaAsync(int idCurriculo, int idIdioma)
        {
            return await _context.Idiomas
                .FirstOrDefaultAsync(i => i.Id == idIdioma && i.IdCurriculo == idCurriculo);
        }

        public async Task<bool> IdiomaExisteAsync(int idCurriculo, string idioma, int? ignorarId = null)
        {
            var normalizado = idioma.ToLower();
            return await _context.Idiomas.AnyAsync(i =>
                i.IdCurriculo == idCurriculo
                && i.Idioma.ToLower() == normalizado
                && (ignorarId == null || i.Id != ignorarId));
        }

        public async Task<LogIdioma> AddIdiomaAsync(LogIdioma idioma)
        {
            _context.Idiomas.Add(idioma);
            await _context.SaveChangesAsync();
            return idioma;
        }

        public async Task UpdateIdiomaAsync(LogIdioma idioma)
        {
            _context.Idiomas.Update(idioma);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteIdiomaAsync(LogIdioma idioma)
        {
            _context.Idiomas.Remove(idioma);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CurriculoArquivo>> GetArquivosSemConteudoAsync(int idCurriculo)
        {
            // Projecao: o SELECT nao traz a coluna "conteudo".
            return await _context.CurriculoArquivos.AsNoTracking()
                .Where(a => a.IdCurriculo == idCurriculo)
                .OrderBy(a => a.Tipo)
                .Select(a => new CurriculoArquivo
                {
                    IdCurriculo = a.IdCurriculo,
                    Tipo = a.Tipo,
                    Nome = a.Nome,
                    ContentType = a.ContentType,
                    Tamanho = a.Tamanho,
                    EnviadoEm = a.EnviadoEm
                })
                .ToListAsync();
        }

        public async Task<CurriculoArquivo?> GetArquivoAsync(int idCurriculo, TipoArquivo tipo)
        {
            return await _context.CurriculoArquivos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdCurriculo == idCurriculo && a.Tipo == tipo);
        }

        public async Task SalvarArquivoAsync(CurriculoArquivo arquivo)
        {
            var existe = await _context.CurriculoArquivos
                .AnyAsync(a => a.IdCurriculo == arquivo.IdCurriculo && a.Tipo == arquivo.Tipo);

            if (existe)
            {
                _context.CurriculoArquivos.Update(arquivo);
            }
            else
            {
                _context.CurriculoArquivos.Add(arquivo);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteArquivoAsync(int idCurriculo, TipoArquivo tipo)
        {
            // Apaga direto no banco, sem ler o conteudo antes.
            var apagados = await _context.CurriculoArquivos
                .Where(a => a.IdCurriculo == idCurriculo && a.Tipo == tipo)
                .ExecuteDeleteAsync();
            return apagados > 0;
        }

        private IQueryable<Curriculo> ComItens()
        {
            return _context.Curriculos
                .Include(c => c.Candidato)
                .Include(c => c.Experiencias.OrderByDescending(e => e.DataInicio))
                .Include(c => c.Habilidades.OrderBy(h => h.Descricao))
                .Include(c => c.Formacoes.OrderByDescending(f => f.DataInicio))
                .Include(c => c.Cursos.OrderByDescending(c => c.DataConclusao))
                .Include(c => c.Projetos.OrderBy(p => p.Id))
                .Include(c => c.Idiomas.OrderBy(i => i.Idioma))
                .AsSplitQuery();
        }
    }
}
