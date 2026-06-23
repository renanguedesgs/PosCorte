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
        public DbSet<Marceneiro> Marceneiros => Set<Marceneiro>();
        public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
        public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
        public DbSet<Liquidacao> Liquidacoes => Set<Liquidacao>();

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
                entity.Property(e => e.MotivoDisputa).HasMaxLength(1000);
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

            modelBuilder.Entity<Marceneiro>(entity =>
            {
                entity.ToTable("marceneiros");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.FotoUrl).HasMaxLength(500);
                entity.Property(e => e.Cidade).HasMaxLength(120);
                entity.Property(e => e.Estado).HasMaxLength(60);
                entity.Property(e => e.Bairro).HasMaxLength(120);
                entity.Property(e => e.Cep).HasMaxLength(10);
                entity.Property(e => e.Especialidades).HasMaxLength(300);
                entity.Property(e => e.Bio).HasMaxLength(600);
                entity.Property(e => e.NotaMedia).HasColumnType("numeric(3,2)");
                entity.Property(e => e.OrigemExterna).HasMaxLength(100);
                entity.HasIndex(e => e.OrigemExterna);
                entity.Property(e => e.DataCadastro).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Avaliacao>(entity =>
            {
                entity.ToTable("avaliacoes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.AutorNome).HasMaxLength(200);
                entity.Property(e => e.Comentario).HasMaxLength(1000);
                entity.Property(e => e.DataCriacao).HasDefaultValueSql("NOW()");
                entity.HasIndex(e => e.MarceneiroId);
                entity.HasOne<Marceneiro>()
                      .WithMany()
                      .HasForeignKey(e => e.MarceneiroId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.ToTable("pagamentos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.Modo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AsaasPaymentId).HasMaxLength(100);
                entity.HasIndex(e => e.AsaasPaymentId);
                entity.Property(e => e.AsaasCustomerId).HasMaxLength(100);
                entity.Property(e => e.ValorTotal).HasColumnType("numeric(12,2)");
                entity.Property(e => e.ValorMarceneiro).HasColumnType("numeric(12,2)");
                entity.Property(e => e.ValorPlataforma).HasColumnType("numeric(12,2)");
                entity.Property(e => e.PixCopiaECola).HasMaxLength(2000);
                entity.Property(e => e.InvoiceUrl).HasMaxLength(500);
                entity.Property(e => e.DataCriacao).HasDefaultValueSql("NOW()");
                entity.HasOne<Projeto>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjetoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Liquidacao>(entity =>
            {
                entity.ToTable("liquidacoes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ValorMarceneiro).HasColumnType("numeric(12,2)");
                entity.Property(e => e.ValorPlataforma).HasColumnType("numeric(12,2)");
                entity.Property(e => e.AsaasSplitId).HasMaxLength(100);
                entity.Property(e => e.DataCriacao).HasDefaultValueSql("NOW()");
                entity.HasOne<Pagamento>()
                      .WithMany()
                      .HasForeignKey(e => e.PagamentoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
