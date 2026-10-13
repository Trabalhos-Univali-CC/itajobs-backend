namespace Itajobs_API.src.DTO
{
    /// <summary>
    /// O que a API <b>devolve</b>. Separado do Model para a tabela poder mudar sem
    /// quebrar o contrato do cliente — e para nao vazar coluna que ninguem de fora
    /// deveria ver.
    /// </summary>
    public class ExemploDTO
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public int Quantidade { get; set; }

        public bool Ativo { get; set; }

        public DateTime CriadoEm { get; set; }
    }
}
