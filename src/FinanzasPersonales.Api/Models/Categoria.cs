namespace FinanzasPersonales.Api;

public class Categoria
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // ingreso, gasto
    public decimal? LimiteMensual { get; set; }
    public bool Activa { get; set; } = true;

    // Propiedades de navegación
    public Usuario Usuario { get; set; } = null!;
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
