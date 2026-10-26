using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de vaga.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A vaga pertence a uma empresa que precisa existir.</item>
    /// <item>A vaga nao troca de empresa depois de criada.</item>
    /// <item>Salario, quando informado, e maior que zero.</item>
    /// <item>Apagar a vaga apaga as candidaturas dela.</item>
    /// </list>
    /// </remarks>
    public class VagaService
    {
        private readonly IVagaRepository _repository;
        private readonly IEmpresaRepository _empresaRepository;

        public VagaService(IVagaRepository repository, IEmpresaRepository empresaRepository)
        {
            _repository = repository;
            _empresaRepository = empresaRepository;
        }

        public async Task<List<VagaDTO>> GetAllAsync(int? idEmpresa, string? cargo)
        {
            var vagas = await _repository.GetAllAsync(idEmpresa, cargo?.Trim());
            return vagas.Select(ToDTO).ToList();
        }

        public async Task<VagaDTO?> GetByIdAsync(int id)
        {
            var vaga = await _repository.GetByIdAsync(id);
            return vaga is null ? null : ToDTO(vaga);
        }

        public async Task<VagaDTO> CreateAsync(VagaCreateDTO dto)
        {
            ValidarSalario(dto.Salario);

            var idEmpresa = dto.IdEmpresa!.Value;
            if (await _empresaRepository.GetByIdAsync(idEmpresa) is null)
            {
                throw new RegraNegocioException(TipoErro.NaoEncontrado,
                    $"Empresa {idEmpresa} nao encontrada.");
            }

            var vaga = new Vaga
            {
                IdEmpresa = idEmpresa,
                Cargo = dto.Cargo.Trim(),
                Descricao = dto.Descricao.Trim(),
                Salario = dto.Salario
            };

            var criada = await _repository.CreateAsync(vaga);
            return ToDTO(criada);
        }

        public async Task<VagaDTO?> UpdateAsync(int id, VagaCreateDTO dto)
        {
            ValidarSalario(dto.Salario);

            var vaga = await _repository.GetByIdAsync(id);
            if (vaga is null)
            {
                return null;
            }

            if (dto.IdEmpresa != vaga.IdEmpresa)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "Uma vaga nao pode ser transferida para outra empresa.");
            }

            vaga.Cargo = dto.Cargo.Trim();
            vaga.Descricao = dto.Descricao.Trim();
            vaga.Salario = dto.Salario;

            await _repository.UpdateAsync(vaga);
            return ToDTO(vaga);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vaga = await _repository.GetByIdAsync(id);
            if (vaga is null)
            {
                return false;
            }

            await _repository.DeleteAsync(vaga);
            return true;
        }

        private static void ValidarSalario(decimal? salario)
        {
            if (salario is <= 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "O salario, quando informado, deve ser maior que zero.");
            }
        }

        private static VagaDTO ToDTO(Vaga vaga) => new()
        {
            Id = vaga.Id,
            IdEmpresa = vaga.IdEmpresa,
            NomeEmpresa = vaga.Empresa.Nome,
            Cargo = vaga.Cargo,
            Descricao = vaga.Descricao,
            Salario = vaga.Salario
        };
    }
}
