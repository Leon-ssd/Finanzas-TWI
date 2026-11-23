using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanzasPersonales.Api.Data.Configurations;

public class PresupuestoMensualConfiguration : IEntityTypeConfiguration<PresupuestoMensual>
{
    public void Configure(EntityTypeBuilder<PresupuestoMensual> builder)
    {
        builder.ToTable("presupuesto_mensual", table =>
        {
            table.HasCheckConstraint("ck_presupuesto_mensual_mes", "mes BETWEEN 1 AND 12");
            table.HasCheckConstraint("ck_presupuesto_mensual_monto_limite", "monto_limite >= 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.Año).HasColumnName("año").HasColumnType("smallint").IsRequired();
        builder.Property(x => x.Mes).HasColumnName("mes").HasColumnType("smallint").IsRequired();
        builder.Property(x => x.MontoLimite).HasColumnName("monto_limite").HasColumnType("numeric(14,2)").IsRequired();

        builder.HasIndex(x => new { x.UsuarioId, x.CategoriaId, x.Año, x.Mes })
            .IsUnique().HasDatabaseName("ux_presupuesto_mensual_usuario_categoria_periodo");
        builder.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_presupuesto_mensual_categoria_id");

        RelacionesDev3.ConEntidadExistente(builder, "Usuario", nameof(PresupuestoMensual.UsuarioId));
        RelacionesDev3.ConEntidadExistente(builder, "Categoria", nameof(PresupuestoMensual.CategoriaId));
    }
}
