using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class CandidatoRepository : ICandidatoRepository
    {
        private readonly AppDbContext _context;

        public CandidatoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UsuarioCandidato>> GetAllAsync()
        {
            return await _context.Candidatos.AsNoTracking()
                .Include(c => c.Endereco)
                .Include(c => c.Contato)
                .Include(c => c.Curriculo)
                .OrderBy(c => c.Nome).ThenBy(c => c.Sobrenome)
                .ToListAsync();
        }

        public async Task<UsuarioCandidato?> GetByIdAsync(int id)
        {
            return await _context.Candidatos
                .Include(c => c.Endereco)
                .Include(c => c.Contato)
                .Include(c => c.Curriculo)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CpfExisteAsync(string cpf, int? ignorarId = null)
        {
            return await _context.Candidatos.AnyAsync(c => c.Cpf == cpf && c.Id != ignorarId);
        }

        public async Task<UsuarioCandidato> CreateAsync(UsuarioCandidato candidato)
        {
            _context.Candidatos.Add(candidato);
            await _context.SaveChangesAsync();
            return candidato;
        }

        public async Task UpdateAsync(UsuarioCandidato candidato)
        {
            _context.Candidatos.Update(candidato);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UsuarioCandidato candidato)
        {
            // Curriculo, experiencias, habilidades e candidaturas saem pelo ON DELETE
            // CASCADE do banco.
            _context.Candidatos.Remove(candidato);
            _context.Enderecos.Remove(candidato.Endereco);
            _context.Contatos.Remove(candidato.Contato);
            await _context.SaveChangesAsync();
        }
    }
}
