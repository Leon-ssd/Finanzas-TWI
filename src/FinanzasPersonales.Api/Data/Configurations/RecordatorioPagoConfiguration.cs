using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanzasPersonales.Api.Data.Configurations;

public class RecordatorioPagoConfiguration : IEntityTypeConfiguration<RecordatorioPago>
{
    public void Configure(EntityTypeBuilder<RecordatorioPago> builder)
    {
        builder.ToTable("recordatorio_pago", table =>
            table.HasCheckConstraint("ck_recordatorio_pago_monto_estimado",
                "monto_estimado IS NULL OR monto_estimado >= 0"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.MovimientoRecurrenteId).HasColumnName("movimiento_recurrente_id").HasColumnType("uuid").IsRequired(false);
        builder.Property(x => x.Titulo).HasColumnName("titulo").HasColumnType("varchar(120)").HasMaxLength(120).IsRequired();
        builder.Property(x => x.MontoEstimado).HasColumnName("monto_estimado").HasColumnType("numeric(14,2)").IsRequired(false);
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento").HasColumnType("date").IsRequired();
        builder.Property(x => x.Pagado).HasColumnName("pagado").HasColumnType("boolean").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.FechaPago).HasColumnName("fecha_pago").HasColumnType("date").IsRequired(false);
        builder.Property(x => x.Notas).HasColumnName("notas").HasColumnType("text").IsRequired(false);

        builder.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_recordatorio_pago_usuario_id");
        builder.HasIndex(x => x.MovimientoRecurrenteId).HasDatabaseName("ix_recordatorio_pago_movimiento_recurrente_id");

        RelacionesDev3.ConEntidadExistente(builder, "Usuario", nameof(RecordatorioPago.UsuarioId));
        builder.HasOne(x => x.MovimientoRecurrente)
            .WithMany()
            .HasForeignKey(x => x.MovimientoRecurrenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
