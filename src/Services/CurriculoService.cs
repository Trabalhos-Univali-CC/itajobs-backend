using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;
using Itajobs_API.src.Utils;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de curriculo e dos itens dele.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>O candidato precisa existir e tem no maximo um curriculo (1:1).</item>
    /// <item>Experiencia: inicio e fim nao ficam no futuro, e o fim nao vem antes do inicio.</item>
    /// <item>Formacao: inicio nao fica no futuro; o fim pode (previsao), mas nao vem antes do inicio.</item>
    /// <item>Curso: a conclusao nao fica no futuro.</item>
    /// <item>Habilidade nao se repete no mesmo curriculo (sem diferenciar maiuscula).</item>
    /// <item>Um PDF (ate 5 MB) e uma foto JPG/PNG/WebP (ate 2 MB). O tipo e conferido pelo
    /// conteudo do arquivo, nao pelo nome nem pelo content-type que o cliente mandou.</item>
    /// <item>Apagar o curriculo apaga itens e arquivos.</item>
    /// </list>
    /// </remarks>
    public class CurriculoService
    {
        public const int TamanhoMaximoPdf = 5 * 1024 * 1024;
        public const int TamanhoMaximoFoto = 2 * 1024 * 1024;

        private readonly ICurriculoRepository _repository;
        private readonly ICandidatoRepository _candidatoRepository;

        public CurriculoService(ICurriculoRepository repository, ICandidatoRepository candidatoRepository)
        {
            _repository = repository;
            _candidatoRepository = candidatoRepository;
        }

        public async Task<CurriculoDTO?> GetByIdAsync(int id)
        {
            var curriculo = await _repository.GetByIdAsync(id);
            return curriculo is null ? null : await MontarDTOAsync(curriculo);
        }

        public async Task<CurriculoDTO?> GetByCandidatoAsync(int idCandidato)
        {
            var curriculo = await _repository.GetByCandidatoAsync(idCandidato);
            return curriculo is null ? null : await MontarDTOAsync(curriculo);
        }

        public async Task<CurriculoDTO> CreateAsync(CurriculoCreateDTO dto)
        {
            var idCandidato = dto.IdCandidato!.Value;

            var candidato = await _candidatoRepository.GetByIdAsync(idCandidato);
            if (candidato is null)
            {
                throw new RegraNegocioException(TipoErro.NaoEncontrado,
                    $"Candidato {idCandidato} nao encontrado.");
            }

            if (candidato.Curriculo is not null)
            {
                throw new RegraNegocioException(TipoErro.Conflito,
                    $"O candidato {idCandidato} ja tem um curriculo (id {candidato.Curriculo.Id}).");
            }

            var curriculo = new Curriculo { IdCandidato = idCandidato, Candidato = candidato };

            var criado = await _repository.CreateAsync(curriculo);
            return await MontarDTOAsync(criado);
        }

        /// <summary><c>null</c> = curriculo nao encontrado.</summary>
        public async Task<CurriculoDTO?> UpdateAsync(int id, CurriculoUpdateDTO dto)
        {
            var curriculo = await _repository.GetByIdAsync(id);
            if (curriculo is null)
            {
                return null;
            }

            curriculo.Resumo = VazioParaNull(dto.Resumo);
            await _repository.UpdateAsync(curriculo);
            return await MontarDTOAsync(curriculo);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var curriculo = await _repository.GetByIdAsync(id);
            if (curriculo is null)
            {
                return false;
            }

            await _repository.DeleteAsync(curriculo);
            return true;
        }

        /// <summary><c>null</c> = curriculo nao encontrado.</summary>
        public async Task<ExperienciaDTO?> AddExperienciaAsync(int idCurriculo, ExperienciaCreateDTO dto)
        {
            ValidarPeriodo(dto.DataInicio!.Value, dto.DataFim);

            if (await _repository.GetByIdAsync(idCurriculo) is null)
            {
                return null;
            }

            var experiencia = new LogExperiencia { IdCurriculo = idCurriculo };
            Copiar(dto, experiencia);

            var criada = await _repository.AddExperienciaAsync(experiencia);
            return ToDTO(criada);
        }

        /// <summary><c>null</c> = experiencia nao encontrada neste curriculo.</summary>
        public async Task<ExperienciaDTO?> UpdateExperienciaAsync(int idCurriculo, int idExperiencia, ExperienciaCreateDTO dto)
        {
            ValidarPeriodo(dto.DataInicio!.Value, dto.DataFim);

            var experiencia = await _repository.GetExperienciaAsync(idCurriculo, idExperiencia);
            if (experiencia is null)
            {
                return null;
            }

            Copiar(dto, experiencia);

            await _repository.UpdateExperienciaAsync(experiencia);
            return ToDTO(experiencia);
        }

        public async Task<bool> DeleteExperienciaAsync(int idCurriculo, int idExperiencia)
        {
            var experiencia = await _repository.GetExperienciaAsync(idCurriculo, idExperiencia);
            if (experiencia is null)
            {
                return false;
            }

            await _repository.DeleteExperienciaAsync(experiencia);
            return true;
        }

        /// <summary><c>null</c> = curriculo nao encontrado.</summary>
        public async Task<HabilidadeDTO?> AddHabilidadeAsync(int idCurriculo, HabilidadeCreateDTO dto)
        {
            var descricao = dto.Descricao.Trim();
            if (descricao.Length == 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "A descricao da habilidade esta vazia.");
            }

            if (await _repository.GetByIdAsync(idCurriculo) is null)
            {
                return null;
            }

            if (await _repository.HabilidadeExisteAsync(idCurriculo, descricao))
            {
                throw new RegraNegocioException(TipoErro.Conflito,
                    $"A habilidade \"{descricao}\" ja esta neste curriculo.");
            }

            var criada = await _repository.AddHabilidadeAsync(
                new LogHabilidade { IdCurriculo = idCurriculo, Descricao = descricao });
            return ToDTO(criada);
        }

        public async Task<bool> DeleteHabilidadeAsync(int idCurriculo, int idHabilidade)
        {
            var habilidade = await _repository.GetHabilidadeAsync(idCurriculo, idHabilidade);
            if (habilidade is null)
            {
                return false;
            }

            await _repository.DeleteHabilidadeAsync(habilidade);
            return true;
        }

        /// <summary><c>null</c> = curriculo nao encontrado.</summary>
        public async Task<FormacaoDTO?> AddFormacaoAsync(int idCurriculo, FormacaoCreateDTO dto)
        {
            ValidarFormacao(dto.DataInicio!.Value, dto.DataFim);

            if (await _repository.GetByIdAsync(idCurriculo) is null)
            {
                return null;
            }

            var formacao = new LogFormacao { IdCurriculo = idCurriculo };
            Copiar(dto, formacao);

            var criada = await _repository.AddFormacaoAsync(formacao);
            return ToDTO(criada);
        }

        /// <summary><c>null</c> = formacao nao encontrada neste curriculo.</summary>
        public async Task<FormacaoDTO?> UpdateFormacaoAsync(int idCurriculo, int idFormacao, FormacaoCreateDTO dto)
        {
            ValidarFormacao(dto.DataInicio!.Value, dto.DataFim);

            var formacao = await _repository.GetFormacaoAsync(idCurriculo, idFormacao);
            if (formacao is null)
            {
                return null;
            }

            Copiar(dto, formacao);

            await _repository.UpdateFormacaoAsync(formacao);
            return ToDTO(formacao);
        }

        public async Task<bool> DeleteFormacaoAsync(int idCurriculo, int idFormacao)
        {
            var formacao = await _repository.GetFormacaoAsync(idCurriculo, idFormacao);
            if (formacao is null)
            {
                return false;
            }

            await _repository.DeleteFormacaoAsync(formacao);
            return true;
        }

        /// <summary><c>null</c> = curriculo nao encontrado.</summary>
        public async Task<CursoDTO?> AddCursoAsync(int idCurriculo, CursoCreateDTO dto)
        {
            ValidarConclusao(dto.DataConclusao);

            if (await _repository.GetByIdAsync(idCurriculo) is null)
            {
                return null;
            }

            var curso = new LogCurso { IdCurriculo = idCurriculo };
            Copiar(dto, curso);

            var criado = await _repository.AddCursoAsync(curso);
            return ToDTO(criado);
        }

        /// <summary><c>null</c> = curso nao encontrado neste curriculo.</summary>
        public async Task<CursoDTO?> UpdateCursoAsync(int idCurriculo, int idCurso, CursoCreateDTO dto)
        {
            ValidarConclusao(dto.DataConclusao);

            var curso = await _repository.GetCursoAsync(idCurriculo, idCurso);
            if (curso is null)
            {
                return null;
            }

            Copiar(dto, curso);

            await _repository.UpdateCursoAsync(curso);
            return ToDTO(curso);
        }

        public async Task<bool> DeleteCursoAsync(int idCurriculo, int idCurso)
        {
            var curso = await _repository.GetCursoAsync(idCurriculo, idCurso);
            if (curso is null)
            {
                return false;
            }

            await _repository.DeleteCursoAsync(curso);
            return true;
        }

        /// <summary>Cria ou substitui o arquivo do tipo. <c>null</c> = curriculo nao encontrado.</summary>
        public async Task<ArquivoDTO?> SalvarArquivoAsync(int idCurriculo, TipoArquivo tipo, string nomeOriginal, byte[] conteudo)
        {
            var contentType = ValidarArquivo(tipo, conteudo);

            if (await _repository.GetByIdAsync(idCurriculo) is null)
            {
                return null;
            }

            var arquivo = new CurriculoArquivo
            {
                IdCurriculo = idCurriculo,
                Tipo = tipo,
                Nome = NomeDoArquivo(nomeOriginal, tipo, contentType),
                ContentType = contentType,
                Tamanho = conteudo.Length,
                Conteudo = conteudo,
                EnviadoEm = Relogio.Agora
            };

            await _repository.SalvarArquivoAsync(arquivo);
            return ToDTO(arquivo);
        }

        /// <summary><c>null</c> = curriculo sem arquivo deste tipo.</summary>
        public async Task<ArquivoDownloadDTO?> GetArquivoAsync(int idCurriculo, TipoArquivo tipo)
        {
            var arquivo = await _repository.GetArquivoAsync(idCurriculo, tipo);
            return arquivo is null ? null : new ArquivoDownloadDTO(arquivo.Nome, arquivo.ContentType, arquivo.Conteudo);
        }

        public async Task<bool> DeleteArquivoAsync(int idCurriculo, TipoArquivo tipo)
        {
            return await _repository.DeleteArquivoAsync(idCurriculo, tipo);
        }

        /// <summary>
        /// Confere tamanho e assinatura binaria ("magic number") e devolve o
        /// content-type real.
        /// </summary>
        private static string ValidarArquivo(TipoArquivo tipo, byte[] conteudo)
        {
            if (conteudo.Length == 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "O arquivo esta vazio.");
            }

            if (tipo == TipoArquivo.Pdf)
            {
                if (conteudo.Length > TamanhoMaximoPdf)
                {
                    throw new RegraNegocioException(TipoErro.Invalido, "O PDF pode ter no maximo 5 MB.");
                }

                if (!ComecaCom(conteudo, "%PDF-"u8))
                {
                    throw new RegraNegocioException(TipoErro.Invalido, "O arquivo nao e um PDF.");
                }

                return "application/pdf";
            }

            if (conteudo.Length > TamanhoMaximoFoto)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "A foto pode ter no maximo 2 MB.");
            }

            if (ComecaCom(conteudo, [0xFF, 0xD8, 0xFF]))
            {
                return "image/jpeg";
            }

            if (ComecaCom(conteudo, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            {
                return "image/png";
            }

            if (conteudo.Length >= 12 && ComecaCom(conteudo, "RIFF"u8) && conteudo.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            {
                return "image/webp";
            }

            throw new RegraNegocioException(TipoErro.Invalido, "A foto precisa ser JPG, PNG ou WebP.");
        }

        private static bool ComecaCom(byte[] conteudo, ReadOnlySpan<byte> assinatura)
        {
            return conteudo.AsSpan().StartsWith(assinatura);
        }

        /// <summary>So o nome (sem pasta), cortado no tamanho da coluna; vazio vira um padrao.</summary>
        private static string NomeDoArquivo(string nomeOriginal, TipoArquivo tipo, string contentType)
        {
            var nome = Path.GetFileName(nomeOriginal ?? string.Empty).Trim();
            if (nome.Length == 0)
            {
                nome = tipo == TipoArquivo.Pdf ? "curriculo.pdf" : $"foto.{contentType[(contentType.IndexOf('/') + 1)..]}";
            }

            return nome.Length > 255 ? nome[^255..] : nome;
        }

        private static void ValidarFormacao(DateOnly inicio, DateOnly? fim)
        {
            if (inicio > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de inicio da formacao nao pode estar no futuro.");
            }

            if (fim is not null && fim < inicio)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de fim nao pode ser anterior a data de inicio.");
            }
        }

        private static void ValidarConclusao(DateOnly? conclusao)
        {
            if (conclusao is not null && conclusao > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de conclusao do curso nao pode estar no futuro.");
            }
        }

        private static void ValidarPeriodo(DateOnly inicio, DateOnly? fim)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            if (inicio > hoje)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de inicio nao pode estar no futuro.");
            }

            if (fim is not null && fim > hoje)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de fim nao pode estar no futuro. Para emprego atual, deixe vazia.");
            }

            if (fim is not null && fim < inicio)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "A data de fim nao pode ser anterior a data de inicio.");
            }
        }

        private static void Copiar(ExperienciaCreateDTO dto, LogExperiencia experiencia)
        {
            experiencia.Cargo = dto.Cargo.Trim();
            experiencia.NomeEmpresa = string.IsNullOrWhiteSpace(dto.NomeEmpresa) ? null : dto.NomeEmpresa.Trim();
            experiencia.DataInicio = dto.DataInicio!.Value;
            experiencia.DataFim = dto.DataFim;
        }

        private static void Copiar(FormacaoCreateDTO dto, LogFormacao formacao)
        {
            formacao.Instituicao = dto.Instituicao.Trim();
            formacao.Curso = VazioParaNull(dto.Curso);
            formacao.Nivel = dto.Nivel!.Value;
            formacao.DataInicio = dto.DataInicio!.Value;
            formacao.DataFim = dto.DataFim;
        }

        private static void Copiar(CursoCreateDTO dto, LogCurso curso)
        {
            curso.Nome = dto.Nome.Trim();
            curso.Instituicao = VazioParaNull(dto.Instituicao);
            curso.CargaHoraria = dto.CargaHoraria;
            curso.DataConclusao = dto.DataConclusao;
        }

        private static string? VazioParaNull(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        /// <summary>Os arquivos vem numa consulta a parte, sem o conteudo binario.</summary>
        private async Task<CurriculoDTO> MontarDTOAsync(Curriculo curriculo) => new()
        {
            Id = curriculo.Id,
            IdCandidato = curriculo.IdCandidato,
            NomeCandidato = $"{curriculo.Candidato.Nome} {curriculo.Candidato.Sobrenome}",
            Resumo = curriculo.Resumo,
            Experiencias = curriculo.Experiencias.Select(ToDTO).ToList(),
            Habilidades = curriculo.Habilidades.Select(ToDTO).ToList(),
            Formacoes = curriculo.Formacoes.Select(ToDTO).ToList(),
            Cursos = curriculo.Cursos.Select(ToDTO).ToList(),
            Arquivos = (await _repository.GetArquivosSemConteudoAsync(curriculo.Id)).Select(ToDTO).ToList()
        };

        private static FormacaoDTO ToDTO(LogFormacao formacao) => new()
        {
            Id = formacao.Id,
            Instituicao = formacao.Instituicao,
            Curso = formacao.Curso,
            Nivel = formacao.Nivel,
            DataInicio = formacao.DataInicio,
            DataFim = formacao.DataFim
        };

        private static CursoDTO ToDTO(LogCurso curso) => new()
        {
            Id = curso.Id,
            Nome = curso.Nome,
            Instituicao = curso.Instituicao,
            CargaHoraria = curso.CargaHoraria,
            DataConclusao = curso.DataConclusao
        };

        private static ArquivoDTO ToDTO(CurriculoArquivo arquivo) => new()
        {
            Tipo = arquivo.Tipo,
            Nome = arquivo.Nome,
            ContentType = arquivo.ContentType,
            Tamanho = arquivo.Tamanho,
            EnviadoEm = arquivo.EnviadoEm
        };

        private static ExperienciaDTO ToDTO(LogExperiencia experiencia) => new()
        {
            Id = experiencia.Id,
            Cargo = experiencia.Cargo,
            NomeEmpresa = experiencia.NomeEmpresa,
            DataInicio = experiencia.DataInicio,
            DataFim = experiencia.DataFim
        };

        private static HabilidadeDTO ToDTO(LogHabilidade habilidade) => new()
        {
            Id = habilidade.Id,
            Descricao = habilidade.Descricao
        };
    }
}
