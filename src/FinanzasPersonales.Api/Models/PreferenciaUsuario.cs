namespace FinanzasPersonales.Api.Models;

public class PreferenciaUsuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string MonedaPrincipal { get; set; } = "BOB";
    public short DiaInicioMes { get; set; } = 1;
    public short AlertaLimitePorcentaje { get; set; } = 80;
    public string TemaUi { get; set; } = "claro";

    // Navegación (1:1 con Usuario)
    public Usuario? Usuario { get; set; }
}
