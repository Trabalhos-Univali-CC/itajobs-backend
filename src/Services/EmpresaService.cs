using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de empresa.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>CNPJ e guardado sem mascara e precisa ter digitos verificadores validos.</item>
    /// <item>CNPJ nao se repete entre empresas.</item>
    /// <item>E-mail nao se repete entre empresas (um candidato pode usar o mesmo).</item>
    /// <item>Senha obrigatoria no cadastro; guardada so como hash. Depois so muda pelo <c>PUT /api/auth/senha</c>.</item>
    /// <item>O endereco precisa ser de Itajai/SC (ver <see cref="EnderecoService"/>).</item>
    /// <item>A nota e a media das avaliacoes dos candidatos (ver <see cref="AvaliacaoService"/>); nao se edita aqui.</item>
    /// <item>Apagar a empresa apaga endereco, contato, vagas, as candidaturas dessas vagas e as avaliacoes.</item>
    /// </list>
    /// </remarks>
    public class EmpresaService
    {
        private readonly IEmpresaRepository _repository;
        private readonly EnderecoService _enderecoService;

        public EmpresaService(IEmpresaRepository repository, EnderecoService enderecoService)
        {
            _repository = repository;
            _enderecoService = enderecoService;
        }

        public async Task<List<EmpresaDTO>> GetAllAsync()
        {
            var empresas = await _repository.GetAllAsync();
            return empresas.Select(ToDTO).ToList();
        }

        public async Task<EmpresaDTO?> GetByIdAsync(int id)
        {
            var empresa = await _repository.GetByIdAsync(id);
            return empresa is null ? null : ToDTO(empresa);
        }

        public async Task<EmpresaDTO> CreateAsync(EmpresaCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Senha))
            {
                throw new RegraNegocioException(TipoErro.Invalido, "Senha e obrigatoria.");
            }

            var cnpj = await ValidarCnpjAsync(dto.Cnpj, ignorarId: null);
            await ValidarEmailAsync(dto.Contato.Email, ignorarId: null);

            var empresa = new UsuarioEmpresa
            {
                Nome = dto.Nome.Trim(),
                Cnpj = cnpj,
                SenhaHash = SenhaHasher.Gerar(dto.Senha),
                Endereco = new Endereco(),
                Contato = new Contato()
            };
            await _enderecoService.PreencherAsync(dto.Endereco, empresa.Endereco);
            EnderecoContatoMapper.Copiar(dto.Contato, empresa.Contato);

            var criada = await _repository.CreateAsync(empresa);
            return ToDTO(criada);
        }

        public async Task<EmpresaDTO?> UpdateAsync(int id, EmpresaCreateDTO dto)
        {
            var empresa = await _repository.GetByIdAsync(id);
            if (empresa is null)
            {
                return null;
            }

            empresa.Cnpj = await ValidarCnpjAsync(dto.Cnpj, ignorarId: id);
            await ValidarEmailAsync(dto.Contato.Email, ignorarId: id);
            empresa.Nome = dto.Nome.Trim();
            await _enderecoService.PreencherAsync(dto.Endereco, empresa.Endereco);
            EnderecoContatoMapper.Copiar(dto.Contato, empresa.Contato);

            await _repository.UpdateAsync(empresa);
            return ToDTO(empresa);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var empresa = await _repository.GetByIdAsync(id);
            if (empresa is null)
            {
                return false;
            }

            await _repository.DeleteAsync(empresa);
            return true;
        }

        /// <summary>Tira a mascara, confere os digitos e a unicidade. Devolve o CNPJ limpo.</summary>
        private async Task<string> ValidarCnpjAsync(string cnpjInformado, int? ignorarId)
        {
            var cnpj = Documento.NormalizarCnpj(cnpjInformado);

            if (!Documento.CnpjValido(cnpj))
            {
                throw new RegraNegocioException(TipoErro.Invalido, "CNPJ invalido.");
            }

            if (await _repository.CnpjExisteAsync(cnpj, ignorarId))
            {
                throw new RegraNegocioException(TipoErro.Conflito, "Ja existe uma empresa com este CNPJ.");
            }

            return cnpj;
        }

        private async Task ValidarEmailAsync(string email, int? ignorarId)
        {
            if (await _repository.EmailExisteAsync(EnderecoContatoMapper.NormalizarEmail(email), ignorarId))
            {
                throw new RegraNegocioException(TipoErro.Conflito, "Ja existe uma empresa com este e-mail.");
            }
        }

        private static EmpresaDTO ToDTO(UsuarioEmpresa empresa) => new()
        {
            Id = empresa.Id,
            Nome = empresa.Nome,
            Cnpj = empresa.Cnpj,
            Nota = empresa.Nota,
            TotalAvaliacoes = empresa.Avaliacoes.Count,
            Endereco = EnderecoContatoMapper.ToDTO(empresa.Endereco),
            Contato = EnderecoContatoMapper.ToDTO(empresa.Contato)
        };
    }
}
