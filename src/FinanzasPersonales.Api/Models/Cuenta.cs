namespace FinanzasPersonales.Api.Models;

public class Cuenta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = "efectivo"; // efectivo, banco, billetera_digital
    public string Moneda { get; set; } = "BOB";
    public decimal SaldoInicial { get; set; } = 0m;
    public bool Activa { get; set; } = true;

    // Navegación
    public Usuario? Usuario { get; set; }
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    public ICollection<Transferencia> TransferenciasOrigen { get; set; } = new List<Transferencia>();
    public ICollection<Transferencia> TransferenciasDestino { get; set; } = new List<Transferencia>();
}
