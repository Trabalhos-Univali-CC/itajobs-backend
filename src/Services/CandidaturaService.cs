using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras da candidatura (o *:* entre vaga e candidato).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A vaga e o candidato precisam existir.</item>
    /// <item>O candidato so se candidata depois de criar o curriculo.</item>
    /// <item>O mesmo candidato nao se candidata duas vezes a mesma vaga.</item>
    /// </list>
    /// </remarks>
    public class CandidaturaService
    {
        private readonly ICandidaturaRepository _repository;
        private readonly IVagaRepository _vagaRepository;
        private readonly ICandidatoRepository _candidatoRepository;

        public CandidaturaService(
            ICandidaturaRepository repository,
            IVagaRepository vagaRepository,
            ICandidatoRepository candidatoRepository)
        {
            _repository = repository;
            _vagaRepository = vagaRepository;
            _candidatoRepository = candidatoRepository;
        }

        /// <summary><c>null</c> = vaga nao encontrada.</summary>
        public async Task<List<CandidaturaDTO>?> GetByVagaAsync(int idVaga)
        {
            if (await _vagaRepository.GetByIdAsync(idVaga) is null)
            {
                return null;
            }

            var candidaturas = await _repository.GetByVagaAsync(idVaga);
            return candidaturas.Select(ToDTO).ToList();
        }

        /// <summary><c>null</c> = candidato nao encontrado.</summary>
        public async Task<List<CandidaturaDTO>?> GetByCandidatoAsync(int idCandidato)
        {
            if (await _candidatoRepository.GetByIdAsync(idCandidato) is null)
            {
                return null;
            }

            var candidaturas = await _repository.GetByCandidatoAsync(idCandidato);
            return candidaturas.Select(ToDTO).ToList();
        }

        /// <summary><c>null</c> = vaga nao encontrada.</summary>
        public async Task<CandidaturaDTO?> CreateAsync(int idVaga, CandidaturaCreateDTO dto)
        {
            var idCandidato = dto.IdCandidato!.Value;

            var vaga = await _vagaRepository.GetByIdAsync(idVaga);
            if (vaga is null)
            {
                return null;
            }

            var candidato = await _candidatoRepository.GetByIdAsync(idCandidato);
            if (candidato is null)
            {
                throw new RegraNegocioException(TipoErro.NaoEncontrado,
                    $"Candidato {idCandidato} nao encontrado.");
            }

            if (candidato.Curriculo is null)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "O candidato precisa cadastrar um curriculo antes de se candidatar.");
            }

            if (await _repository.GetAsync(idVaga, idCandidato) is not null)
            {
                throw new RegraNegocioException(TipoErro.Conflito,
                    "O candidato ja se candidatou a esta vaga.");
            }

            var candidatura = new Candidatura
            {
                IdVaga = idVaga,
                Vaga = vaga,
                IdCandidato = idCandidato,
                Candidato = candidato
            };

            var criada = await _repository.CreateAsync(candidatura);
            return ToDTO(criada);
        }

        public async Task<bool> DeleteAsync(int idVaga, int idCandidato)
        {
            var candidatura = await _repository.GetAsync(idVaga, idCandidato);
            if (candidatura is null)
            {
                return false;
            }

            await _repository.DeleteAsync(candidatura);
            return true;
        }

        private static CandidaturaDTO ToDTO(Candidatura candidatura) => new()
        {
            IdVaga = candidatura.IdVaga,
            Cargo = candidatura.Vaga.Cargo,
            IdEmpresa = candidatura.Vaga.IdEmpresa,
            NomeEmpresa = candidatura.Vaga.Empresa.Nome,
            IdCandidato = candidatura.IdCandidato,
            NomeCandidato = $"{candidatura.Candidato.Nome} {candidatura.Candidato.Sobrenome}",
            DataCandidatura = candidatura.DataCandidatura
        };
    }
}
