using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Login (senha ou Google), troca e recuperacao de senha e "quem sou eu". O
    /// cadastro continua nos <c>POST</c> de candidato e empresa.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>Nomes das politicas de rate limiting registradas no Program.cs.</summary>
        public const string LimiteLogin = "login";

        public const string LimiteRecuperacao = "recuperacao";

        private readonly AuthService _service;

        public AuthController(AuthService service)
        {
            _service = service;
        }

        /// <summary>POST /api/auth/login (limitado por IP, ver Program.cs)</summary>
        [AllowAnonymous]
        [EnableRateLimiting(LimiteLogin)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            return Ok(await _service.LoginAsync(dto));
        }

        /// <summary>PUT /api/auth/senha</summary>
        [HttpPut("senha")]
        public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaDTO dto)
        {
            await _service.TrocarSenhaAsync(User.Usuario(), dto);
            return NoContent();
        }

        /// <summary>
        /// POST /api/auth/esqueci-senha. Sempre 202 com a mesma mensagem, tenha o e-mail
        /// conta ou nao (senao a rota serviria para descobrir quem e cadastrado).
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(LimiteRecuperacao)]
        [HttpPost("esqueci-senha")]
        public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaDTO dto)
        {
            await _service.SolicitarRecuperacaoAsync(dto);
            return Accepted(new
            {
                mensagem = "Se este e-mail tiver conta no ItaJobs, enviamos um link para redefinir a senha."
            });
        }

        /// <summary>POST /api/auth/redefinir-senha (token do link do e-mail + senha nova)</summary>
        [AllowAnonymous]
        [EnableRateLimiting(LimiteRecuperacao)]
        [HttpPost("redefinir-senha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDTO dto)
        {
            await _service.RedefinirSenhaAsync(dto);
            return NoContent();
        }

        /// <summary>POST /api/auth/google (login com o ID token do Google)</summary>
        [AllowAnonymous]
        [EnableRateLimiting(LimiteLogin)]
        [HttpPost("google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginDTO dto)
        {
            return Ok(await _service.LoginGoogleAsync(dto));
        }

        /// <summary>POST /api/auth/google/vincular (liga um Google a conta logada)</summary>
        [HttpPost("google/vincular")]
        public async Task<IActionResult> VincularGoogle([FromBody] GoogleLoginDTO dto)
        {
            await _service.VincularGoogleAsync(User.Usuario(), dto);
            return NoContent();
        }

        /// <summary>DELETE /api/auth/google (desliga o Google da conta logada)</summary>
        [HttpDelete("google")]
        public async Task<IActionResult> DesvincularGoogle()
        {
            if (!await _service.DesvincularGoogleAsync(User.Usuario()))
            {
                return NotFound(new { mensagem = "Sua conta nao tem Google vinculado." });
            }

            return NoContent();
        }

        /// <summary>GET /api/auth/me</summary>
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(AuthService.ToDTO(User.Usuario()));
        }
    }
}
