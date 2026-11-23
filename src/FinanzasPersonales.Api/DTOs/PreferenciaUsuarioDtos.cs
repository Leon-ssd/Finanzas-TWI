using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.DTOs;

public class CrearPreferenciaUsuarioDto
{
    [StringLength(3, MinimumLength = 3, ErrorMessage = "La moneda principal debe contener 3 caracteres (ej. BOB, USD).")]
    public string MonedaPrincipal { get; set; } = "BOB";

    [Range(1, 28, ErrorMessage = "El día de inicio de mes debe estar entre 1 y 28.")]
    public short DiaInicioMes { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "La alerta de límite debe estar entre 1 y 100 por ciento.")]
    public short AlertaLimitePorcentaje { get; set; } = 80;

    [StringLength(20, ErrorMessage = "El tema de la interfaz no puede superar los 20 caracteres.")]
    public string TemaUi { get; set; } = "claro";
}

public class PreferenciaUsuarioResponseDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string MonedaPrincipal { get; set; } = "BOB";
    public short DiaInicioMes { get; set; }
    public short AlertaLimitePorcentaje { get; set; }
    public string TemaUi { get; set; } = "claro";
}
