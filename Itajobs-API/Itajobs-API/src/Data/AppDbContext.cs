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

        public DbSet<Exemplo> Exemplos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Exemplo>(entidade =>
            {
                entidade.ToTable("exemplo");
                entidade.HasKey(e => e.Id);
                entidade.HasIndex(e => e.Nome);
            });
        }
    }
}
