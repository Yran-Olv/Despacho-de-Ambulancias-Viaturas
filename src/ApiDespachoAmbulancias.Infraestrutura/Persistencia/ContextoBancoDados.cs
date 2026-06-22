using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using Microsoft.EntityFrameworkCore;

namespace ApiDespachoAmbulancias.Infraestrutura.Persistencia;

public class ContextoBancoDados : DbContext
{
    public ContextoBancoDados(DbContextOptions<ContextoBancoDados> options) : base(options) { }

    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Veiculo>(entity =>
        {
            entity.ToTable("veiculos");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Id).HasColumnName("id");
            entity.Property(v => v.Identificacao).HasColumnName("identificacao").HasMaxLength(20).IsRequired();
            entity.Property(v => v.Tipo).HasColumnName("tipo").HasConversion<string>().IsRequired();
            entity.Property(v => v.Status).HasColumnName("status").HasConversion<string>().IsRequired();
            entity.Property(v => v.DataCriacao).HasColumnName("data_criacao").IsRequired();
            entity.Property(v => v.DataAtualizacao).HasColumnName("data_atualizacao");

            entity.HasIndex(v => v.Identificacao).IsUnique();
            entity.HasIndex(v => v.Status);
        });

        modelBuilder.Entity<Ocorrencia>(entity =>
        {
            entity.ToTable("ocorrencias");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.Cpf).HasColumnName("cpf").HasMaxLength(11).IsRequired();
            entity.Property(o => o.Descricao).HasColumnName("descricao").HasMaxLength(500).IsRequired();
            entity.Property(o => o.Endereco).HasColumnName("endereco").HasMaxLength(300).IsRequired();
            entity.Property(o => o.Gravidade).HasColumnName("gravidade").HasConversion<string>().IsRequired();
            entity.Property(o => o.TipoEmergencia).HasColumnName("tipo_emergencia").HasConversion<string>().IsRequired();
            entity.Property(o => o.PacientesEnvolvidos).HasColumnName("pacientes_envolvidos").IsRequired();
            entity.Property(o => o.TempoEsperaMinutos).HasColumnName("tempo_espera_minutos").IsRequired();
            entity.Property(o => o.ScorePrioridade).HasColumnName("score_prioridade").HasPrecision(18, 2).IsRequired();
            entity.Property(o => o.Status).HasColumnName("status").HasConversion<string>().IsRequired();
            entity.Property(o => o.DataCriacao).HasColumnName("data_criacao").IsRequired();
            entity.Property(o => o.DataAtualizacao).HasColumnName("data_atualizacao");
            entity.Property(o => o.DataDespacho).HasColumnName("data_despacho");
            entity.Property(o => o.DataConclusao).HasColumnName("data_conclusao");
            entity.Property(o => o.VeiculoId).HasColumnName("veiculo_id");

            entity.HasOne(o => o.Veiculo)
                .WithMany()
                .HasForeignKey(o => o.VeiculoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(o => o.Cpf);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.ScorePrioridade);
            entity.HasIndex(o => o.VeiculoId);
        });
    }
}
