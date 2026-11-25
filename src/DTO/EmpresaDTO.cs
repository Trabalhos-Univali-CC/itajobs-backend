namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API devolve de uma empresa.
    /// </summary>
    public class EmpresaDTO
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Cnpj { get; set; } = string.Empty;

        /// <summary>Media das avaliacoes (1 a 5); <c>null</c> sem avaliacao.</summary>
        public decimal? Nota { get; set; }

        public int TotalAvaliacoes { get; set; }

        public string? RazaoSocial { get; set; }

        public string? RamoAtuacao { get; set; }

        public string? DescricaoCurta { get; set; }

        public string? Sobre { get; set; }

        public string? Site { get; set; }

        public string? Linkedin { get; set; }

        public EnderecoDTO Endereco { get; set; } = new();

        public ContatoDTO Contato { get; set; } = new();
    }
}
