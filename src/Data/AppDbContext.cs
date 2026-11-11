using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Models;

namespace Itajobs_API.src.Data
{
    /// <summary>
    /// Contexto do EF Core: a ponte entre as classes de <c>Models</c> e as tabelas do
    /// banco. Cada <c>DbSet</c> declarado aqui vira uma tabela.
    /// </summary>
    /// <remarks>
    /// Registrado no <c>Program.cs</c> com <c>AddDbContext</c>. Depois de mexer num
    /// Model ou adicionar um DbSet, gere a migration:
    /// <code>dotnet ef migrations add NomeDaMudanca</code>
    /// </remarks>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Contato> Contatos { get; set; }

        public DbSet<Endereco> Enderecos { get; set; }

        public DbSet<UsuarioEmpresa> Empresas { get; set; }

        public DbSet<UsuarioCandidato> Candidatos { get; set; }

        public DbSet<Vaga> Vagas { get; set; }

        public DbSet<Curriculo> Curriculos { get; set; }

        public DbSet<LogExperiencia> Experiencias { get; set; }

        public DbSet<LogHabilidade> Habilidades { get; set; }

        public DbSet<Candidatura> Candidaturas { get; set; }

        public DbSet<LogFormacao> Formacoes { get; set; }

        public DbSet<LogCurso> Cursos { get; set; }

        public DbSet<LogProjeto> Projetos { get; set; }

        public DbSet<LogIdioma> Idiomas { get; set; }

        public DbSet<CurriculoArquivo> CurriculoArquivos { get; set; }

        public DbSet<AvaliacaoEmpresa> Avaliacoes { get; set; }

        /// <summary>Enums viram texto no banco ("Aberta", "EmAnalise"...), nao numero.</summary>
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<StatusVaga>().HaveConversion<string>().HaveMaxLength(20);
            configurationBuilder.Properties<TipoContrato>().HaveConversion<string>().HaveMaxLength(20);
            configurationBuilder.Properties<Jornada>().HaveConversion<string>().HaveMaxLength(20);
            configurationBuilder.Properties<StatusCandidatura>().HaveConversion<string>().HaveMaxLength(20);
            configurationBuilder.Properties<NivelFormacao>().HaveConversion<string>().HaveMaxLength(20);
            configurationBuilder.Properties<NivelIdioma>().HaveConversion<string>().HaveMaxLength(20);
            configurationBuilder.Properties<TipoArquivo>().HaveConversion<string>().HaveMaxLength(10);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nomes de tabela e regras de FK iguais ao "tabelas itajobs ver2.sql" do
            // itajobs-banco. Os nomes das colunas estao nos [Column] dos Models.
            // O login e o cadastro procuram o usuario pelo e-mail. Sem indice, cada
            // busca le a tabela inteira. Nao e unico: candidato e empresa podem repetir.
            modelBuilder.Entity<Contato>(entidade =>
            {
                entidade.ToTable("contato");
                entidade.HasIndex(c => c.Email);
            });

            modelBuilder.Entity<Endereco>().ToTable("endereco");

