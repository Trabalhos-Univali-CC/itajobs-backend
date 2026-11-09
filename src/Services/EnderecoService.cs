using System.Globalization;
using System.Text;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de endereco, usadas por <see cref="EmpresaService"/> e
    /// <see cref="CandidatoService"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>O ItaJobs e so para Itajai/SC: o CEP e consultado e, se for de outra cidade, o cadastro e recusado.</item>
    /// <item>A cidade sai do CEP, nunca do que o cliente mandou.</item>
    /// <item>Rua e bairro em branco sao completados pelo CEP.</item>
    /// </list>
    /// </remarks>
    public class EnderecoService
    {
        /// <summary>Codigo IBGE do municipio de Itajai/SC.</summary>
        public const string CodigoIbgeItajai = "4208203";

        private readonly ICepService _cep;

        public EnderecoService(ICepService cep)
        {
            _cep = cep;
        }

        /// <summary>Valida o CEP e copia o endereco para o Model.</summary>
        public async Task PreencherAsync(EnderecoDTO origem, Endereco destino)
        {
            var cep = NormalizarCep(origem.Cep);
            if (cep.Length != 8)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "CEP invalido. Informe os 8 digitos.");
            }

            var info = await _cep.ConsultarAsync(cep)
                ?? throw new RegraNegocioException(TipoErro.Invalido, $"CEP {Formatar(cep)} nao encontrado.");

            if (!EhItajai(info))
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    $"O ItaJobs atende apenas Itajai/SC. O CEP {Formatar(cep)} e de {info.Cidade}/{info.Uf}.");
            }

            var rua = VazioParaNull(origem.Rua) ?? VazioParaNull(info.Logradouro)
                ?? throw new RegraNegocioException(TipoErro.Invalido, "Informe a rua: o CEP nao indica uma.");

            destino.Cep = cep;
            destino.Cidade = info.Cidade;
            destino.Bairro = VazioParaNull(origem.Bairro) ?? VazioParaNull(info.Bairro);
            destino.Rua = rua;
            destino.Numero = origem.Numero.Trim();
            destino.Complemento = VazioParaNull(origem.Complemento);
        }

        public static string NormalizarCep(string cep)
        {
            return new string(cep.Where(char.IsAsciiDigit).ToArray());
        }

        /// <summary>Pelo codigo IBGE; se o ViaCEP nao mandar, pelo nome sem acento + UF.</summary>
        private static bool EhItajai(CepInfo info)
        {
            if (!string.IsNullOrEmpty(info.CodigoIbge))
            {
                return info.CodigoIbge == CodigoIbgeItajai;
            }

            return info.Uf.Equals("SC", StringComparison.OrdinalIgnoreCase)
                && SemAcento(info.Cidade).Equals("itajai", StringComparison.OrdinalIgnoreCase);
        }

        private static string SemAcento(string texto)
        {
            var decomposto = texto.Trim().Normalize(NormalizationForm.FormD);
            return new string(decomposto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        }

        private static string Formatar(string cep) => $"{cep[..5]}-{cep[5..]}";

        private static string? VazioParaNull(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
