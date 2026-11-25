using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;
using Itajobs_API.src.Utils;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras das imagens da empresa.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Uma logo (ate 2 MB) e um banner (ate 5 MB); enviar de novo substitui.</item>
    /// <item>Galeria: ate 10 fotos de ate 5 MB, na ordem do campo <c>ordem</c>.</item>
    /// <item>So JPG, PNG ou WebP, conferido pelo conteudo do arquivo.</item>
    /// <item>Sao publicas: aparecem na pagina da empresa e nas vagas, sem login.</item>
    /// </list>
    /// </remarks>
    public class EmpresaImagemService
    {
        public const int TamanhoMaximoLogo = 2 * 1024 * 1024;
        public const int TamanhoMaximoBanner = 5 * 1024 * 1024;
        public const int TamanhoMaximoGaleria = 5 * 1024 * 1024;
        public const int MaximoFotosGaleria = 10;

        private readonly IEmpresaImagemRepository _repository;
        private readonly IEmpresaRepository _empresaRepository;

        public EmpresaImagemService(IEmpresaImagemRepository repository, IEmpresaRepository empresaRepository)
        {
            _repository = repository;
            _empresaRepository = empresaRepository;
        }

        /// <summary><c>null</c> = empresa nao encontrada.</summary>
        public async Task<List<EmpresaImagemDTO>?> ListarAsync(int idEmpresa)
        {
            if (!await _empresaRepository.ExisteAsync(idEmpresa))
            {
                return null;
            }

            var imagens = await _repository.ListarSemConteudoAsync(idEmpresa);
            return imagens.Select(ToDTO).ToList();
        }

        /// <summary>Cria ou substitui a logo/banner. <c>null</c> = empresa nao encontrada.</summary>
        public async Task<EmpresaImagemDTO?> SalvarUnicaAsync(int idEmpresa, TipoImagemEmpresa tipo, string? nomeOriginal, byte[] conteudo)
        {
            ExigirUnica(tipo);
            var limite = tipo == TipoImagemEmpresa.Logo ? TamanhoMaximoLogo : TamanhoMaximoBanner;
            var contentType = ValidarImagem(conteudo, limite, tipo == TipoImagemEmpresa.Logo ? "A logo" : "O banner");

            if (!await _empresaRepository.ExisteAsync(idEmpresa))
            {
                return null;
            }

            var arquivo = Novo(idEmpresa, tipo, ordem: 0, nomeOriginal, contentType, conteudo);
            await _repository.SalvarUnicaAsync(arquivo);
            return ToDTO(arquivo);
        }

        /// <summary>Adiciona uma foto na galeria. <c>null</c> = empresa nao encontrada.</summary>
        /// <param name="ordem">Sem informar, vai para o fim.</param>
        public async Task<EmpresaImagemDTO?> AdicionarGaleriaAsync(int idEmpresa, string? nomeOriginal, byte[] conteudo, int? ordem)
        {
            if (ordem is < 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "A ordem nao pode ser negativa.");
            }

            var contentType = ValidarImagem(conteudo, TamanhoMaximoGaleria, "A foto");

            if (!await _empresaRepository.ExisteAsync(idEmpresa))
            {
                return null;
            }

            if (await _repository.ContarGaleriaAsync(idEmpresa) >= MaximoFotosGaleria)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    $"A galeria aceita no maximo {MaximoFotosGaleria} fotos. Apague uma antes de enviar outra.");
            }

            var posicao = ordem ?? ((await _repository.MaiorOrdemGaleriaAsync(idEmpresa) ?? -1) + 1);

            var arquivo = Novo(idEmpresa, TipoImagemEmpresa.Galeria, posicao, nomeOriginal, contentType, conteudo);
            await _repository.AdicionarAsync(arquivo);
            return ToDTO(arquivo);
        }

        /// <summary><c>null</c> = a empresa nao tem esta logo/banner.</summary>
        public async Task<ImagemDownloadDTO?> BaixarUnicaAsync(int idEmpresa, TipoImagemEmpresa tipo)
        {
            ExigirUnica(tipo);
            var arquivo = await _repository.GetUnicaAsync(idEmpresa, tipo);
            return arquivo is null ? null : ToDownload(arquivo);
        }

        /// <summary><c>null</c> = foto nao encontrada nesta empresa.</summary>
        public async Task<ImagemDownloadDTO?> BaixarGaleriaAsync(int idEmpresa, int idImagem)
        {
            var arquivo = await _repository.GetGaleriaAsync(idEmpresa, idImagem);
            return arquivo is null ? null : ToDownload(arquivo);
        }

        public Task<bool> AlterarOrdemAsync(int idEmpresa, int idImagem, int ordem)
        {
            return _repository.AlterarOrdemGaleriaAsync(idEmpresa, idImagem, ordem);
        }

        public Task<bool> DeleteUnicaAsync(int idEmpresa, TipoImagemEmpresa tipo)
        {
            ExigirUnica(tipo);
            return _repository.DeleteUnicaAsync(idEmpresa, tipo);
        }

        public Task<bool> DeleteGaleriaAsync(int idEmpresa, int idImagem)
        {
            return _repository.DeleteGaleriaAsync(idEmpresa, idImagem);
        }

        /// <summary>A rota <c>/imagens/{tipo}</c> e so para logo e banner.</summary>
        private static void ExigirUnica(TipoImagemEmpresa tipo)
        {
            if (tipo == TipoImagemEmpresa.Galeria)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "Fotos da galeria usam /imagens/galeria (POST para enviar, /galeria/{idImagem} para ver ou apagar).");
            }
        }

        private static string ValidarImagem(byte[] conteudo, int limite, string oQue)
        {
            if (conteudo.Length == 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "O arquivo esta vazio.");
            }

            if (conteudo.Length > limite)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    $"{oQue} pode ter no maximo {limite / (1024 * 1024)} MB.");
            }

            return Imagem.DetectarContentType(conteudo)
                ?? throw new RegraNegocioException(TipoErro.Invalido, $"{oQue} precisa ser JPG, PNG ou WebP.");
        }

        private static EmpresaArquivo Novo(int idEmpresa, TipoImagemEmpresa tipo, int ordem, string? nomeOriginal, string contentType, byte[] conteudo) => new()
        {
            IdEmpresa = idEmpresa,
            Tipo = tipo,
            Ordem = ordem,
            Nome = Imagem.NomeDoArquivo(nomeOriginal, $"{tipo.ToString().ToLowerInvariant()}.{Imagem.Extensao(contentType)}"),
            ContentType = contentType,
            Tamanho = conteudo.Length,
            Conteudo = conteudo,
            EnviadoEm = AgoraEmMicrossegundos()
        };

        /// <summary>
        /// O Postgres guarda microssegundos; o .NET tem 100 ns. Cortando aqui, a data
        /// (e o <c>?v=</c> da URL) sai igual na resposta do envio e nas leituras seguintes.
        /// </summary>
        private static DateTime AgoraEmMicrossegundos()
        {
            var agora = DateTime.UtcNow;
            return new DateTime(agora.Ticks - agora.Ticks % 10, DateTimeKind.Utc);
        }

        private static ImagemDownloadDTO ToDownload(EmpresaArquivo arquivo) =>
            new(arquivo.Nome, arquivo.ContentType, arquivo.Conteudo, arquivo.EnviadoEm);

        /// <summary>
        /// A URL leva <c>?v=</c> com a data do envio: quando a imagem muda, a URL muda, e
        /// o navegador nao fica preso na versao antiga do cache.
        /// </summary>
        private static EmpresaImagemDTO ToDTO(EmpresaArquivo arquivo) => new()
        {
            Id = arquivo.Id,
            Tipo = arquivo.Tipo,
            Ordem = arquivo.Ordem,
            Nome = arquivo.Nome,
            ContentType = arquivo.ContentType,
            Tamanho = arquivo.Tamanho,
            EnviadoEm = arquivo.EnviadoEm,
            Url = (arquivo.Tipo == TipoImagemEmpresa.Galeria
                    ? $"/api/empresa/{arquivo.IdEmpresa}/imagens/galeria/{arquivo.Id}"
                    : $"/api/empresa/{arquivo.IdEmpresa}/imagens/{arquivo.Tipo.ToString().ToLowerInvariant()}")
                + $"?v={arquivo.EnviadoEm.Ticks}"
        };
    }
}
