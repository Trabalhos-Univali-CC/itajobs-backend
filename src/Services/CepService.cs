using System.Net.Http.Json;
using System.Text.Json;

namespace Itajobs_API.src.Services
{
    /// <summary>O que interessa de um CEP consultado.</summary>
    public record CepInfo(string Cep, string Logradouro, string Bairro, string Cidade, string Uf, string CodigoIbge);

    /// <summary>
    /// Consulta de CEP. Interface para dar para trocar o ViaCEP (ou simular nos
    /// testes) sem mexer no <see cref="EnderecoService"/>.
    /// </summary>
    public interface ICepService
    {
        /// <summary><c>null</c> = CEP nao existe.</summary>
        /// <param name="cep">8 digitos, sem mascara.</param>
        Task<CepInfo?> ConsultarAsync(string cep);
    }

    /// <summary>
    /// <see cref="ICepService"/> pelo ViaCEP (viacep.com.br). O HttpClient vem
    /// configurado do <c>Program.cs</c>.
    /// </summary>
    public class ViaCepService : ICepService
    {
        private readonly HttpClient _http;

        public ViaCepService(HttpClient http)
        {
            _http = http;
        }

        public async Task<CepInfo?> ConsultarAsync(string cep)
        {
            JsonElement json;
            try
            {
                json = await _http.GetFromJsonAsync<JsonElement>($"ws/{cep}/json/");
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
            {
                throw new RegraNegocioException(TipoErro.Indisponivel,
                    "Nao foi possivel consultar o CEP agora. Tente de novo em instantes.");
            }

            // CEP com formato valido mas inexistente: { "erro": true } (ou "true").
            if (json.TryGetProperty("erro", out _))
            {
                return null;
            }

            string Campo(string nome) => json.TryGetProperty(nome, out var valor) ? valor.GetString() ?? "" : "";

            return new CepInfo(cep, Campo("logradouro"), Campo("bairro"), Campo("localidade"), Campo("uf"), Campo("ibge"));
        }
    }
}
