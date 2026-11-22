namespace FinanzasPersonales.Api;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
    public bool Activo { get; set; } = true;

    // Propiedades de navegación
    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
