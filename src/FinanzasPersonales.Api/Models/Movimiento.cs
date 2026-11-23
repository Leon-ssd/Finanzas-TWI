namespace FinanzasPersonales.Api.Models;

public class Movimiento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Guid CuentaId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Tipo { get; set; } = "gasto"; // ingreso, gasto
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public DateOnly FechaMovimiento { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string? Notas { get; set; }
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    // Navegación
    public Usuario? Usuario { get; set; }
    public Cuenta? Cuenta { get; set; }
    public Categoria? Categoria { get; set; }
    public ICollection<MovimientoEtiqueta> MovimientoEtiquetas { get; set; } = new List<MovimientoEtiqueta>();
}
