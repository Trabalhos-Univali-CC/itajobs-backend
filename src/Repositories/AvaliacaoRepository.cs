using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class AvaliacaoRepository : IAvaliacaoRepository
    {
        private readonly AppDbContext _context;

        public AvaliacaoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AvaliacaoEmpresa>> GetByEmpresaAsync(int idEmpresa)
        {
            return await _context.Avaliacoes.AsNoTracking()
                .Include(a => a.Candidato)
                .Where(a => a.IdEmpresa == idEmpresa)
                .OrderByDescending(a => a.Data)
                .ToListAsync();
        }

        public async Task<AvaliacaoEmpresa?> GetAsync(int idEmpresa, int idCandidato)
        {
            return await _context.Avaliacoes
                .Include(a => a.Candidato)
                .FirstOrDefaultAsync(a => a.IdEmpresa == idEmpresa && a.IdCandidato == idCandidato);
        }

        public async Task<decimal?> MediaAsync(int idEmpresa)
        {
            // Cast para decimal? faz o AVG de uma empresa sem avaliacao virar null.
            return await _context.Avaliacoes
                .Where(a => a.IdEmpresa == idEmpresa)
                .AverageAsync(a => (decimal?)a.Nota);
        }

        public async Task<AvaliacaoEmpresa> CreateAsync(AvaliacaoEmpresa avaliacao)
        {
            // Data vem do DEFAULT do banco e o EF le de volta no insert.
            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();
            return avaliacao;
        }

        public async Task UpdateAsync(AvaliacaoEmpresa avaliacao)
        {
            _context.Avaliacoes.Update(avaliacao);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AvaliacaoEmpresa avaliacao)
        {
            _context.Avaliacoes.Remove(avaliacao);
            await _context.SaveChangesAsync();
        }
    }
}
