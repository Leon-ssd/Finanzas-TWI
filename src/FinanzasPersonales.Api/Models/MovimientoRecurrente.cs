namespace FinanzasPersonales.Api.Models;

public class MovimientoRecurrente
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid CuentaId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal MontoEstimado { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public string Frecuencia { get; set; } = string.Empty;

    // Semanal: lunes=1 a domingo=7. Las demás: día del mes (1 a 28).
    // Quincenal: día ancla; siguiente ocurrencia a los 15 días.
    // Anual: el mes de referencia se obtiene de ProximaFecha.
    public short DiaEjecucion { get; set; }

    public DateOnly ProximaFecha { get; set; }
    public bool Activo { get; set; } = true;
}
