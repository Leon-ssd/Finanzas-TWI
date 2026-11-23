using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_usuario_email");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255);

        builder.Property(u => u.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(u => u.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .IsRequired();
    }
}
