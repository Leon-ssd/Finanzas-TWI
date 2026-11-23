
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Data.Configurations;

namespace FinanzasPersonales.Api.Data;

public class FinanzasDbContext : DbContext
{
    public FinanzasDbContext(DbContextOptions<FinanzasDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Transferencia> Transferencias => Set<Transferencia>();
    public DbSet<PreferenciaUsuario> PreferenciasUsuario => Set<PreferenciaUsuario>();
    public DbSet<Etiqueta> Etiquetas => Set<Etiqueta>();
    public DbSet<MovimientoEtiqueta> MovimientoEtiquetas => Set<MovimientoEtiqueta>();
    public DbSet<PresupuestoMensual> PresupuestosMensuales => Set<PresupuestoMensual>();
    public DbSet<MetaAhorro> MetasAhorro => Set<MetaAhorro>();
    public DbSet<MovimientoRecurrente> MovimientosRecurrentes => Set<MovimientoRecurrente>();
    public DbSet<RecordatorioPago> RecordatoriosPago => Set<RecordatorioPago>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanzasDbContext).Assembly);

        // Registrar las entidades/configuraciones reales del líder ANTES de este bloque.
        // Las configuraciones fallan explícitamente si falta una entidad principal.
        modelBuilder.ApplyConfiguration(new PresupuestoMensualConfiguration());
        modelBuilder.ApplyConfiguration(new MetaAhorroConfiguration());
        modelBuilder.ApplyConfiguration(new MovimientoRecurrenteConfiguration());
        modelBuilder.ApplyConfiguration(new RecordatorioPagoConfiguration());

    }
}
