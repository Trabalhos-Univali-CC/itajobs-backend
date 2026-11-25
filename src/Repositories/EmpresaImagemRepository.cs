using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Repositories
{
    public class EmpresaImagemRepository : IEmpresaImagemRepository
    {
        private readonly AppDbContext _context;

        public EmpresaImagemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmpresaArquivo>> ListarSemConteudoAsync(int idEmpresa)
        {
            // Projecao: o SELECT nao traz a coluna "conteudo".
            return await _context.EmpresaArquivos.AsNoTracking()
                .Where(a => a.IdEmpresa == idEmpresa)
                .OrderBy(a => a.Tipo == TipoImagemEmpresa.Logo ? 0 : a.Tipo == TipoImagemEmpresa.Banner ? 1 : 2)
                .ThenBy(a => a.Ordem).ThenBy(a => a.Id)
                .Select(a => new EmpresaArquivo
                {
                    Id = a.Id,
                    IdEmpresa = a.IdEmpresa,
                    Tipo = a.Tipo,
                    Ordem = a.Ordem,
                    Nome = a.Nome,
                    ContentType = a.ContentType,
                    Tamanho = a.Tamanho,
                    EnviadoEm = a.EnviadoEm
                })
                .ToListAsync();
        }

        public async Task<EmpresaArquivo?> GetUnicaAsync(int idEmpresa, TipoImagemEmpresa tipo)
        {
            return await _context.EmpresaArquivos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdEmpresa == idEmpresa && a.Tipo == tipo);
        }

        public async Task<EmpresaArquivo?> GetGaleriaAsync(int idEmpresa, int idImagem)
        {
            return await _context.EmpresaArquivos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == idImagem && a.IdEmpresa == idEmpresa && a.Tipo == TipoImagemEmpresa.Galeria);
        }

        public async Task SalvarUnicaAsync(EmpresaArquivo arquivo)
        {
            var idExistente = await _context.EmpresaArquivos
                .Where(a => a.IdEmpresa == arquivo.IdEmpresa && a.Tipo == arquivo.Tipo)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();

            if (idExistente is null)
            {
                _context.EmpresaArquivos.Add(arquivo);
            }
            else
            {
                arquivo.Id = idExistente.Value;
                _context.EmpresaArquivos.Update(arquivo);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarGaleriaAsync(int idEmpresa)
        {
            return await _context.EmpresaArquivos
                .CountAsync(a => a.IdEmpresa == idEmpresa && a.Tipo == TipoImagemEmpresa.Galeria);
        }

        public async Task<int?> MaiorOrdemGaleriaAsync(int idEmpresa)
        {
            return await _context.EmpresaArquivos
                .Where(a => a.IdEmpresa == idEmpresa && a.Tipo == TipoImagemEmpresa.Galeria)
                .MaxAsync(a => (int?)a.Ordem);
        }

        public async Task<EmpresaArquivo> AdicionarAsync(EmpresaArquivo arquivo)
        {
            _context.EmpresaArquivos.Add(arquivo);
            await _context.SaveChangesAsync();
            return arquivo;
        }

        public async Task<bool> AlterarOrdemGaleriaAsync(int idEmpresa, int idImagem, int ordem)
        {
            var alterados = await _context.EmpresaArquivos
                .Where(a => a.Id == idImagem && a.IdEmpresa == idEmpresa && a.Tipo == TipoImagemEmpresa.Galeria)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.Ordem, ordem));
            return alterados > 0;
        }

        public async Task<bool> DeleteUnicaAsync(int idEmpresa, TipoImagemEmpresa tipo)
        {
            var apagados = await _context.EmpresaArquivos
                .Where(a => a.IdEmpresa == idEmpresa && a.Tipo == tipo)
                .ExecuteDeleteAsync();
            return apagados > 0;
        }

        public async Task<bool> DeleteGaleriaAsync(int idEmpresa, int idImagem)
        {
            var apagados = await _context.EmpresaArquivos
                .Where(a => a.Id == idImagem && a.IdEmpresa == idEmpresa && a.Tipo == TipoImagemEmpresa.Galeria)
                .ExecuteDeleteAsync();
            return apagados > 0;
        }
    }
}
