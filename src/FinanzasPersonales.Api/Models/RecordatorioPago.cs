namespace FinanzasPersonales.Api.Models;

public class RecordatorioPago
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? MovimientoRecurrenteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal? MontoEstimado { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool Pagado { get; set; } = false;
    public DateOnly? FechaPago { get; set; }
    public string? Notas { get; set; }
    public MovimientoRecurrente? MovimientoRecurrente { get; set; }
}
