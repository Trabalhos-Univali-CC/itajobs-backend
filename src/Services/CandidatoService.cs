using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de candidato.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>CPF e guardado sem mascara e precisa ter digitos verificadores validos.</item>
    /// <item>CPF nao se repete entre candidatos.</item>
    /// <item>E-mail nao se repete entre candidatos (uma empresa pode usar o mesmo).</item>
    /// <item>Senha obrigatoria no cadastro; guardada so como hash. Depois so muda pelo <c>PUT /api/auth/senha</c>.</item>
    /// <item>O endereco precisa ser de Itajai/SC (ver <see cref="EnderecoService"/>).</item>
    /// <item>Apagar o candidato apaga endereco, contato, curriculo e candidaturas.</item>
    /// </list>
    /// </remarks>
    public class CandidatoService
    {
        private readonly ICandidatoRepository _repository;
        private readonly EnderecoService _enderecoService;

        public CandidatoService(ICandidatoRepository repository, EnderecoService enderecoService)
        {
            _repository = repository;
            _enderecoService = enderecoService;
        }

        public async Task<List<CandidatoDTO>> GetAllAsync()
        {
            var candidatos = await _repository.GetAllAsync();
            return candidatos.Select(ToDTO).ToList();
        }

        public async Task<CandidatoDTO?> GetByIdAsync(int id)
        {
            var candidato = await _repository.GetByIdAsync(id);
            return candidato is null ? null : ToDTO(candidato);
        }

        public async Task<CandidatoDTO> CreateAsync(CandidatoCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Senha))
            {
                throw new RegraNegocioException(TipoErro.Invalido, "Senha e obrigatoria.");
            }

            var cpf = await ValidarCpfAsync(dto.Cpf, ignorarId: null);
            await ValidarEmailAsync(dto.Contato.Email, ignorarId: null);

            var candidato = new UsuarioCandidato
            {
                Nome = dto.Nome.Trim(),
                Sobrenome = dto.Sobrenome.Trim(),
                Cpf = cpf,
                SenhaHash = SenhaHasher.Gerar(dto.Senha),
                Endereco = new Endereco(),
                Contato = new Contato()
            };
            await _enderecoService.PreencherAsync(dto.Endereco, candidato.Endereco);
            EnderecoContatoMapper.Copiar(dto.Contato, candidato.Contato);

            var criado = await _repository.CreateAsync(candidato);
            return ToDTO(criado);
        }

        public async Task<CandidatoDTO?> UpdateAsync(int id, CandidatoCreateDTO dto)
        {
            var candidato = await _repository.GetByIdAsync(id);
            if (candidato is null)
            {
                return null;
            }

            candidato.Cpf = await ValidarCpfAsync(dto.Cpf, ignorarId: id);
            await ValidarEmailAsync(dto.Contato.Email, ignorarId: id);
            candidato.Nome = dto.Nome.Trim();
            candidato.Sobrenome = dto.Sobrenome.Trim();
            await _enderecoService.PreencherAsync(dto.Endereco, candidato.Endereco);
            EnderecoContatoMapper.Copiar(dto.Contato, candidato.Contato);

            await _repository.UpdateAsync(candidato);
            return ToDTO(candidato);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var candidato = await _repository.GetByIdAsync(id);
            if (candidato is null)
            {
                return false;
            }

            await _repository.DeleteAsync(candidato);
            return true;
        }

        /// <summary>Tira a mascara, confere os digitos e a unicidade. Devolve o CPF limpo.</summary>
        private async Task<string> ValidarCpfAsync(string cpfInformado, int? ignorarId)
        {
            var cpf = Documento.NormalizarCpf(cpfInformado);

            if (!Documento.CpfValido(cpf))
            {
                throw new RegraNegocioException(TipoErro.Invalido, "CPF invalido.");
            }

            if (await _repository.CpfExisteAsync(cpf, ignorarId))
            {
                throw new RegraNegocioException(TipoErro.Conflito, "Ja existe um candidato com este CPF.");
            }

            return cpf;
        }

        private async Task ValidarEmailAsync(string email, int? ignorarId)
        {
            if (await _repository.EmailExisteAsync(EnderecoContatoMapper.NormalizarEmail(email), ignorarId))
            {
                throw new RegraNegocioException(TipoErro.Conflito, "Ja existe um candidato com este e-mail.");
            }
        }

        private static CandidatoDTO ToDTO(UsuarioCandidato candidato) => new()
        {
            Id = candidato.Id,
            Nome = candidato.Nome,
            Sobrenome = candidato.Sobrenome,
            Cpf = candidato.Cpf,
            Endereco = EnderecoContatoMapper.ToDTO(candidato.Endereco),
            Contato = EnderecoContatoMapper.ToDTO(candidato.Contato),
            IdCurriculo = candidato.Curriculo?.Id
        };
    }
}
