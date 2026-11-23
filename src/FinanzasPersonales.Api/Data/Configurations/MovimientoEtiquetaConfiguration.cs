using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Data.Configurations;

public class MovimientoEtiquetaConfiguration : IEntityTypeConfiguration<MovimientoEtiqueta>
{
    public void Configure(EntityTypeBuilder<MovimientoEtiqueta> builder)
    {
        builder.ToTable("movimiento_etiqueta");

        builder.HasKey(me => new { me.MovimientoId, me.EtiquetaId });

        builder.Property(me => me.MovimientoId)
            .HasColumnName("movimiento_id");

        builder.Property(me => me.EtiquetaId)
            .HasColumnName("etiqueta_id");

        builder.HasOne(me => me.Movimiento)
            .WithMany(m => m.MovimientoEtiquetas)
            .HasForeignKey(me => me.MovimientoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(me => me.Etiqueta)
            .WithMany(e => e.MovimientoEtiquetas)
            .HasForeignKey(me => me.EtiquetaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
