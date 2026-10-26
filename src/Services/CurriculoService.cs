using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de curriculo e dos itens dele.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>O candidato precisa existir e tem no maximo um curriculo (1:1).</item>
    /// <item>Experiencia: inicio e fim nao ficam no futuro, e o fim nao vem antes do inicio.</item>
    /// <item>Habilidade nao se repete no mesmo curriculo (sem diferenciar maiuscula).</item>
    /// <item>Apagar o curriculo apaga experiencias e habilidades.</item>
    /// </list>
    /// </remarks>
    public class CurriculoService
    {
        private readonly ICurriculoRepository _repository;
        private readonly ICandidatoRepository _candidatoRepository;

        public CurriculoService(ICurriculoRepository repository, ICandidatoRepository candidatoRepository)
        {
            _repository = repository;
            _candidatoRepository = candidatoRepository;
        }

        public async Task<CurriculoDTO?> GetByIdAsync(int id)
        {
            var curriculo = await _repository.GetByIdAsync(id);
            return curriculo is null ? null : ToDTO(curriculo);
        }

        public async Task<CurriculoDTO?> GetByCandidatoAsync(int idCandidato)
        {
            var curriculo = await _repository.GetByCandidatoAsync(idCandidato);
            return curriculo is null ? null : ToDTO(curriculo);
        }

        public async Task<CurriculoDTO> CreateAsync(CurriculoCreateDTO dto)
        {
            var idCandidato = dto.IdCandidato!.Value;

            var candidato = await _candidatoRepository.GetByIdAsync(idCandidato);
            if (candidato is null)
            {
                throw new RegraNegocioException(TipoErro.NaoEncontrado,
                    $"Candidato {idCandidato} nao encontrado.");
            }

            if (candidato.Curriculo is not null)
            {
                throw new RegraNegocioException(TipoErro.Conflito,
                    $"O candidato {idCandidato} ja tem um curriculo (id {candidato.Curriculo.Id}).");
            }

            var curriculo = new Curriculo { IdCandidato = idCandidato, Candidato = candidato };

            var criado = await _repository.CreateAsync(curriculo);
            return ToDTO(criado);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var curriculo = await _repository.GetByIdAsync(id);
            if (curriculo is null)
            {
                return false;
            }

            await _repository.DeleteAsync(curriculo);
            return true;
        }

        /// <summary><c>null</c> = curriculo nao encontrado.</summary>
        public async Task<ExperienciaDTO?> AddExperienciaAsync(int idCurriculo, ExperienciaCreateDTO dto)
        {
            ValidarPeriodo(dto.DataInicio!.Value, dto.DataFim);

            if (await _repository.GetByIdAsync(idCurriculo) is null)
            {
                return null;
            }

            var experiencia = new LogExperiencia { IdCurriculo = idCurriculo };
            Copiar(dto, experiencia);

            var criada = await _repository.AddExperienciaAsync(experiencia);
            return ToDTO(criada);
        }

        /// <summary><c>null</c> = experiencia nao encontrada neste curriculo.</summary>
        public async Task<ExperienciaDTO?> UpdateExperienciaAsync(int idCurriculo, int idExperiencia, ExperienciaCreateDTO dto)
        {
            ValidarPeriodo(dto.DataInicio!.Value, dto.DataFim);

            var experiencia = await _repository.GetExperienciaAsync(idCurriculo, idExperiencia);
            if (experiencia is null)
            {
                return null;
            }

            Copiar(dto, experiencia);

            await _repository.UpdateExperienciaAsync(experiencia);
            return ToDTO(experiencia);
        }

        public async Task<bool> DeleteExperienciaAsync(int idCurriculo, int idExperiencia)
        {
            var experiencia = await _repository.GetExperienciaAsync(idCurriculo, idExperiencia);
            if (experiencia is null)
            {
                return false;
            }

            await _repository.DeleteExperienciaAsync(experiencia);
            return true;
        }

        /// <summary><c>null</c> = curriculo nao encontrado.</summary>
        public async Task<HabilidadeDTO?> AddHabilidadeAsync(int idCurriculo, HabilidadeCreateDTO dto)
        {
            var descricao = dto.Descricao.Trim();
            if (descricao.Length == 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "A descricao da habilidade esta vazia.");
            }

            if (await _repository.GetByIdAsync(idCurriculo) is null)
            {
                return null;
            }

            if (await _repository.HabilidadeExisteAsync(idCurriculo, descricao))
            {
                throw new RegraNegocioException(TipoErro.Conflito,
                    $"A habilidade \"{descricao}\" ja esta neste curriculo.");
            }

            var criada = await _repository.AddHabilidadeAsync(
                new LogHabilidade { IdCurriculo = idCurriculo, Descricao = descricao });
            return ToDTO(criada);
        }

        public async Task<bool> DeleteHabilidadeAsync(int idCurriculo, int idHabilidade)
        {
            var habilidade = await _repository.GetHabilidadeAsync(idCurriculo, idHabilidade);
            if (habilidade is null)
            {
                return false;
            }

            await _repository.DeleteHabilidadeAsync(habilidade);
            return true;
        }

        private static void ValidarPeriodo(DateOnly inicio, DateOnly? fim)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            if (inicio > hoje)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de inicio nao pode estar no futuro.");
            }

            if (fim is not null && fim > hoje)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de fim nao pode estar no futuro. Para emprego atual, deixe vazia.");
            }

            if (fim is not null && fim < inicio)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de fim nao pode ser anterior a data de inicio.");
            }
        }

        private static void Copiar(ExperienciaCreateDTO dto, LogExperiencia experiencia)
        {
            experiencia.Cargo = dto.Cargo.Trim();
            experiencia.NomeEmpresa = string.IsNullOrWhiteSpace(dto.NomeEmpresa) ? null : dto.NomeEmpresa.Trim();
            experiencia.DataInicio = dto.DataInicio!.Value;
            experiencia.DataFim = dto.DataFim;
        }

        private static CurriculoDTO ToDTO(Curriculo curriculo) => new()
        {
            Id = curriculo.Id,
            IdCandidato = curriculo.IdCandidato,
            NomeCandidato = $"{curriculo.Candidato.Nome} {curriculo.Candidato.Sobrenome}",
            Experiencias = curriculo.Experiencias.Select(ToDTO).ToList(),
            Habilidades = curriculo.Habilidades.Select(ToDTO).ToList()
        };

        private static ExperienciaDTO ToDTO(LogExperiencia experiencia) => new()
        {
            Id = experiencia.Id,
            Cargo = experiencia.Cargo,
            NomeEmpresa = experiencia.NomeEmpresa,
            DataInicio = experiencia.DataInicio,
            DataFim = experiencia.DataFim
        };

        private static HabilidadeDTO ToDTO(LogHabilidade habilidade) => new()
        {
            Id = habilidade.Id,
            Descricao = habilidade.Descricao
        };
    }
}
