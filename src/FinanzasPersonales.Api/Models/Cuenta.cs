namespace FinanzasPersonales.Api;

public class Cuenta
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // efectivo, banco, billetera_digital
    public string Moneda { get; set; } = "BOB";
    public decimal SaldoInicial { get; set; }
    public bool Activa { get; set; } = true;

    // Propiedades de navegación
    public Usuario Usuario { get; set; } = null!;
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
