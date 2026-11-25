using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRecuperacaoAsync(RecuperacaoSenha recuperacao)
        {
            _context.RecuperacoesSenha.Add(recuperacao);
            await _context.SaveChangesAsync();
        }

        public async Task<RecuperacaoSenha?> GetRecuperacaoPorHashAsync(string tokenHash)
        {
            return await _context.RecuperacoesSenha.AsNoTracking().FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
        }

        public async Task<bool> ConsumirRecuperacaoAsync(int id)
        {
            var agora = DateTime.UtcNow;
            var alterados = await _context.RecuperacoesSenha
                .Where(r => r.Id == id && r.UtilizadoEm == null && r.ExpiraEm > agora)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.UtilizadoEm, agora));
            return alterados > 0;
        }

        public async Task ApagarRecuperacoesPendentesAsync(int? idCandidato, int? idEmpresa)
        {
            await _context.RecuperacoesSenha
                .Where(r => r.UtilizadoEm == null && r.IdCandidato == idCandidato && r.IdEmpresa == idEmpresa)
                .ExecuteDeleteAsync();
        }

        public async Task<IdentidadeExterna?> GetIdentidadeAsync(string provedor, string identificadorExterno)
        {
            return await _context.IdentidadesExternas.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Provedor == provedor && i.IdentificadorExterno == identificadorExterno);
        }

        public async Task<IdentidadeExterna?> GetIdentidadeDaContaAsync(string provedor, int? idCandidato, int? idEmpresa)
        {
            return await _context.IdentidadesExternas.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Provedor == provedor && i.IdCandidato == idCandidato && i.IdEmpresa == idEmpresa);
        }

        public async Task AddIdentidadeAsync(IdentidadeExterna identidade)
        {
            _context.IdentidadesExternas.Add(identidade);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ApagarIdentidadeDaContaAsync(string provedor, int? idCandidato, int? idEmpresa)
        {
            var apagados = await _context.IdentidadesExternas
                .Where(i => i.Provedor == provedor && i.IdCandidato == idCandidato && i.IdEmpresa == idEmpresa)
                .ExecuteDeleteAsync();
            return apagados > 0;
        }
    }
}
