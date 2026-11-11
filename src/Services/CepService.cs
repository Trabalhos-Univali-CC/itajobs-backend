using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

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
    /// <remarks>
    /// As respostas ficam em cache: CEP quase nunca muda, e o ViaCEP bloqueia quem
    /// consulta demais. CEP encontrado fica 24 h; inexistente, 1 h. Falha de rede nao
    /// entra no cache.
    /// </remarks>
    public class ViaCepService : ICepService
    {
        private static readonly TimeSpan TempoEncontrado = TimeSpan.FromHours(24);
        private static readonly TimeSpan TempoInexistente = TimeSpan.FromHours(1);

        /// <summary>Marca "CEP nao existe" no cache (o cache nao guarda <c>null</c> como resposta).</summary>
        private static readonly CepInfo Inexistente = new("", "", "", "", "", "");

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public ViaCepService(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public async Task<CepInfo?> ConsultarAsync(string cep)
        {
            var chave = $"cep:{cep}";
            if (_cache.TryGetValue(chave, out CepInfo? guardado))
            {
                return ReferenceEquals(guardado, Inexistente) ? null : guardado;
            }

            var info = await ConsultarViaCepAsync(cep);

            _cache.Set(chave, info ?? Inexistente, info is null ? TempoInexistente : TempoEncontrado);
            return info;
        }

        private async Task<CepInfo?> ConsultarViaCepAsync(string cep)
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
