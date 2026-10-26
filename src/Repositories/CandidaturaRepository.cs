using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class CandidaturaRepository : ICandidaturaRepository
    {
        private readonly AppDbContext _context;

        public CandidaturaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Candidatura>> GetByVagaAsync(int idVaga)
        {
            return await Completas().AsNoTracking()
                .Where(c => c.IdVaga == idVaga)
                .OrderBy(c => c.DataCandidatura)
                .ToListAsync();
        }

        public async Task<List<Candidatura>> GetByCandidatoAsync(int idCandidato)
        {
            return await Completas().AsNoTracking()
                .Where(c => c.IdCandidato == idCandidato)
                .OrderByDescending(c => c.DataCandidatura)
                .ToListAsync();
        }

        public async Task<Candidatura?> GetAsync(int idVaga, int idCandidato)
        {
            return await Completas()
                .FirstOrDefaultAsync(c => c.IdVaga == idVaga && c.IdCandidato == idCandidato);
        }

        public async Task<Candidatura> CreateAsync(Candidatura candidatura)
        {
            // DataCandidatura vem do DEFAULT do banco e o EF le de volta no insert.
            _context.Candidaturas.Add(candidatura);
            await _context.SaveChangesAsync();
            return candidatura;
        }

        public async Task DeleteAsync(Candidatura candidatura)
        {
            _context.Candidaturas.Remove(candidatura);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Candidatura> Completas()
        {
            return _context.Candidaturas
                .Include(c => c.Vaga).ThenInclude(v => v.Empresa)
                .Include(c => c.Candidato);
        }
    }
}
