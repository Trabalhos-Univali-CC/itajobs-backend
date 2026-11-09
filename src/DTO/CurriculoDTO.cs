namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API devolve de um curriculo, ja com todos os itens. Dos arquivos sai
    /// so a descricao; o conteudo baixa em <c>GET /api/curriculo/{id}/arquivos/{tipo}</c>.
    /// </summary>
    public class CurriculoDTO
    {
        public int Id { get; set; }

        public int IdCandidato { get; set; }

        public string NomeCandidato { get; set; } = string.Empty;

        public string? Resumo { get; set; }

        public List<ExperienciaDTO> Experiencias { get; set; } = [];

        public List<HabilidadeDTO> Habilidades { get; set; } = [];

        public List<FormacaoDTO> Formacoes { get; set; } = [];

        public List<CursoDTO> Cursos { get; set; } = [];

        public List<ArquivoDTO> Arquivos { get; set; } = [];
    }
}
