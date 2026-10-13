using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    /// <summary>
    /// Acesso a dados de <see cref="Exemplo"/>. <b>Unica camada que toca o
    /// <c>AppDbContext</c></b> — Controller e Service nao conhecem o EF Core.
    /// </summary>
    public class ExemploRepository : IExemploRepository
    {
        private readonly AppDbContext _context;

        public ExemploRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Exemplo>> GetAllAsync(bool apenasAtivos)
        {
            // AsNoTracking: leitura pura, nao precisa do change tracker do EF.
            var query = _context.Exemplos.AsNoTracking();

            if (apenasAtivos)
            {
                query = query.Where(e => e.Ativo);
            }

            return await query.OrderByDescending(e => e.CriadoEm).ToListAsync();
        }

        public async Task<Exemplo?> GetByIdAsync(int id)
        {
            return await _context.Exemplos.FindAsync(id);
        }

        public async Task<Exemplo> CreateAsync(Exemplo exemplo)
        {
            _context.Exemplos.Add(exemplo);
            await _context.SaveChangesAsync();
            return exemplo;
        }

        public async Task<Exemplo?> UpdateAsync(Exemplo exemplo)
        {
            var existente = await _context.Exemplos.FindAsync(exemplo.Id);
            if (existente is null)
            {
                return null;
            }

            existente.Nome = exemplo.Nome;
            existente.Descricao = exemplo.Descricao;
            existente.Quantidade = exemplo.Quantidade;
            existente.Ativo = exemplo.Ativo;

            await _context.SaveChangesAsync();
            return existente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existente = await _context.Exemplos.FindAsync(id);
            if (existente is null)
            {
                return false;
            }

            _context.Exemplos.Remove(existente);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
