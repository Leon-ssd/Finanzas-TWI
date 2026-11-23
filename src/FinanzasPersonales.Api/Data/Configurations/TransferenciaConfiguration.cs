using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Data.Configurations;

public class TransferenciaConfiguration : IEntityTypeConfiguration<Transferencia>
{
    public void Configure(EntityTypeBuilder<Transferencia> builder)
    {
        builder.ToTable("transferencia");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(t => t.CuentaOrigenId)
            .HasColumnName("cuenta_origen_id")
            .IsRequired();

        builder.Property(t => t.CuentaDestinoId)
            .HasColumnName("cuenta_destino_id")
            .IsRequired();

        builder.Property(t => t.Monto)
            .HasColumnName("monto")
            .HasColumnType("numeric(14,2)")
            .IsRequired();

        builder.Property(t => t.Comision)
            .HasColumnName("comision")
            .HasColumnType("numeric(14,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(t => t.FechaTransferencia)
            .HasColumnName("fecha_transferencia")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(t => t.Concepto)
            .HasColumnName("concepto")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(t => t.Usuario)
            .WithMany(u => u.Transferencias)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.CuentaOrigen)
            .WithMany(c => c.TransferenciasOrigen)
            .HasForeignKey(t => t.CuentaOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CuentaDestino)
            .WithMany(c => c.TransferenciasDestino)
            .HasForeignKey(t => t.CuentaDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_transferencia_monto_positivo", "monto > 0");
            t.HasCheckConstraint("chk_transferencia_comision_no_negativa", "comision >= 0");
            t.HasCheckConstraint("chk_transferencia_cuentas_distintas", "cuenta_origen_id <> cuenta_destino_id");
        });
    }
}
