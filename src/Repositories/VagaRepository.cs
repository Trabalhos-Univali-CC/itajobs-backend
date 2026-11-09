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

        public async Task<(List<Vaga> Itens, int Total)> GetPaginaAsync(VagaFiltro filtro)
        {
            var query = _context.Vagas.AsNoTracking().AsQueryable();

            if (filtro.IdEmpresa is not null)
            {
                query = query.Where(v => v.IdEmpresa == filtro.IdEmpresa);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Cargo))
            {
                // ILike: busca parcial sem diferenciar maiuscula de minuscula.
                query = query.Where(v => EF.Functions.ILike(v.Cargo, $"%{filtro.Cargo}%"));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Bairro))
            {
                query = query.Where(v => v.Bairro != null && EF.Functions.ILike(v.Bairro, filtro.Bairro));
            }

            if (filtro.SalarioMinimo is not null)
            {
                query = query.Where(v => v.Salario >= filtro.SalarioMinimo);
            }

            if (filtro.SalarioMaximo is not null)
            {
                query = query.Where(v => v.Salario <= filtro.SalarioMaximo);
            }

            if (filtro.TipoContrato is not null)
            {
                query = query.Where(v => v.TipoContrato == filtro.TipoContrato);
            }

            if (filtro.Jornada is not null)
            {
                query = query.Where(v => v.Jornada == filtro.Jornada);
            }

            if (filtro.Status is not null)
            {
                query = query.Where(v => v.Status == filtro.Status);
            }

            var total = await query.CountAsync();

            var itens = await query
                .Include(v => v.Empresa)
                .OrderByDescending(v => v.DataPublicacao).ThenByDescending(v => v.Id)
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .ToListAsync();

            return (itens, total);
        }

        public async Task<Vaga?> GetByIdAsync(int id)
        {
            return await _context.Vagas
                .Include(v => v.Empresa)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vaga> CreateAsync(Vaga vaga)
        {
            // DataPublicacao vem do DEFAULT do banco e o EF le de volta no insert.
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
