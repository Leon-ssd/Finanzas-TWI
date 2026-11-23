namespace FinanzasPersonales.Api.Models;

public class Etiqueta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public bool Activa { get; set; } = true;

    // Navegación
    public Usuario? Usuario { get; set; }
    public ICollection<MovimientoEtiqueta> MovimientoEtiquetas { get; set; } = new List<MovimientoEtiqueta>();
}
