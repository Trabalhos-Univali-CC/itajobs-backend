namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API devolve de um candidato.
    /// </summary>
    public class CandidatoDTO
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Sobrenome { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public EnderecoDTO Endereco { get; set; } = new();

        public ContatoDTO Contato { get; set; } = new();

        /// <summary><c>null</c> enquanto o candidato nao criou o curriculo.</summary>
        public int? IdCurriculo { get; set; }
    }
}
