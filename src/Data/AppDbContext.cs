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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nomes de tabela e regras de FK iguais ao "tabelas itajobs ver2.sql" do
            // itajobs-banco. Os nomes das colunas estao nos [Column] dos Models.
            modelBuilder.Entity<Contato>().ToTable("contato");

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

                entidade.HasOne(v => v.Empresa).WithMany(e => e.Vagas)
                    .HasForeignKey(v => v.IdEmpresa).OnDelete(DeleteBehavior.Cascade);
            });

            // 1:1 candidato -> curriculo, e 1:* curriculo -> experiencias/habilidades
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
            });

            modelBuilder.Entity<LogExperiencia>().ToTable("logs_experiencias");

            modelBuilder.Entity<LogHabilidade>().ToTable("logs_habilidades");

            // *:* vaga <-> candidato
            modelBuilder.Entity<Candidatura>(entidade =>
            {
                entidade.ToTable("candidatura");
                entidade.HasKey(c => new { c.IdVaga, c.IdCandidato });

                entidade.Property(c => c.DataCandidatura)
                    .HasColumnType("timestamp without time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entidade.HasOne(c => c.Vaga).WithMany(v => v.Candidaturas)
                    .HasForeignKey(c => c.IdVaga).OnDelete(DeleteBehavior.Cascade);
                entidade.HasOne(c => c.Candidato).WithMany(c => c.Candidaturas)
                    .HasForeignKey(c => c.IdCandidato).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
