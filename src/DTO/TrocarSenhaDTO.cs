using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Corpo do <c>PUT /api/auth/senha</c>. Pede a senha atual para um token
    /// roubado nao bastar para tomar a conta.
    /// </summary>
    public class TrocarSenhaDTO
    {
        [Required(ErrorMessage = "SenhaAtual e obrigatoria.")]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "NovaSenha e obrigatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A nova senha deve ter entre 8 e 100 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;
    }
}
