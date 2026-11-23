namespace FinanzasPersonales.Api.Models;

public class PresupuestoMensual
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid CategoriaId { get; set; }
    public short Año { get; set; }
    public short Mes { get; set; }
    public decimal MontoLimite { get; set; }
}
