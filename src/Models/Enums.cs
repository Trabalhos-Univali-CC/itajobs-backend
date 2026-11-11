namespace Itajobs_API.src.Models
{
    // Guardados no banco como texto (ver AppDbContext) e trafegados no JSON em
    // camelCase: "aberta", "emAnalise", "meioPeriodo"...

    /// <summary>Se a vaga ainda aceita candidaturas.</summary>
    public enum StatusVaga
    {
        Aberta,
        Encerrada
    }

    public enum TipoContrato
    {
        Clt,
        Pj,
        Estagio,
        Temporario,
        Aprendiz
    }

    public enum Jornada
    {
        Integral,
        MeioPeriodo,
        Noturno,
        /// <summary>Plantao/escala (12x36, 6x1...), comum no porto e na pesca.</summary>
        Escala
    }

    /// <summary>Andamento da candidatura, controlado pela empresa dona da vaga.</summary>
    public enum StatusCandidatura
    {
        Recebida,
        EmAnalise,
        Aprovada,
        Recusada
    }

    public enum NivelFormacao
    {
        Fundamental,
        Medio,
        Tecnico,
        Superior,
        PosGraduacao
    }

    public enum NivelIdioma
    {
        Basico,
        Intermediario,
        Avancado,
        Fluente,
        Nativo
    }

    /// <summary>Os arquivos que um curriculo pode ter: um de cada.</summary>
    public enum TipoArquivo
    {
        Pdf,
        Foto
    }
}
