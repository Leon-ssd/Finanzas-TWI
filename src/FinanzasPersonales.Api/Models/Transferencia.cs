namespace FinanzasPersonales.Api.Models;

public class Transferencia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Guid CuentaOrigenId { get; set; }
    public Guid CuentaDestinoId { get; set; }
    public decimal Monto { get; set; }
    public decimal Comision { get; set; } = 0m;
    public DateOnly FechaTransferencia { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string Concepto { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    // Navegación
    public Usuario? Usuario { get; set; }
    public Cuenta? CuentaOrigen { get; set; }
    public Cuenta? CuentaDestino { get; set; }
}
