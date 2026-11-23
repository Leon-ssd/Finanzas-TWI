using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanzasPersonales.Api.Data.Configurations;

public class MetaAhorroConfiguration : IEntityTypeConfiguration<MetaAhorro>
{
    public void Configure(EntityTypeBuilder<MetaAhorro> builder)
    {
        builder.ToTable("meta_ahorro", table =>
        {
            table.HasCheckConstraint("ck_meta_ahorro_monto_objetivo", "monto_objetivo > 0");
            table.HasCheckConstraint("ck_meta_ahorro_monto_ahorrado", "monto_ahorrado >= 0");
            table.HasCheckConstraint("ck_meta_ahorro_estado", "estado IN ('activa', 'completada', 'pausada')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CuentaVinculadaId).HasColumnName("cuenta_vinculada_id").HasColumnType("uuid").IsRequired(false);
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
        builder.Property(x => x.MontoObjetivo).HasColumnName("monto_objetivo").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(x => x.MontoAhorrado).HasColumnName("monto_ahorrado").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.FechaLimite).HasColumnName("fecha_limite").HasColumnType("date").IsRequired(false);
        builder.Property(x => x.Estado).HasColumnName("estado").HasColumnType("varchar(20)").HasMaxLength(20).HasDefaultValue("activa").IsRequired();

        builder.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_meta_ahorro_usuario_id");
        builder.HasIndex(x => x.CuentaVinculadaId).HasDatabaseName("ix_meta_ahorro_cuenta_vinculada_id");

        RelacionesDev3.ConEntidadExistente(builder, "Usuario", nameof(MetaAhorro.UsuarioId), "Usuario");
        RelacionesDev3.ConEntidadExistente(builder, "Cuenta", nameof(MetaAhorro.CuentaVinculadaId));
    }
}
