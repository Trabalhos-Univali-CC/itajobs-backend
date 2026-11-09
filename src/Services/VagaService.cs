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
    /// <item>Sem bairro informado, a vaga fica no bairro da empresa.</item>
    /// <item>Nasce aberta; so a rota de status encerra ou reabre. Encerrada nao aceita candidatura.</item>
    /// <item>A busca, sem status informado, mostra so abertas, menos quando filtra por empresa.</item>
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

        public async Task<PaginaDTO<VagaDTO>> BuscarAsync(VagaFiltroDTO filtro)
        {
            if (filtro.SalarioMinimo > filtro.SalarioMaximo)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "SalarioMinimo nao pode ser maior que SalarioMaximo.");
            }

            var status = filtro.Status ?? (filtro.IdEmpresa is null ? StatusVaga.Aberta : null);

            var (itens, total) = await _repository.GetPaginaAsync(new VagaFiltro(
                filtro.IdEmpresa,
                filtro.Cargo?.Trim(),
                filtro.Bairro?.Trim(),
                filtro.SalarioMinimo,
                filtro.SalarioMaximo,
                filtro.TipoContrato,
                filtro.Jornada,
                status,
                filtro.Pagina,
                filtro.TamanhoPagina));

            return new PaginaDTO<VagaDTO>
            {
                Itens = itens.Select(ToDTO).ToList(),
                Pagina = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina,
                Total = total
            };
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
            var empresa = await _empresaRepository.GetByIdAsync(idEmpresa)
                ?? throw new RegraNegocioException(TipoErro.NaoEncontrado, $"Empresa {idEmpresa} nao encontrada.");

            var vaga = new Vaga { IdEmpresa = idEmpresa, Status = StatusVaga.Aberta };
            Copiar(dto, vaga, bairroPadrao: empresa.Endereco.Bairro);

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

            var empresa = await _empresaRepository.GetByIdAsync(vaga.IdEmpresa);
            Copiar(dto, vaga, bairroPadrao: empresa?.Endereco.Bairro);

            await _repository.UpdateAsync(vaga);
            return ToDTO(vaga);
        }

        /// <summary>Encerra ou reabre. <c>null</c> = vaga nao encontrada.</summary>
        public async Task<VagaDTO?> AlterarStatusAsync(int id, StatusVaga status)
        {
            var vaga = await _repository.GetByIdAsync(id);
            if (vaga is null)
            {
                return null;
            }

            vaga.Status = status;
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

        private static void Copiar(VagaCreateDTO dto, Vaga vaga, string? bairroPadrao)
        {
            vaga.Cargo = dto.Cargo.Trim();
            vaga.Descricao = dto.Descricao.Trim();
            vaga.Salario = dto.Salario;
            vaga.TipoContrato = dto.TipoContrato!.Value;
            vaga.Jornada = dto.Jornada!.Value;
            vaga.Bairro = VazioParaNull(dto.Bairro) ?? bairroPadrao;
            vaga.Requisitos = VazioParaNull(dto.Requisitos);
        }

        private static string? VazioParaNull(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static VagaDTO ToDTO(Vaga vaga) => new()
        {
            Id = vaga.Id,
            IdEmpresa = vaga.IdEmpresa,
            NomeEmpresa = vaga.Empresa.Nome,
            Cargo = vaga.Cargo,
            Descricao = vaga.Descricao,
            Salario = vaga.Salario,
            TipoContrato = vaga.TipoContrato,
            Jornada = vaga.Jornada,
            Bairro = vaga.Bairro,
            Requisitos = vaga.Requisitos,
            Status = vaga.Status,
            DataPublicacao = vaga.DataPublicacao
        };
    }
}
