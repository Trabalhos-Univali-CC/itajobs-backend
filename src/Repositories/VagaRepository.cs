using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class VagaRepository : IVagaRepository
    {
        private readonly AppDbContext _context;

        public VagaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vaga>> GetAllAsync(int? idEmpresa, string? cargo)
        {
            var query = _context.Vagas.AsNoTracking().Include(v => v.Empresa).AsQueryable();

            if (idEmpresa is not null)
            {
                query = query.Where(v => v.IdEmpresa == idEmpresa);
            }

            if (!string.IsNullOrWhiteSpace(cargo))
            {
                // ILike: busca parcial sem diferenciar maiuscula de minuscula.
                query = query.Where(v => EF.Functions.ILike(v.Cargo, $"%{cargo}%"));
            }

            return await query.OrderByDescending(v => v.Id).ToListAsync();
        }

        public async Task<Vaga?> GetByIdAsync(int id)
        {
            return await _context.Vagas
                .Include(v => v.Empresa)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vaga> CreateAsync(Vaga vaga)
        {
            _context.Vagas.Add(vaga);
            await _context.SaveChangesAsync();

            // A empresa e lida para o Service montar o DTO com o nome dela.
            await _context.Entry(vaga).Reference(v => v.Empresa).LoadAsync();
            return vaga;
        }

        public async Task UpdateAsync(Vaga vaga)
        {
            _context.Vagas.Update(vaga);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Vaga vaga)
        {
            // As candidaturas saem pelo ON DELETE CASCADE do banco.
            _context.Vagas.Remove(vaga);
            await _context.SaveChangesAsync();
        }
    }
}
