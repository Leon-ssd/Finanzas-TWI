using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Models;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanzasDbContext).Assembly);
    }
}
