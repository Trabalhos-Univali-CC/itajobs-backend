namespace Itajobs_API.src.Services
{
    /// <summary>
    /// Os tipos de conta. Viram a role do JWT, entao valem em
    /// <c>[Authorize(Roles = Perfis.Empresa)]</c>.
    /// </summary>
    public static class Perfis
    {
        public const string Candidato = "candidato";

        public const string Empresa = "empresa";
    }

    /// <summary>
    /// Quem fez a request, lido do token pelos Controllers. Fica aqui (e nao em
    /// Controllers) para as regras de acesso nao dependerem de HTTP.
    /// </summary>
    public record UsuarioLogado(string Perfil, int Id, string Nome)
    {
        public bool EhCandidato(int idCandidato) => Perfil == Perfis.Candidato && Id == idCandidato;

        public bool EhEmpresa(int idEmpresa) => Perfil == Perfis.Empresa && Id == idEmpresa;

        public bool EhAlgumaEmpresa => Perfil == Perfis.Empresa;

        /// <summary>Barra com 403 quando a regra de acesso nao passa.</summary>
        public static void Exigir(bool permitido)
        {
            if (!permitido)
            {
                throw new RegraNegocioException(TipoErro.Proibido,
                    "Voce nao tem permissao para acessar este recurso.");
            }
        }
    }
}
