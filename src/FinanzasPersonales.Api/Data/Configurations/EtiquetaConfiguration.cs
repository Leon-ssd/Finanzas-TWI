using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Data.Configurations;

public class EtiquetaConfiguration : IEntityTypeConfiguration<Etiqueta>
{
    public void Configure(EntityTypeBuilder<Etiqueta> builder)
    {
        builder.ToTable("etiqueta");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(7)
            .HasDefaultValue("#000000")
            .IsRequired();

        builder.Property(e => e.Activa)
            .HasColumnName("activa")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(e => e.Usuario)
            .WithMany(u => u.Etiquetas)
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Evitar nombres duplicados entre etiquetas activas del mismo usuario
        builder.HasIndex(e => new { e.UsuarioId, e.Nombre })
            .IsUnique()
            .HasFilter("activa = true")
            .HasDatabaseName("ix_etiqueta_usuario_nombre_activa");
    }
}
