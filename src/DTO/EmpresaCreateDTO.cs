using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API recebe no POST e no PUT de empresa.
    /// </summary>
    /// <remarks>
    /// Sem <c>Nota</c>: a empresa nao define a propria nota; e a media das avaliacoes.
    /// Logo, banner e galeria vao pelas rotas de imagem (<c>/api/empresa/{id}/imagens</c>).
    /// </remarks>
    public class EmpresaCreateDTO
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Aceita com ou sem mascara (12.345.678/0001-95).</summary>
        [Required(ErrorMessage = "CNPJ e obrigatorio.")]
        [StringLength(18)]
        public string Cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereco e obrigatorio.")]
        public EnderecoDTO Endereco { get; set; } = new();

        [Required(ErrorMessage = "Contato e obrigatorio.")]
        public ContatoDTO Contato { get; set; } = new();

        // Perfil publico: tudo opcional. Vazio apaga no PUT.

        [StringLength(200)]
        public string? RazaoSocial { get; set; }

        [StringLength(100)]
        public string? RamoAtuacao { get; set; }

        [StringLength(500)]
        public string? DescricaoCurta { get; set; }

        [StringLength(5000)]
        public string? Sobre { get; set; }

        /// <summary>Link com http:// ou https://.</summary>
        [StringLength(255)]
        public string? Site { get; set; }

        /// <summary>Link do perfil no LinkedIn (https://www.linkedin.com/company/...).</summary>
        [StringLength(255)]
        public string? Linkedin { get; set; }

        /// <summary>Obrigatoria no POST. No PUT e ignorada: troca pelo <c>PUT /api/auth/senha</c>.</summary>
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        public string? Senha { get; set; }
    }
}
