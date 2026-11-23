namespace FinanzasPersonales.Api.Models;

public class MetaAhorro
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? CuentaVinculadaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal MontoObjetivo { get; set; }
    public decimal MontoAhorrado { get; set; } = 0m;
    public DateOnly? FechaLimite { get; set; }
    public string Estado { get; set; } = "activa";
}
