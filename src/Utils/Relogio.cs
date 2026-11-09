namespace Itajobs_API.src.Utils
{
    /// <summary>
    /// "Agora" para gravar nas colunas <c>timestamp without time zone</c>.
    /// </summary>
    /// <remarks>
    /// As datas preenchidas pelo banco (DEFAULT CURRENT_TIMESTAMP) voltam sem fuso,
    /// no horario de Itajai (ver <c>DB_TIMEZONE</c> no <see cref="EnvLoader"/>). As que
    /// a API grava precisam sair igual: hora local, sem fuso; senao o JSON mistura
    /// "2026-11-09T14:28:00" com "2026-11-09T14:28:00-03:00".
    /// </remarks>
    public static class Relogio
    {
        private static readonly TimeZoneInfo Itajai = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

        public static DateTime Agora =>
            DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Itajai), DateTimeKind.Unspecified);
    }
}
