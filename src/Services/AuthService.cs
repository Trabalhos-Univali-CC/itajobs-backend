using Itajobs_API.src.DTO;
using Itajobs_API.src.Repositories;

namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Login: confere e-mail e senha do tipo de conta escolhido e devolve o JWT.
    /// </summary>
    /// <remarks>
    /// E-mail inexistente e senha errada dao a mesma mensagem, para o login nao
    /// revelar quais e-mails tem conta.
    /// </remarks>
    public class AuthService
    {
        private readonly ICandidatoRepository _candidatos;
        private readonly IEmpresaRepository _empresas;
        private readonly TokenService _tokens;

        public AuthService(ICandidatoRepository candidatos, IEmpresaRepository empresas, TokenService tokens)
        {
            _candidatos = candidatos;
            _empresas = empresas;
            _tokens = tokens;
        }

        public async Task<TokenDTO> LoginAsync(LoginDTO dto)
        {
            var email = EnderecoContatoMapper.NormalizarEmail(dto.Email);

            UsuarioLogado? usuario = null;
            if (dto.Tipo == Perfis.Candidato)
            {
                var candidato = await _candidatos.GetByEmailAsync(email);
                if (candidato is not null && SenhaHasher.Confere(candidato.SenhaHash, dto.Senha))
                {
                    usuario = new UsuarioLogado(Perfis.Candidato, candidato.Id, candidato.Nome);
                }
            }
            else
            {
                var empresa = await _empresas.GetByEmailAsync(email);
                if (empresa is not null && SenhaHasher.Confere(empresa.SenhaHash, dto.Senha))
                {
                    usuario = new UsuarioLogado(Perfis.Empresa, empresa.Id, empresa.Nome);
                }
            }

            if (usuario is null)
            {
                throw new RegraNegocioException(TipoErro.NaoAutenticado, "E-mail ou senha incorretos.");
            }

            var (token, expiraEm) = _tokens.Gerar(usuario);
            return new TokenDTO { Token = token, ExpiraEm = expiraEm, Usuario = ToDTO(usuario) };
        }

        /// <summary>Confere a senha atual antes de trocar.</summary>
        public async Task TrocarSenhaAsync(UsuarioLogado usuario, TrocarSenhaDTO dto)
        {
            if (dto.NovaSenha == dto.SenhaAtual)
            {
                throw new RegraNegocioException(TipoErro.Invalido, "A nova senha precisa ser diferente da atual.");
            }

            if (usuario.Perfil == Perfis.Candidato)
            {
                var candidato = await _candidatos.GetByIdAsync(usuario.Id) ?? throw ContaNaoEncontrada();
                ConferirSenhaAtual(candidato.SenhaHash, dto.SenhaAtual);
                candidato.SenhaHash = SenhaHasher.Gerar(dto.NovaSenha);
                await _candidatos.UpdateAsync(candidato);
            }
            else
            {
                var empresa = await _empresas.GetByIdAsync(usuario.Id) ?? throw ContaNaoEncontrada();
                ConferirSenhaAtual(empresa.SenhaHash, dto.SenhaAtual);
                empresa.SenhaHash = SenhaHasher.Gerar(dto.NovaSenha);
                await _empresas.UpdateAsync(empresa);
            }
        }

        // 400 e nao 401: o usuario continua logado, so errou a senha atual.
        private static void ConferirSenhaAtual(string hash, string senhaAtual)
        {
            if (!SenhaHasher.Confere(hash, senhaAtual))
            {
                throw new RegraNegocioException(TipoErro.Invalido, "Senha atual incorreta.");
            }
        }

        // Token valido de uma conta que foi apagada depois do login.
        private static RegraNegocioException ContaNaoEncontrada() =>
            new(TipoErro.NaoAutenticado, "Sua conta nao existe mais. Faca login de novo.");

        public static UsuarioLogadoDTO ToDTO(UsuarioLogado usuario) => new()
        {
            Tipo = usuario.Perfil,
            Id = usuario.Id,
            Nome = usuario.Nome
        };
    }
}
