using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Conversoes de endereco e contato, usadas por <see cref="EmpresaService"/> e
    /// <see cref="CandidatoService"/>.
    /// </summary>
    internal static class EnderecoContatoMapper
    {
        public static void Copiar(EnderecoDTO origem, Endereco destino)
        {
            destino.Cidade = origem.Cidade.Trim();
            destino.Bairro = VazioParaNull(origem.Bairro);
            destino.Rua = origem.Rua.Trim();
            destino.Numero = origem.Numero.Trim();
            destino.Complemento = VazioParaNull(origem.Complemento);
        }

        public static void Copiar(ContatoDTO origem, Contato destino)
        {
            destino.Email = origem.Email.Trim().ToLowerInvariant();
            destino.Telefone = origem.Telefone.Trim();
        }

        public static EnderecoDTO ToDTO(Endereco endereco) => new()
        {
            Cidade = endereco.Cidade,
            Bairro = endereco.Bairro,
            Rua = endereco.Rua,
            Numero = endereco.Numero,
            Complemento = endereco.Complemento
        };

        public static ContatoDTO ToDTO(Contato contato) => new()
        {
            Email = contato.Email,
            Telefone = contato.Telefone
        };

        private static string? VazioParaNull(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
