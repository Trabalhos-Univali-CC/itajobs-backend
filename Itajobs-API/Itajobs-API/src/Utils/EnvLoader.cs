namespace Itajobs_API.src.Utils
{
    /// <summary>
    /// Leitor dos arquivos <c>Core/Secrets/*.env</c>: pares <c>CHAVE=VALOR</c>, uma por
    /// linha, <c>#</c> inicia comentario.
    /// </summary>
    /// <remarks>
    /// Existe para a senha do banco ficar fora do <c>appsettings.json</c>, que vai
    /// versionado. O <c>Program.cs</c> monta a connection string a partir daqui.
    /// </remarks>
    public static class EnvLoader
    {
        private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> Cache =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly Lock Gate = new();

        /// <summary>Onde os .env ficam ao lado do binario.</summary>
        public static string SecretsDirectory { get; } =
            Path.Combine(AppContext.BaseDirectory, "Core", "Secrets");

        /// <summary>
        /// Carrega (e memoiza) um arquivo .env. Arquivo ausente devolve dicionario
        /// vazio — o erro aparece na hora de conectar, nao aqui.
        /// </summary>
        public static IReadOnlyDictionary<string, string> Load(string fileName)
        {
            lock (Gate)
            {
                if (Cache.TryGetValue(fileName, out var cached))
                {
                    return cached;
                }

                var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var caminho = Path.Combine(SecretsDirectory, fileName);

                if (File.Exists(caminho))
                {
                    foreach (var bruta in File.ReadAllLines(caminho))
                    {
                        var linha = bruta.Trim();
                        if (linha.Length == 0 || linha.StartsWith('#'))
                        {
                            continue;
                        }

                        var separador = linha.IndexOf('=');
                        if (separador <= 0)
                        {
                            continue;
                        }

                        var chave = linha[..separador].Trim();
                        var valor = linha[(separador + 1)..].Trim().Trim('"');
                        valores[chave] = valor;
                    }
                }

                Cache[fileName] = valores;
                return valores;
            }
        }

        /// <summary>
        /// Le uma chave, caindo na variavel de ambiente do processo e depois no default.
        /// </summary>
        public static string Get(IReadOnlyDictionary<string, string> env, string chave, string padrao = "")
        {
            if (env.TryGetValue(chave, out var valor) && !string.IsNullOrWhiteSpace(valor))
            {
                return valor;
            }

            return Environment.GetEnvironmentVariable(chave) is { Length: > 0 } doProcesso
                ? doProcesso
                : padrao;
        }

        /// <summary>
        /// Monta a connection string do Npgsql a partir do .env informado.
        /// </summary>
        public static string BuildConnectionString(string fileName = "itajobs.env")
        {
            var env = Load(fileName);

            return $"Host={Get(env, "DB_HOST")};" +
                   $"Port={Get(env, "DB_PORT", "5432")};" +
                   $"Database={Get(env, "DB_DATABASE")};" +
                   $"Username={Get(env, "DB_USER")};" +
                   $"Password={Get(env, "DB_PASSWORD")}";
        }
    }
}
