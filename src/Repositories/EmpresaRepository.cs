using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly AppDbContext _context;

        public EmpresaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UsuarioEmpresa>> GetAllAsync()
        {
            return await _context.Empresas.AsNoTracking()
                .Include(e => e.Endereco)
                .Include(e => e.Contato)
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        public async Task<UsuarioEmpresa?> GetByIdAsync(int id)
        {
            return await _context.Empresas
                .Include(e => e.Endereco)
                .Include(e => e.Contato)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> CnpjExisteAsync(string cnpj, int? ignorarId = null)
        {
            return await _context.Empresas.AnyAsync(e => e.Cnpj == cnpj && e.Id != ignorarId);
        }

        public async Task<UsuarioEmpresa> CreateAsync(UsuarioEmpresa empresa)
        {
            // Endereco e contato sao inseridos junto, na mesma transacao.
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();
            return empresa;
        }

        public async Task UpdateAsync(UsuarioEmpresa empresa)
        {
            _context.Empresas.Update(empresa);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UsuarioEmpresa empresa)
        {
            // As vagas (e as candidaturas delas) saem pelo ON DELETE CASCADE do banco.
            _context.Empresas.Remove(empresa);
            _context.Enderecos.Remove(empresa.Endereco);
            _context.Contatos.Remove(empresa.Contato);
            await _context.SaveChangesAsync();
        }
    }
}
