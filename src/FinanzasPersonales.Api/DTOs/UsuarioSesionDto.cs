namespace FinanzasPersonales.Api;

public class UsuarioSesionDto
{
    public bool Autenticado { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
