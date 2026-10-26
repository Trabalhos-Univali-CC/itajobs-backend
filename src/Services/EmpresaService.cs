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
    /// <item>A nota vai de <see cref="NotaMinima"/> a <see cref="NotaMaxima"/> e so muda pela rota propria.</item>
    /// <item>Apagar a empresa apaga endereco, contato, vagas e as candidaturas dessas vagas.</item>
    /// </list>
    /// </remarks>
    public class EmpresaService
    {
        public const decimal NotaMinima = 0;
        public const decimal NotaMaxima = 5;

        private readonly IEmpresaRepository _repository;

        public EmpresaService(IEmpresaRepository repository)
        {
            _repository = repository;
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
            var cnpj = await ValidarCnpjAsync(dto.Cnpj, ignorarId: null);

            var empresa = new UsuarioEmpresa
            {
                Nome = dto.Nome.Trim(),
                Cnpj = cnpj,
                Endereco = new Endereco(),
                Contato = new Contato()
            };
            EnderecoContatoMapper.Copiar(dto.Endereco, empresa.Endereco);
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
            empresa.Nome = dto.Nome.Trim();
            EnderecoContatoMapper.Copiar(dto.Endereco, empresa.Endereco);
            EnderecoContatoMapper.Copiar(dto.Contato, empresa.Contato);

            await _repository.UpdateAsync(empresa);
            return ToDTO(empresa);
        }

        public async Task<EmpresaDTO?> AtualizarNotaAsync(int id, decimal nota)
        {
            if (nota < NotaMinima || nota > NotaMaxima)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    $"A nota deve ficar entre {NotaMinima} e {NotaMaxima}.");
            }

            var empresa = await _repository.GetByIdAsync(id);
            if (empresa is null)
            {
                return null;
            }

            empresa.Nota = Math.Round(nota, 2);
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

        private static EmpresaDTO ToDTO(UsuarioEmpresa empresa) => new()
        {
            Id = empresa.Id,
            Nome = empresa.Nome,
            Cnpj = empresa.Cnpj,
            Nota = empresa.Nota,
            Endereco = EnderecoContatoMapper.ToDTO(empresa.Endereco),
            Contato = EnderecoContatoMapper.ToDTO(empresa.Contato)
        };
    }
}
