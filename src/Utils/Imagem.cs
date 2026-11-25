namespace Itajobs_API.src.Utils
{
    /// <summary>
    /// Descobre o tipo de uma imagem pelo conteudo (assinatura binaria, o "magic
    /// number"), nao pelo nome nem pelo content-type que o cliente mandou: um .exe
    /// renomeado para .png nao passa.
    /// </summary>
    public static class Imagem
    {
        public const string Jpeg = "image/jpeg";
        public const string Png = "image/png";
        public const string Webp = "image/webp";

        /// <summary>O content-type real (JPEG, PNG ou WebP), ou <c>null</c> se nao for nenhum deles.</summary>
        public static string? DetectarContentType(byte[] conteudo)
        {
            var bytes = conteudo.AsSpan();

            if (bytes.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
            {
                return Jpeg;
            }

            if (bytes.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            {
                return Png;
            }

            if (bytes.Length >= 12 && bytes.StartsWith("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8))
            {
                return Webp;
            }

            return null;
        }

        /// <summary>So o nome (sem pasta), cortado em 255; vazio vira <paramref name="padrao"/>.</summary>
        public static string NomeDoArquivo(string? nomeOriginal, string padrao)
        {
            var nome = Path.GetFileName(nomeOriginal ?? string.Empty).Trim();
            if (nome.Length == 0)
            {
                nome = padrao;
            }

            return nome.Length > 255 ? nome[^255..] : nome;
        }

        /// <summary>"png", "jpeg", "webp" a partir do content-type.</summary>
        public static string Extensao(string contentType) => contentType[(contentType.IndexOf('/') + 1)..];
    }
}
