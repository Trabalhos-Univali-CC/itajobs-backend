namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// Uma pagina de resultados de uma listagem paginada.
    /// </summary>
    public class PaginaDTO<T>
    {
        public List<T> Itens { get; set; } = [];

        /// <summary>Comeca em 1.</summary>
        public int Pagina { get; set; }

        public int TamanhoPagina { get; set; }

        /// <summary>Total de itens em todas as paginas.</summary>
        public int Total { get; set; }

        public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);
    }
}
