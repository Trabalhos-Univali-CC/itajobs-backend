using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Regras de vaga.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A vaga pertence a uma empresa que precisa existir.</item>
    /// <item>A vaga nao troca de empresa depois de criada.</item>
    /// <item>Remuneracao: "a combinar" (nada), salario fixo, ou faixa (minimo e maximo, maximo >= minimo). Nunca fixo e faixa juntos.</item>
    /// <item>Palavras-chave: ate 20, sem espacos nas pontas; repetidas (sem diferenciar maiuscula) entram uma vez.</item>
    /// <item>Sem bairro informado, a vaga fica no bairro da empresa.</item>
    /// <item>Nasce aberta; so a rota de status encerra ou reabre. Encerrada nao aceita candidatura.</item>
    /// <item>A busca, sem status informado, mostra so abertas, menos quando filtra por empresa.</item>
    /// <item>Apagar a vaga apaga as candidaturas dela.</item>
    /// </list>
    /// </remarks>
    public class VagaService
    {
        private readonly IVagaRepository _repository;
        private readonly IEmpresaRepository _empresaRepository;

        public VagaService(IVagaRepository repository, IEmpresaRepository empresaRepository)
        {
            _repository = repository;
            _empresaRepository = empresaRepository;
        }

        public async Task<PaginaDTO<VagaDTO>> BuscarAsync(VagaFiltroDTO filtro)
        {
            if (filtro.SalarioMinimo > filtro.SalarioMaximo)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "SalarioMinimo nao pode ser maior que SalarioMaximo.");
            }

            var status = filtro.Status ?? (filtro.IdEmpresa is null ? StatusVaga.Aberta : null);

            var (itens, total) = await _repository.GetPaginaAsync(new VagaFiltro(
                filtro.IdEmpresa,
                filtro.Cargo?.Trim(),
                filtro.Bairro?.Trim(),
                filtro.SalarioMinimo,
                filtro.SalarioMaximo,
                filtro.TipoContrato,
                filtro.Jornada,
                filtro.Modalidade,
                filtro.PalavraChave?.Trim(),
                status,
                filtro.Pagina,
                filtro.TamanhoPagina));

            return new PaginaDTO<VagaDTO>
            {
                Itens = itens.Select(ToDTO).ToList(),
                Pagina = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina,
                Total = total
            };
        }

        public async Task<VagaDTO?> GetByIdAsync(int id)
        {
            var vaga = await _repository.GetByIdAsync(id);
            return vaga is null ? null : ToDTO(vaga);
        }

        public async Task<VagaDTO> CreateAsync(VagaCreateDTO dto)
        {
            ValidarRemuneracao(dto);
            var palavras = NormalizarPalavrasChave(dto.PalavrasChave);

            var idEmpresa = dto.IdEmpresa!.Value;
            var empresa = await _empresaRepository.GetByIdAsync(idEmpresa)
                ?? throw new RegraNegocioException(TipoErro.NaoEncontrado, $"Empresa {idEmpresa} nao encontrada.");

            var vaga = new Vaga { IdEmpresa = idEmpresa, Status = StatusVaga.Aberta };
            Copiar(dto, vaga, bairroPadrao: empresa.Endereco.Bairro);
            AplicarPalavrasChave(vaga, palavras);

            var criada = await _repository.CreateAsync(vaga);
            return ToDTO(criada);
        }

        public async Task<VagaDTO?> UpdateAsync(int id, VagaCreateDTO dto)
        {
            ValidarRemuneracao(dto);
            var palavras = NormalizarPalavrasChave(dto.PalavrasChave);

            var vaga = await _repository.GetByIdAsync(id);
            if (vaga is null)
            {
                return null;
            }

            if (dto.IdEmpresa != vaga.IdEmpresa)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "Uma vaga nao pode ser transferida para outra empresa.");
            }

            var empresa = await _empresaRepository.GetByIdAsync(vaga.IdEmpresa);
            Copiar(dto, vaga, bairroPadrao: empresa?.Endereco.Bairro);
            AplicarPalavrasChave(vaga, palavras);

            await _repository.UpdateAsync(vaga);
            return ToDTO(vaga);
        }

        /// <summary>Encerra ou reabre. <c>null</c> = vaga nao encontrada.</summary>
        public async Task<VagaDTO?> AlterarStatusAsync(int id, StatusVaga status)
        {
            var vaga = await _repository.GetByIdAsync(id);
            if (vaga is null)
            {
                return null;
            }

            vaga.Status = status;
            await _repository.UpdateAsync(vaga);
            return ToDTO(vaga);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vaga = await _repository.GetByIdAsync(id);
            if (vaga is null)
            {
                return false;
            }

            await _repository.DeleteAsync(vaga);
            return true;
        }

        /// <summary>
        /// As tres formas aceitas (as mesmas do CHECK <c>ck_vaga_remuneracao</c> do banco),
        /// com mensagem clara em vez do erro do Postgres.
        /// </summary>
        private static void ValidarRemuneracao(VagaCreateDTO dto)
        {
            var fixo = dto.Salario is not null;
            var faixa = dto.SalarioMinimo is not null || dto.SalarioMaximo is not null;

            if (fixo && faixa)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "Informe salario fixo (salario) ou faixa (salarioMinimo e salarioMaximo), nao os dois.");
            }

            if (dto.Salario is <= 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "O salario deve ser maior que zero.");
            }

            if (!faixa)
            {
                return;
            }

            if (dto.SalarioMinimo is null || dto.SalarioMaximo is null)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "Na faixa salarial, informe salarioMinimo e salarioMaximo.");
            }

            if (dto.SalarioMinimo <= 0)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "O salarioMinimo deve ser maior que zero.");
            }

            if (dto.SalarioMaximo < dto.SalarioMinimo)
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "O salarioMaximo nao pode ser menor que o salarioMinimo.");
            }
        }

        /// <summary>Tira espacos, descarta vazias e repetidas (sem diferenciar maiuscula).</summary>
        private static List<string> NormalizarPalavrasChave(List<string>? palavras)
        {
            var resultado = new List<string>();
            foreach (var palavra in palavras ?? [])
            {
                var limpa = palavra?.Trim() ?? string.Empty;
                if (limpa.Length == 0)
                {
                    continue;
                }

                if (limpa.Length > 100)
                {
                    throw new RegraNegocioException(TipoErro.Invalido,
                        $"A palavra-chave \"{limpa[..20]}...\" passa de 100 caracteres.");
                }

                if (!resultado.Any(p => p.Equals(limpa, StringComparison.OrdinalIgnoreCase)))
                {
                    resultado.Add(limpa);
                }
            }

            return resultado;
        }

        /// <summary>
        /// Deixa a vaga com exatamente essas palavras. Mexe so na diferenca: as que ja
        /// existem ficam (com a grafia nova), as que sairam sao apagadas e as novas
        /// entram. Assim o indice unico do banco nunca ve a mesma palavra duas vezes.
        /// </summary>
        private static void AplicarPalavrasChave(Vaga vaga, List<string> palavras)
        {
            vaga.PalavrasChave.RemoveAll(existente =>
                !palavras.Any(p => p.Equals(existente.Descricao, StringComparison.OrdinalIgnoreCase)));

            foreach (var palavra in palavras)
            {
                var existente = vaga.PalavrasChave.FirstOrDefault(p =>
                    p.Descricao.Equals(palavra, StringComparison.OrdinalIgnoreCase));

                if (existente is null)
                {
                    vaga.PalavrasChave.Add(new VagaPalavraChave { Descricao = palavra });
                }
                else
                {
                    existente.Descricao = palavra;
                }
            }
        }

        private static void Copiar(VagaCreateDTO dto, Vaga vaga, string? bairroPadrao)
        {
            vaga.Cargo = dto.Cargo.Trim();
            vaga.Descricao = dto.Descricao.Trim();
            vaga.Salario = dto.Salario;
            vaga.SalarioMinimo = dto.SalarioMinimo;
            vaga.SalarioMaximo = dto.SalarioMaximo;
            vaga.Modalidade = dto.Modalidade;
            vaga.TipoContrato = dto.TipoContrato!.Value;
            vaga.Jornada = dto.Jornada!.Value;
            vaga.Bairro = VazioParaNull(dto.Bairro) ?? bairroPadrao;
            vaga.Requisitos = VazioParaNull(dto.Requisitos);
        }

        private static string? VazioParaNull(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static VagaDTO ToDTO(Vaga vaga) => new()
        {
            Id = vaga.Id,
            IdEmpresa = vaga.IdEmpresa,
            NomeEmpresa = vaga.Empresa.Nome,
            Cargo = vaga.Cargo,
            Descricao = vaga.Descricao,
            Salario = vaga.Salario,
            SalarioMinimo = vaga.SalarioMinimo,
            SalarioMaximo = vaga.SalarioMaximo,
            Modalidade = vaga.Modalidade,
            PalavrasChave = vaga.PalavrasChave.Select(p => p.Descricao).ToList(),
            TipoContrato = vaga.TipoContrato,
            Jornada = vaga.Jornada,
            Bairro = vaga.Bairro,
            Requisitos = vaga.Requisitos,
            Status = vaga.Status,
            DataPublicacao = vaga.DataPublicacao
        };
    }
}
