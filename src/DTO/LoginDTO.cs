using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Corpo do <c>POST /api/auth/login</c>.
    /// </summary>
    /// <remarks>
    /// O e-mail e unico entre todas as contas, entao o tipo nao e mais necessario.
    /// Continua aceito por compatibilidade: se vier, a conta precisa ser daquele tipo.
    /// </remarks>
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Email invalido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha e obrigatoria.")]
        public string Senha { get; set; } = string.Empty;

        /// <summary>Opcional: <c>candidato</c> ou <c>empresa</c>.</summary>
        [RegularExpression("^(candidato|empresa)$", ErrorMessage = "Tipo deve ser \"candidato\" ou \"empresa\".")]
        public string? Tipo { get; set; }
    }

    /// <summary>Corpo do <c>POST /api/auth/esqueci-senha</c>.</summary>
    public class EsqueciSenhaDTO
    {
        [Required(ErrorMessage = "Email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Email invalido.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>Corpo do <c>POST /api/auth/redefinir-senha</c>.</summary>
    public class RedefinirSenhaDTO
    {
        /// <summary>O token que chegou no link do e-mail.</summary>
        [Required(ErrorMessage = "Token e obrigatorio.")]
        [StringLength(200)]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "NovaSenha e obrigatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A nova senha deve ter entre 8 e 100 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;
    }

    /// <summary>Corpo do login e do vinculo com Google.</summary>
    public class GoogleLoginDTO
    {
        /// <summary>O ID token (JWT) que o botao "Entrar com Google" devolve no front.</summary>
        [Required(ErrorMessage = "IdToken e obrigatorio.")]
        [StringLength(4096)]
        public string IdToken { get; set; } = string.Empty;
    }
}
