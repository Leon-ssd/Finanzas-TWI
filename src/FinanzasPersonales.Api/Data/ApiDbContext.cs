using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api;

public class ApiDbContext : DbContext
{

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
        
    }

    public DbSet<Usuario> Usuarios {get; set;}
    public DbSet<Cuenta> Cuentas {get; set;}
    public DbSet<Categoria> Categorias {get; set;}
    public DbSet<Movimiento> Movimientos {get; set;}
}