            modelBuilder.Entity<UsuarioEmpresa>(entidade =>
            {
                entidade.ToTable("usuario_empresa");
                entidade.HasIndex(e => e.Cnpj).IsUnique();

                entidade.HasOne(e => e.Endereco).WithMany()
                    .HasForeignKey(e => e.IdEndereco).OnDelete(DeleteBehavior.Restrict);
                entidade.HasOne(e => e.Contato).WithMany()
                    .HasForeignKey(e => e.IdContato).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UsuarioCandidato>(entidade =>
            {
                entidade.ToTable("usuario_candidato");
                entidade.HasIndex(c => c.Cpf).IsUnique();

                entidade.HasOne(c => c.Endereco).WithMany()
                    .HasForeignKey(c => c.IdEndereco).OnDelete(DeleteBehavior.Restrict);
                entidade.HasOne(c => c.Contato).WithMany()
                    .HasForeignKey(c => c.IdContato).OnDelete(DeleteBehavior.Restrict);
            });

            // 1:* empresa -> vaga
            modelBuilder.Entity<Vaga>(entidade =>
            {
                entidade.ToTable("vaga");

                // A busca publica filtra por status e ordena pela data.
                entidade.HasIndex(v => new { v.Status, v.DataPublicacao });

                entidade.Property(v => v.DataPublicacao)
                    .HasColumnType("timestamp without time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entidade.HasOne(v => v.Empresa).WithMany(e => e.Vagas)
                    .HasForeignKey(v => v.IdEmpresa).OnDelete(DeleteBehavior.Cascade);
            });

            // 1:1 candidato -> curriculo, e 1:* curriculo -> itens e arquivos
            modelBuilder.Entity<Curriculo>(entidade =>
            {
                entidade.ToTable("curriculo");
                entidade.HasIndex(c => c.IdCandidato).IsUnique();

                entidade.HasOne(c => c.Candidato).WithOne(c => c.Curriculo)
                    .HasForeignKey<Curriculo>(c => c.IdCandidato).OnDelete(DeleteBehavior.Cascade);
                entidade.HasMany(c => c.Experiencias).WithOne()
                    .HasForeignKey(e => e.IdCurriculo).OnDelete(DeleteBehavior.Cascade);
                entidade.HasMany(c => c.Habilidades).WithOne()
                    .HasForeignKey(h => h.IdCurriculo).OnDelete(DeleteBehavior.Cascade);
                entidade.HasMany(c => c.Formacoes).WithOne()
                    .HasForeignKey(f => f.IdCurriculo).OnDelete(DeleteBehavior.Cascade);
                entidade.HasMany(c => c.Cursos).WithOne()
                    .HasForeignKey(c => c.IdCurriculo).OnDelete(DeleteBehavior.Cascade);
                entidade.HasMany(c => c.Projetos).WithOne()
                    .HasForeignKey(p => p.IdCurriculo).OnDelete(DeleteBehavior.Cascade);
                entidade.HasMany(c => c.Idiomas).WithOne()
                    .HasForeignKey(i => i.IdCurriculo).OnDelete(DeleteBehavior.Cascade);
                entidade.HasMany(c => c.Arquivos).WithOne()
                    .HasForeignKey(a => a.IdCurriculo).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LogExperiencia>().ToTable("logs_experiencias");

            modelBuilder.Entity<LogHabilidade>().ToTable("logs_habilidades");

            modelBuilder.Entity<LogFormacao>().ToTable("logs_formacoes");

            modelBuilder.Entity<LogCurso>().ToTable("logs_cursos");

            modelBuilder.Entity<LogProjeto>().ToTable("logs_projetos");

            modelBuilder.Entity<LogIdioma>().ToTable("logs_idiomas");

            modelBuilder.Entity<CurriculoArquivo>(entidade =>
            {
                entidade.ToTable("curriculo_arquivo");
                entidade.HasKey(a => new { a.IdCurriculo, a.Tipo });
                entidade.Property(a => a.EnviadoEm).HasColumnType("timestamp without time zone");
            });

            // *:* empresa <-> candidato (a nota que o candidato deu)
            modelBuilder.Entity<AvaliacaoEmpresa>(entidade =>
            {
                entidade.ToTable("avaliacao_empresa", t => t.HasCheckConstraint("ck_avaliacao_nota", "nota BETWEEN 1 AND 5"));
                entidade.HasKey(a => new { a.IdEmpresa, a.IdCandidato });

                entidade.Property(a => a.Data)
                    .HasColumnType("timestamp without time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entidade.HasOne(a => a.Empresa).WithMany(e => e.Avaliacoes)
                    .HasForeignKey(a => a.IdEmpresa).OnDelete(DeleteBehavior.Cascade);
                entidade.HasOne(a => a.Candidato).WithMany()
                    .HasForeignKey(a => a.IdCandidato).OnDelete(DeleteBehavior.Cascade);
            });

            // *:* vaga <-> candidato
            modelBuilder.Entity<Candidatura>(entidade =>
            {
                entidade.ToTable("candidatura");
                entidade.HasKey(c => new { c.IdVaga, c.IdCandidato });

                entidade.Property(c => c.DataCandidatura)
                    .HasColumnType("timestamp without time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entidade.Property(c => c.DataStatus).HasColumnType("timestamp without time zone");

                entidade.HasOne(c => c.Vaga).WithMany(v => v.Candidaturas)
                    .HasForeignKey(c => c.IdVaga).OnDelete(DeleteBehavior.Cascade);
                entidade.HasOne(c => c.Candidato).WithMany(c => c.Candidaturas)
                    .HasForeignKey(c => c.IdCandidato).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
