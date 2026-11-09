using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Configuracao do JWT. Montada no <c>Program.cs</c> a partir do itajobs.env e
    /// usada tanto para gerar o token quanto para validar.
    /// </summary>
    public class JwtOptions
    {
        public const string Issuer = "itajobs-api";
        public const string Audience = "itajobs";

        /// <summary>Nomes das claims do token (sem o mapeamento antigo do .NET).</summary>
        public const string ClaimId = "sub";
        public const string ClaimNome = "name";
        public const string ClaimPerfil = "role";

        public required SymmetricSecurityKey Chave { get; init; }

        public required TimeSpan Validade { get; init; }

        /// <summary>A chave HMAC-SHA256 precisa de pelo menos 256 bits.</summary>
        public static JwtOptions Criar(string segredo, TimeSpan validade)
        {
            var bytes = Encoding.UTF8.GetBytes(segredo);
            if (bytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT_SECRET ausente ou curto demais no Core/Secrets/itajobs.env (minimo 32 caracteres).");
            }

            return new JwtOptions { Chave = new SymmetricSecurityKey(bytes), Validade = validade };
        }

        public TokenValidationParameters ParametrosDeValidacao() => new()
        {
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = Chave,
            NameClaimType = ClaimNome,
            RoleClaimType = ClaimPerfil,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    /// <summary>
    /// Gera o JWT do login.
    /// </summary>
    public class TokenService
    {
        private readonly JwtOptions _options;
        private readonly JsonWebTokenHandler _handler = new();

        public TokenService(JwtOptions options)
        {
            _options = options;
        }

        public (string Token, DateTime ExpiraEm) Gerar(UsuarioLogado usuario)
        {
            var expiraEm = DateTime.UtcNow.Add(_options.Validade);

            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = JwtOptions.Issuer,
                Audience = JwtOptions.Audience,
                Expires = expiraEm,
                SigningCredentials = new SigningCredentials(_options.Chave, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtOptions.ClaimId, usuario.Id.ToString()),
                    new Claim(JwtOptions.ClaimNome, usuario.Nome),
                    new Claim(JwtOptions.ClaimPerfil, usuario.Perfil)
                ])
            });

            return (token, expiraEm);
        }
    }
}
