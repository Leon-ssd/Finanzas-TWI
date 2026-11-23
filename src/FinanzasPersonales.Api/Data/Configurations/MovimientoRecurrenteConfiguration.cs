using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanzasPersonales.Api.Data.Configurations;

public class MovimientoRecurrenteConfiguration : IEntityTypeConfiguration<MovimientoRecurrente>
{
    public void Configure(EntityTypeBuilder<MovimientoRecurrente> builder)
    {
        builder.ToTable("movimiento_recurrente", table =>
        {
            table.HasCheckConstraint("ck_movimiento_recurrente_tipo", "tipo IN ('ingreso', 'gasto')");
            table.HasCheckConstraint("ck_movimiento_recurrente_monto_estimado", "monto_estimado > 0");
            table.HasCheckConstraint("ck_movimiento_recurrente_frecuencia", "frecuencia IN ('semanal', 'quincenal', 'mensual', 'anual')");
            table.HasCheckConstraint("ck_movimiento_recurrente_dia_ejecucion",
                "(frecuencia = 'semanal' AND dia_ejecucion BETWEEN 1 AND 7) OR " +
                "(frecuencia IN ('quincenal', 'mensual', 'anual') AND dia_ejecucion BETWEEN 1 AND 28)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CuentaId).HasColumnName("cuenta_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.MontoEstimado).HasColumnName("monto_estimado").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(x => x.Concepto).HasColumnName("concepto").HasColumnType("varchar(200)").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Frecuencia).HasColumnName("frecuencia").HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();
        builder.Property(x => x.DiaEjecucion).HasColumnName("dia_ejecucion").HasColumnType("smallint").IsRequired();
        builder.Property(x => x.ProximaFecha).HasColumnName("proxima_fecha").HasColumnType("date").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasColumnType("boolean").HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_movimiento_recurrente_usuario_id");
        builder.HasIndex(x => x.CuentaId).HasDatabaseName("ix_movimiento_recurrente_cuenta_id");
        builder.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_movimiento_recurrente_categoria_id");

        RelacionesDev3.ConEntidadExistente(builder, "Usuario", nameof(MovimientoRecurrente.UsuarioId));
        RelacionesDev3.ConEntidadExistente(builder, "Cuenta", nameof(MovimientoRecurrente.CuentaId));
        RelacionesDev3.ConEntidadExistente(builder, "Categoria", nameof(MovimientoRecurrente.CategoriaId));
    }
}
