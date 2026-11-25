using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Itajobs_API.src.Services
{
    /// <summary>O que interessa de um ID token do Google ja validado.</summary>
    public record GoogleUsuario(string Sub, string? Email, bool EmailVerificado);

    /// <summary>
    /// Valida o ID token que o botao "Entrar com Google" entrega ao front.
    /// </summary>
    /// <remarks>
    /// Confere assinatura (chaves publicas do Google, baixadas e renovadas sozinhas pelo
    /// <see cref="ConfigurationManager{T}"/>), emissor, validade e se o token foi emitido
    /// para o nosso Client ID (<c>GOOGLE_CLIENT_ID</c>). Sem essa ultima checagem, um
    /// token de qualquer outro site que usa Google serviria aqui.
    /// </remarks>
    public class GoogleTokenService
    {
        private static readonly string[] Emissores = ["accounts.google.com", "https://accounts.google.com"];

        private readonly string[] _clientIds;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configuracao;
        private readonly JsonWebTokenHandler _handler = new();

        /// <param name="clientIds">Um ou mais Client IDs (web, Android...), separados por virgula.</param>
        public GoogleTokenService(string? clientIds)
        {
            _clientIds = (clientIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            _configuracao = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://accounts.google.com/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true });
        }

        public bool Configurado => _clientIds.Length > 0;

        /// <summary>401 se o token nao vale; 503 se o login com Google nao esta configurado.</summary>
        public async Task<GoogleUsuario> ValidarAsync(string idToken)
        {
            if (!Configurado)
            {
                throw new RegraNegocioException(TipoErro.Indisponivel,
                    "O login com Google nao esta disponivel (GOOGLE_CLIENT_ID nao configurado).");
            }

            OpenIdConnectConfiguration configuracao;
            try
            {
                configuracao = await _configuracao.GetConfigurationAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                throw new RegraNegocioException(TipoErro.Indisponivel,
                    "Nao foi possivel falar com o Google agora. Tente de novo em instantes.");
            }

            var resultado = await _handler.ValidateTokenAsync(idToken, new TokenValidationParameters
            {
                ValidIssuers = Emissores,
                ValidAudiences = _clientIds,
                IssuerSigningKeys = configuracao.SigningKeys,
                ClockSkew = TimeSpan.FromMinutes(2)
            });

            if (!resultado.IsValid
                || !resultado.Claims.TryGetValue("sub", out var sub)
                || string.IsNullOrWhiteSpace(sub?.ToString()))
            {
                throw new RegraNegocioException(TipoErro.NaoAutenticado, "Login com Google invalido ou expirado.");
            }

            resultado.Claims.TryGetValue("email", out var email);
            resultado.Claims.TryGetValue("email_verified", out var verificado);

            return new GoogleUsuario(
                sub.ToString()!,
                email?.ToString(),
                verificado is true || string.Equals(verificado?.ToString(), "true", StringComparison.OrdinalIgnoreCase));
        }
    }
}
