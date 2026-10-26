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

        public decimal? Nota { get; set; }

        public EnderecoDTO Endereco { get; set; } = new();

        public ContatoDTO Contato { get; set; } = new();
    }
}
