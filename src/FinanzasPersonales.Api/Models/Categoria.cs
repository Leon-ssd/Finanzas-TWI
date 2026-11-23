namespace FinanzasPersonales.Api.Models;

public class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = "gasto"; // ingreso, gasto
    public decimal? LimiteMensual { get; set; }
    public bool Activa { get; set; } = true;

    // Navegación
    public Usuario? Usuario { get; set; }
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
