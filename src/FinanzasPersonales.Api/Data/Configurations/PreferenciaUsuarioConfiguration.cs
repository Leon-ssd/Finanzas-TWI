using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Data.Configurations;

public class PreferenciaUsuarioConfiguration : IEntityTypeConfiguration<PreferenciaUsuario>
{
    public void Configure(EntityTypeBuilder<PreferenciaUsuario> builder)
    {
        builder.ToTable("preferencia_usuario");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(p => p.MonedaPrincipal)
            .HasColumnName("moneda_principal")
            .HasMaxLength(3)
            .HasDefaultValue("BOB")
            .IsRequired();

        builder.Property(p => p.DiaInicioMes)
            .HasColumnName("dia_inicio_mes")
            .HasColumnType("smallint")
            .HasDefaultValue((short)1)
            .IsRequired();

        builder.Property(p => p.AlertaLimitePorcentaje)
            .HasColumnName("alerta_limite_porcentaje")
            .HasColumnType("smallint")
            .HasDefaultValue((short)80)
            .IsRequired();

        builder.Property(p => p.TemaUi)
            .HasColumnName("tema_ui")
            .HasMaxLength(20)
            .HasDefaultValue("claro")
            .IsRequired();

        builder.HasIndex(p => p.UsuarioId)
            .IsUnique()
            .HasDatabaseName("ix_preferencia_usuario_usuario_id");

        builder.HasOne(p => p.Usuario)
            .WithOne(u => u.PreferenciaUsuario)
            .HasForeignKey<PreferenciaUsuario>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_preferencia_dia_inicio_mes", "dia_inicio_mes >= 1 AND dia_inicio_mes <= 28");
            t.HasCheckConstraint("chk_preferencia_alerta_limite_porcentaje", "alerta_limite_porcentaje >= 1 AND alerta_limite_porcentaje <= 100");
        });
    }
}
