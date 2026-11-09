using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Services;

namespace Itajobs_API.src.Controllers
{
    /// <summary>
    /// Login, troca de senha e "quem sou eu". O cadastro continua nos <c>POST</c> de
    /// candidato e empresa.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>Nome da politica de rate limiting registrada no Program.cs.</summary>
        public const string LimiteLogin = "login";

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

        /// <summary>GET /api/auth/me</summary>
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(AuthService.ToDTO(User.Usuario()));
        }
    }
}
