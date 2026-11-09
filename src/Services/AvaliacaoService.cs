using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;
using Itajobs_API.src.Utils;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras da avaliacao de empresas.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>So avalia quem ja se candidatou a alguma vaga da empresa.</item>
    /// <item>Uma avaliacao por candidato por empresa; avaliar de novo substitui a anterior.</item>
    /// <item>Nota de 1 a 5. A nota da empresa e a media, recalculada a cada mudanca.</item>
    /// </list>
    /// </remarks>
    public class AvaliacaoService
    {
        private readonly IAvaliacaoRepository _repository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly ICandidaturaRepository _candidaturaRepository;

        public AvaliacaoService(
            IAvaliacaoRepository repository,
            IEmpresaRepository empresaRepository,
            ICandidaturaRepository candidaturaRepository)
        {
            _repository = repository;
            _empresaRepository = empresaRepository;
            _candidaturaRepository = candidaturaRepository;
        }

        /// <summary><c>null</c> = empresa nao encontrada.</summary>
        public async Task<List<AvaliacaoDTO>?> GetByEmpresaAsync(int idEmpresa)
        {
            if (await _empresaRepository.GetByIdAsync(idEmpresa) is null)
            {
                return null;
            }

            var avaliacoes = await _repository.GetByEmpresaAsync(idEmpresa);
            return avaliacoes.Select(ToDTO).ToList();
        }

        /// <summary>Cria ou substitui. <c>null</c> = empresa nao encontrada.</summary>
        public async Task<AvaliacaoDTO?> AvaliarAsync(int idEmpresa, int idCandidato, AvaliacaoCreateDTO dto)
        {
            var empresa = await _empresaRepository.GetByIdAsync(idEmpresa);
            if (empresa is null)
            {
                return null;
            }

            if (!await _candidaturaRepository.CandidatouNaEmpresaAsync(idCandidato, idEmpresa))
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "So pode avaliar a empresa quem ja se candidatou a uma vaga dela.");
            }

            var comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim();

            var avaliacao = await _repository.GetAsync(idEmpresa, idCandidato);
            if (avaliacao is null)
            {
                avaliacao = await _repository.CreateAsync(new AvaliacaoEmpresa
                {
                    IdEmpresa = idEmpresa,
                    IdCandidato = idCandidato,
                    Nota = dto.Nota!.Value,
                    Comentario = comentario
                });
            }
            else
            {
                avaliacao.Nota = dto.Nota!.Value;
                avaliacao.Comentario = comentario;
                avaliacao.Data = Relogio.Agora;
                await _repository.UpdateAsync(avaliacao);
            }

            await RecalcularNotaAsync(empresa);

            // Recarrega com o candidato (o nome sai no DTO).
            return ToDTO((await _repository.GetAsync(idEmpresa, idCandidato))!);
        }

        public async Task<bool> DeleteAsync(int idEmpresa, int idCandidato)
        {
            var avaliacao = await _repository.GetAsync(idEmpresa, idCandidato);
            if (avaliacao is null)
            {
                return false;
            }

            await _repository.DeleteAsync(avaliacao);

            var empresa = await _empresaRepository.GetByIdAsync(idEmpresa);
            if (empresa is not null)
            {
                await RecalcularNotaAsync(empresa);
            }

            return true;
        }

        private async Task RecalcularNotaAsync(UsuarioEmpresa empresa)
        {
            var media = await _repository.MediaAsync(empresa.Id);
            empresa.Nota = media is null ? null : Math.Round(media.Value, 2);
            await _empresaRepository.UpdateAsync(empresa);
        }

        private static AvaliacaoDTO ToDTO(AvaliacaoEmpresa avaliacao) => new()
        {
            IdEmpresa = avaliacao.IdEmpresa,
            IdCandidato = avaliacao.IdCandidato,
            NomeCandidato = avaliacao.Candidato.Nome,
            Nota = avaliacao.Nota,
            Comentario = avaliacao.Comentario,
            Data = avaliacao.Data
        };
    }
}
