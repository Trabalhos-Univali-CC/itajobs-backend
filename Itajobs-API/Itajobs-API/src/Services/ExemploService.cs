using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regra de negocio. Fica entre o Controller e o Repository, e e onde a conversao
    /// Model &lt;-&gt; DTO acontece.
    /// </summary>
    /// <remarks>
    /// Nao conhece HTTP (nada de <c>IActionResult</c>) nem EF Core (nada de
    /// <c>AppDbContext</c>). Devolver <c>null</c> aqui significa "nao achei" — quem
    /// traduz isso para 404 e o Controller.
    /// </remarks>
    public class ExemploService
    {
        private readonly IExemploRepository _repository;

        public ExemploService(IExemploRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ExemploDTO>> GetAllAsync(bool apenasAtivos)
        {
            var exemplos = await _repository.GetAllAsync(apenasAtivos);
            return exemplos.Select(ToDTO).ToList();
        }

        public async Task<ExemploDTO?> GetByIdAsync(int id)
        {
            var exemplo = await _repository.GetByIdAsync(id);
            return exemplo is null ? null : ToDTO(exemplo);
        }

        public async Task<ExemploDTO> CreateAsync(ExemploCreateDTO dto)
        {
            var exemplo = new Exemplo
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Quantidade = dto.Quantidade,
                Ativo = dto.Ativo,
                CriadoEm = DateTime.UtcNow
            };

            var criado = await _repository.CreateAsync(exemplo);
            return ToDTO(criado);
        }

        public async Task<ExemploDTO?> UpdateAsync(int id, ExemploCreateDTO dto)
        {
            var exemplo = new Exemplo
            {
                Id = id,
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Quantidade = dto.Quantidade,
                Ativo = dto.Ativo
            };

            var atualizado = await _repository.UpdateAsync(exemplo);
            return atualizado is null ? null : ToDTO(atualizado);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _repository.DeleteAsync(id);
        }

        /// <summary>
        /// Model -> DTO. Com muitas entidades, isto vira perfil do AutoMapper numa
        /// pasta <c>Mappings/</c>; com poucas, a mao e mais simples de ler.
        /// </summary>
        private static ExemploDTO ToDTO(Exemplo exemplo) => new()
        {
            Id = exemplo.Id,
            Nome = exemplo.Nome,
            Descricao = exemplo.Descricao,
            Quantidade = exemplo.Quantidade,
            Ativo = exemplo.Ativo,
            CriadoEm = exemplo.CriadoEm
        };
    }
}
