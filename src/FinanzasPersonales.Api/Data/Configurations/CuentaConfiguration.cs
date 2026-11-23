using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Data.Configurations;

public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("cuenta");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.Moneda)
            .HasColumnName("moneda")
            .HasMaxLength(3)
            .HasDefaultValue("BOB")
            .IsRequired();

        builder.Property(c => c.SaldoInicial)
            .HasColumnName("saldo_inicial")
            .HasColumnType("numeric(14,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(c => c.Activa)
            .HasColumnName("activa")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Cuentas)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("chk_cuenta_saldo_inicial", "saldo_inicial >= 0"));
    }
}
