using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Data.Configurations;

public class MovimientoConfiguration : IEntityTypeConfiguration<Movimiento>
{
    public void Configure(EntityTypeBuilder<Movimiento> builder)
    {
        builder.ToTable("movimiento");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(m => m.CuentaId)
            .HasColumnName("cuenta_id")
            .IsRequired();

        builder.Property(m => m.CategoriaId)
            .HasColumnName("categoria_id")
            .IsRequired();

        builder.Property(m => m.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.Monto)
            .HasColumnName("monto")
            .HasColumnType("numeric(14,2)")
            .IsRequired();

        builder.Property(m => m.Concepto)
            .HasColumnName("concepto")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.FechaMovimiento)
            .HasColumnName("fecha_movimiento")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(m => m.Notas)
            .HasColumnName("notas")
            .HasColumnType("text");

        builder.Property(m => m.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(m => m.Usuario)
            .WithMany(u => u.Movimientos)
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Cuenta)
            .WithMany(c => c.Movimientos)
            .HasForeignKey(m => m.CuentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Categoria)
            .WithMany(c => c.Movimientos)
            .HasForeignKey(m => m.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("chk_movimiento_monto", "monto > 0"));
    }
}
