using System.Security.Cryptography;
using System.Text;
using Itajobs_API.src.DTO;
using Itajobs_API.src.Models;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>Configuracao da recuperacao de senha, lida do itajobs.env.</summary>
    /// <param name="UrlRedefinicao">Pagina do front que recebe <c>?token=</c>.</param>
    public record RecuperacaoSenhaOptions(string UrlRedefinicao, TimeSpan Validade);

    /// <summary>
    /// Login (senha ou Google), troca e recuperacao de senha.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>E-mail inexistente e senha errada dao a mesma mensagem, para o login nao revelar quais e-mails tem conta.</item>
    /// <item>O e-mail e unico entre todas as contas, entao ele sozinho acha a conta.</item>
    /// <item>"Esqueci a senha" responde igual exista ou nao a conta. O link vale 1 vez, por
    /// <see cref="RecuperacaoSenhaOptions.Validade"/>; pedir de novo invalida o anterior.
    /// O banco guarda so o SHA-256 do token.</item>
    /// <item>Google: so entra quem ja tem conta. A conta e achada pelo vinculo (sub do Google)
    /// ou, no primeiro acesso, pelo e-mail verificado do Google, que entao fica vinculado.</item>
    /// </list>
    /// </remarks>
    public class AuthService
    {
        private readonly ICandidatoRepository _candidatos;
        private readonly IEmpresaRepository _empresas;
        private readonly IAuthRepository _repository;
        private readonly TokenService _tokens;
        private readonly GoogleTokenService _google;
        private readonly IEmailService _email;
        private readonly RecuperacaoSenhaOptions _recuperacao;

        public AuthService(
            ICandidatoRepository candidatos,
            IEmpresaRepository empresas,
            IAuthRepository repository,
            TokenService tokens,
            GoogleTokenService google,
            IEmailService email,
            RecuperacaoSenhaOptions recuperacao)
        {
            _candidatos = candidatos;
            _empresas = empresas;
            _repository = repository;
            _tokens = tokens;
            _google = google;
            _email = email;
            _recuperacao = recuperacao;
        }

        public async Task<TokenDTO> LoginAsync(LoginDTO dto)
        {
            var conta = await AcharContaPorEmailAsync(dto.Email);

            var valida = conta is not null
                && (dto.Tipo is null || dto.Tipo == conta.Usuario.Perfil)
                && SenhaHasher.Confere(conta.SenhaHash, dto.Senha);

            if (!valida)
            {
                throw new RegraNegocioException(TipoErro.NaoAutenticado, "E-mail ou senha incorretos.");
            }

            return GerarToken(conta!.Usuario);
        }

        /// <summary>Confere a senha atual antes de trocar.</summary>
        public async Task TrocarSenhaAsync(UsuarioLogado usuario, TrocarSenhaDTO dto)
        {
            if (dto.NovaSenha == dto.SenhaAtual)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "A nova senha precisa ser diferente da atual.");
            }

            await AlterarSenhaAsync(usuario, hashAtual =>
            {
                // 400 e nao 401: o usuario continua logado, so errou a senha atual.
                if (!SenhaHasher.Confere(hashAtual, dto.SenhaAtual))
                {
                    throw new RegraNegocioException(TipoErro.Invalido, "Senha atual incorreta.");
                }
            }, dto.NovaSenha);
        }

        /// <summary>
        /// Gera o link de troca de senha e manda por e-mail. Nao diz se o e-mail tem
        /// conta: o Controller responde a mesma coisa nos dois casos.
        /// </summary>
        public async Task SolicitarRecuperacaoAsync(EsqueciSenhaDTO dto)
        {
            var conta = await AcharContaPorEmailAsync(dto.Email);
            if (conta is null)
            {
                return;
            }

            var (idCandidato, idEmpresa) = Ids(conta.Usuario);
            await _repository.ApagarRecuperacoesPendentesAsync(idCandidato, idEmpresa);

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var agora = DateTime.UtcNow;

            await _repository.AddRecuperacaoAsync(new RecuperacaoSenha
            {
                IdCandidato = idCandidato,
                IdEmpresa = idEmpresa,
                TokenHash = Hash(token),
                CriadoEm = agora,
                ExpiraEm = agora.Add(_recuperacao.Validade)
            });

            var link = $"{_recuperacao.UrlRedefinicao}?token={token}";
            var minutos = (int)_recuperacao.Validade.TotalMinutes;

            // Sem await: o tempo de resposta nao denuncia se o e-mail tem conta.
            _ = Task.Run(() => _email.EnviarAsync(conta.Email, "ItaJobs - redefinir sua senha",
                $"Ola, {conta.Usuario.Nome}.\n\n"
                + $"Recebemos um pedido para redefinir a senha da sua conta no ItaJobs. "
                + $"Para criar uma nova senha, acesse o link abaixo (vale por {minutos} minutos e so pode ser usado uma vez):\n\n"
                + $"{link}\n\n"
                + "Se nao foi voce, ignore este e-mail: sua senha continua a mesma."));
        }

        /// <summary>Troca a senha com o token do e-mail. 400 se o link nao vale mais.</summary>
        public async Task RedefinirSenhaAsync(RedefinirSenhaDTO dto)
        {
            var recuperacao = await _repository.GetRecuperacaoPorHashAsync(Hash(dto.Token.Trim()));

            // ConsumirAsync marca como usado so se ainda nao foi e nao expirou, num UPDATE
            // so: dois envios simultaneos do mesmo link nao passam os dois.
            if (recuperacao is null || !await _repository.ConsumirRecuperacaoAsync(recuperacao.Id))
            {
                throw new RegraNegocioException(TipoErro.Invalido,
                    "Link de recuperacao invalido ou expirado. Peca um novo em \"Esqueci minha senha\".");
            }

            var usuario = recuperacao.IdCandidato is { } idCandidato
                ? new UsuarioLogado(Perfis.Candidato, idCandidato, "")
                : new UsuarioLogado(Perfis.Empresa, recuperacao.IdEmpresa!.Value, "");

            await AlterarSenhaAsync(usuario, _ => { }, dto.NovaSenha);
            await _repository.ApagarRecuperacoesPendentesAsync(recuperacao.IdCandidato, recuperacao.IdEmpresa);
        }

        /// <summary>Login com o ID token do Google.</summary>
        public async Task<TokenDTO> LoginGoogleAsync(GoogleLoginDTO dto)
        {
            var google = await _google.ValidarAsync(dto.IdToken);

            var identidade = await _repository.GetIdentidadeAsync(IdentidadeExterna.Google, google.Sub);
            if (identidade is not null)
            {
                var usuario = await UsuarioDaIdentidadeAsync(identidade)
                    ?? throw new RegraNegocioException(TipoErro.NaoAutenticado, "A conta vinculada a este Google nao existe mais.");
                return GerarToken(usuario);
            }

            // Primeiro acesso com Google: liga pelo e-mail, se o Google garante que e dele.
            if (google.EmailVerificado && google.Email is not null)
            {
                var conta = await AcharContaPorEmailAsync(google.Email);
                if (conta is not null)
                {
                    await VincularAsync(conta.Usuario, google.Sub);
                    return GerarToken(conta.Usuario);
                }
            }

            throw new RegraNegocioException(TipoErro.NaoEncontrado,
                "Nenhuma conta do ItaJobs com este Google. Cadastre-se primeiro e depois vincule o Google ao seu perfil.");
        }

        /// <summary>Liga uma conta Google a conta logada (mesmo com e-mail diferente).</summary>
        public async Task VincularGoogleAsync(UsuarioLogado usuario, GoogleLoginDTO dto)
        {
            var google = await _google.ValidarAsync(dto.IdToken);
            var (idCandidato, idEmpresa) = Ids(usuario);

            var existente = await _repository.GetIdentidadeAsync(IdentidadeExterna.Google, google.Sub);
            if (existente is not null)
            {
                if (existente.IdCandidato == idCandidato && existente.IdEmpresa == idEmpresa)
                {
                    return;
                }

                throw new RegraNegocioException(TipoErro.Conflito, "Esta conta Google ja esta vinculada a outra conta do ItaJobs.");
            }

            if (await _repository.GetIdentidadeDaContaAsync(IdentidadeExterna.Google, idCandidato, idEmpresa) is not null)
            {
                throw new RegraNegocioException(TipoErro.Conflito,
                    "Sua conta ja esta vinculada a outro Google. Desvincule antes de vincular outro.");
            }

            await VincularAsync(usuario, google.Sub);
        }

        /// <summary><c>false</c> se a conta nao tinha Google vinculado.</summary>
        public Task<bool> DesvincularGoogleAsync(UsuarioLogado usuario)
        {
            var (idCandidato, idEmpresa) = Ids(usuario);
            return _repository.ApagarIdentidadeDaContaAsync(IdentidadeExterna.Google, idCandidato, idEmpresa);
        }

        public static UsuarioLogadoDTO ToDTO(UsuarioLogado usuario) => new()
        {
            Tipo = usuario.Perfil,
            Id = usuario.Id,
            Nome = usuario.Nome
        };

        /// <summary>Uma conta achada pelo e-mail, de candidato ou de empresa.</summary>
        private record Conta(UsuarioLogado Usuario, string SenhaHash, string Email);

        private async Task<Conta?> AcharContaPorEmailAsync(string emailInformado)
        {
            var email = EnderecoContatoMapper.NormalizarEmail(emailInformado);

            var candidato = await _candidatos.GetByEmailAsync(email);
            if (candidato is not null)
            {
                return new Conta(new UsuarioLogado(Perfis.Candidato, candidato.Id, candidato.Nome), candidato.SenhaHash, email);
            }

            var empresa = await _empresas.GetByEmailAsync(email);
            return empresa is null
                ? null
                : new Conta(new UsuarioLogado(Perfis.Empresa, empresa.Id, empresa.Nome), empresa.SenhaHash, email);
        }

        private async Task<UsuarioLogado?> UsuarioDaIdentidadeAsync(IdentidadeExterna identidade)
        {
            if (identidade.IdCandidato is { } idCandidato)
            {
                var candidato = await _candidatos.GetByIdAsync(idCandidato);
                return candidato is null ? null : new UsuarioLogado(Perfis.Candidato, candidato.Id, candidato.Nome);
            }

            var empresa = await _empresas.GetByIdAsync(identidade.IdEmpresa!.Value);
            return empresa is null ? null : new UsuarioLogado(Perfis.Empresa, empresa.Id, empresa.Nome);
        }

        private Task VincularAsync(UsuarioLogado usuario, string sub)
        {
            var (idCandidato, idEmpresa) = Ids(usuario);
            return _repository.AddIdentidadeAsync(new IdentidadeExterna
            {
                IdCandidato = idCandidato,
                IdEmpresa = idEmpresa,
                Provedor = IdentidadeExterna.Google,
                IdentificadorExterno = sub,
                CriadoEm = DateTime.UtcNow
            });
        }

        /// <summary>Le a conta, roda a conferencia e grava o hash da senha nova.</summary>
        private async Task AlterarSenhaAsync(UsuarioLogado usuario, Action<string> conferir, string novaSenha)
        {
            if (usuario.Perfil == Perfis.Candidato)
            {
                var candidato = await _candidatos.GetByIdAsync(usuario.Id) ?? throw ContaNaoEncontrada();
                conferir(candidato.SenhaHash);
                candidato.SenhaHash = SenhaHasher.Gerar(novaSenha);
                await _candidatos.UpdateAsync(candidato);
            }
            else
            {
                var empresa = await _empresas.GetByIdAsync(usuario.Id) ?? throw ContaNaoEncontrada();
                conferir(empresa.SenhaHash);
                empresa.SenhaHash = SenhaHasher.Gerar(novaSenha);
                await _empresas.UpdateAsync(empresa);
            }
        }

        private TokenDTO GerarToken(UsuarioLogado usuario)
        {
            var (token, expiraEm) = _tokens.Gerar(usuario);
            return new TokenDTO { Token = token, ExpiraEm = expiraEm, Usuario = ToDTO(usuario) };
        }

        private static (int? IdCandidato, int? IdEmpresa) Ids(UsuarioLogado usuario) =>
            usuario.Perfil == Perfis.Candidato ? (usuario.Id, null) : (null, usuario.Id);

        /// <summary>SHA-256 em hexadecimal minusculo (64 caracteres), como na coluna <c>token_hash</c>.</summary>
        private static string Hash(string token) =>
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        // Token valido de uma conta que foi apagada depois do login.
        private static RegraNegocioException ContaNaoEncontrada() =>
            new(TipoErro.NaoAutenticado, "Sua conta nao existe mais. Faca login de novo.");
    }
}
