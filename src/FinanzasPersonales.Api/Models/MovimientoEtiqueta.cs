namespace FinanzasPersonales.Api.Models;

public class MovimientoEtiqueta
{
    public Guid MovimientoId { get; set; }
    public Guid EtiquetaId { get; set; }

    // Navegación
    public Movimiento? Movimiento { get; set; }
    public Etiqueta? Etiqueta { get; set; }
}
