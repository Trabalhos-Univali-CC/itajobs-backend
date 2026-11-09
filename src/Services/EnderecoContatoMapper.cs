using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Conversoes de endereco e contato, usadas por <see cref="EmpresaService"/> e
    /// <see cref="CandidatoService"/>. A entrada do endereco tem regra (CEP de
    /// Itajai) e fica no <see cref="EnderecoService"/>.
    /// </summary>
    internal static class EnderecoContatoMapper
    {
        public static void Copiar(ContatoDTO origem, Contato destino)
        {
            destino.Email = NormalizarEmail(origem.Email);
            destino.Telefone = origem.Telefone.Trim();
        }

        /// <summary>E-mail e guardado e comparado sempre assim.</summary>
        public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

        public static EnderecoDTO ToDTO(Endereco endereco) => new()
        {
            Cep = endereco.Cep,
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
    }
}
