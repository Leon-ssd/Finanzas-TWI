using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.DTOs;

public class CrearEtiquetaDto
{
    [Required(ErrorMessage = "El nombre de la etiqueta es obligatorio.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El nombre de la etiqueta no puede superar los 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [RegularExpression(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", ErrorMessage = "El color debe ser un valor hexadecimal válido (ej. #FF5733 o #000).")]
    public string ColorHex { get; set; } = "#000000";
}

public class EtiquetaResponseDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public bool Activa { get; set; }
}
