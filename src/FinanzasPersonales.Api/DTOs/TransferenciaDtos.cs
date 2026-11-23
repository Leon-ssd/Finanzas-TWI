using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.DTOs;

public class CrearTransferenciaDto
{
    [Required]
    public Guid CuentaOrigenId { get; set; }

    [Required]
    public Guid CuentaDestinoId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto a transferir debe ser mayor a 0.")]
    public decimal Monto { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "La comisión no puede ser negativa.")]
    public decimal Comision { get; set; } = 0m;

    public DateOnly? FechaTransferencia { get; set; }

    [StringLength(200, ErrorMessage = "El concepto no puede superar los 200 caracteres.")]
    public string Concepto { get; set; } = string.Empty;
}

public class TransferenciaResponseDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid CuentaOrigenId { get; set; }
    public string? NombreCuentaOrigen { get; set; }
    public Guid CuentaDestinoId { get; set; }
    public string? NombreCuentaDestino { get; set; }
    public decimal Monto { get; set; }
    public decimal Comision { get; set; }
    public DateOnly FechaTransferencia { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistro { get; set; }
}
