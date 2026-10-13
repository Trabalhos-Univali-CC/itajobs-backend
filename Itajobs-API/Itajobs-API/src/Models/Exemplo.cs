using System.ComponentModel.DataAnnotations;

namespace Itajobs_API.src.Models
{
    /// <summary>
    /// Entidade: representa a tabela no codigo. Uma classe aqui = uma tabela no banco,
    /// e uma propriedade = uma coluna.
    /// </summary>
    /// <remarks>
    /// Model nunca sai da API — quem atravessa a fronteira HTTP e o DTO.
    /// </remarks>
    public class Exemplo
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descricao { get; set; }

        public int Quantidade { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
