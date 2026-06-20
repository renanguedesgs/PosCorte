using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Data
{
    public class PosCorteDbContext : DbContext
    {
        public PosCorteDbContext(DbContextOptions<PosCorteDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Projeto> Projetos => Set<Projeto>();
        public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CpfCnpj).IsRequired().HasMaxLength(18);
                entity.HasIndex(e => e.CpfCnpj).IsUnique();
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("Arquiteto");
                entity.Property(e => e.Ativo).HasDefaultValue(true);
                entity.Property(e => e.DataCadastro).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Projeto>(entity =>
            {
                entity.ToTable("projetos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.NomeProjeto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UrlArquivoCorteCloud).HasMaxLength(500);
                entity.Property(e => e.CepObra).HasMaxLength(10);
                entity.Property(e => e.EnderecoCompleto).HasMaxLength(500);
                entity.Property(e => e.StatusProjeto).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DataCriacao).HasDefaultValueSql("NOW()");
                entity.HasOne<Usuario>()
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrdemServico>(entity =>
            {
                entity.ToTable("ordens_servico");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.ExternalProviderId).HasMaxLength(100);
                entity.Property(e => e.StatusProvedor).HasMaxLength(50);
                entity.Property(e => e.MontadorNome).HasMaxLength(200);
                entity.Property(e => e.MontadorTelefone).HasMaxLength(20);
                entity.Property(e => e.MontadorFotoUrl).HasMaxLength(500);
                entity.Property(e => e.DataAtualizacao).HasDefaultValueSql("NOW()");
                entity.HasOne<Projeto>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjetoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
