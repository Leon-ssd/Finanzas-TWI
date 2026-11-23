using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.DTOs;

public class CrearMovimientoEtiquetaDto
{
    [Required]
    public Guid MovimientoId { get; set; }

    [Required]
    public Guid EtiquetaId { get; set; }
}

public class MovimientoEtiquetaResponseDto
{
    public Guid MovimientoId { get; set; }
    public string? ConceptoMovimiento { get; set; }
    public Guid EtiquetaId { get; set; }
    public string? NombreEtiqueta { get; set; }
    public string? ColorHexEtiqueta { get; set; }
}
