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

        public async Task DeleteAsync(Curriculo curriculo)
        {
            // Experiencias e habilidades saem pelo ON DELETE CASCADE do banco.
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

        private IQueryable<Curriculo> ComItens()
        {
            return _context.Curriculos
                .Include(c => c.Candidato)
                .Include(c => c.Experiencias.OrderByDescending(e => e.DataInicio))
                .Include(c => c.Habilidades.OrderBy(h => h.Descricao));
        }
    }
}
