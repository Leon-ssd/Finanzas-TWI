using FinanzasPersonales.Api.Data.Configurations;
using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api.Data;

// Contexto mínimo: integrar aquí el bloque base del líder, sin crear otro contexto.
public class FinanzasDbContext(DbContextOptions<FinanzasDbContext> options) : DbContext(options)
{
    public DbSet<PresupuestoMensual> PresupuestosMensuales => Set<PresupuestoMensual>();
    public DbSet<MetaAhorro> MetasAhorro => Set<MetaAhorro>();
    public DbSet<MovimientoRecurrente> MovimientosRecurrentes => Set<MovimientoRecurrente>();
    public DbSet<RecordatorioPago> RecordatoriosPago => Set<RecordatorioPago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Registrar las entidades/configuraciones reales del líder ANTES de este bloque.
        // Las configuraciones fallan explícitamente si falta una entidad principal.
        modelBuilder.ApplyConfiguration(new PresupuestoMensualConfiguration());
        modelBuilder.ApplyConfiguration(new MetaAhorroConfiguration());
        modelBuilder.ApplyConfiguration(new MovimientoRecurrenteConfiguration());
        modelBuilder.ApplyConfiguration(new RecordatorioPagoConfiguration());
    }
}
