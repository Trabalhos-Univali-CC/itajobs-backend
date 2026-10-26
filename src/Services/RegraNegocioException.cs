namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Que regra foi quebrada. O Service diz o <i>tipo</i> do problema; quem escolhe
    /// o status HTTP e o <c>RegraNegocioFilter</c> nos Controllers.
    /// </summary>
    public enum TipoErro
    {
        /// <summary>Dado bem formado mas que a regra nao aceita (CPF invalido, data no futuro...).</summary>
        Invalido,

        /// <summary>Algo que a operacao depende nao existe (vaga de uma empresa inexistente...).</summary>
        NaoEncontrado,

        /// <summary>Choca com o que ja esta no banco (CNPJ repetido, candidatura duplicada...).</summary>
        Conflito
    }

    /// <summary>
    /// Lancada pelos Services quando uma regra de negocio impede a operacao.
    /// </summary>
    /// <remarks>
    /// "Nao achei o recurso da rota" continua sendo <c>null</c> (o Controller vira
    /// 404). Esta excecao e para o resto: a regra que diz <i>por que</i>
    /// a operacao nao pode acontecer.
    /// </remarks>
    public class RegraNegocioException : Exception
    {
        public TipoErro Tipo { get; }

        public RegraNegocioException(TipoErro tipo, string mensagem) : base(mensagem)
        {
            Tipo = tipo;
        }
    }
}
