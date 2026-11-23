using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.DTOs;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/etiquetas")]
public class EtiquetasController : BaseApiController
{
    private readonly FinanzasDbContext _context;

    public EtiquetasController(FinanzasDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea una nueva etiqueta asociada al usuario autenticado.
    /// Evita nombres duplicados entre etiquetas activas del mismo usuario.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EtiquetaResponseDto>> Crear([FromBody] CrearEtiquetaDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var nombreNormalizado = dto.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombreNormalizado))
        {
            return BadRequest(new { mensaje = "El nombre de la etiqueta no puede estar vacío." });
        }

        // Regla de negocio: Evitar nombres duplicados entre etiquetas activas del mismo usuario
        var duplicado = await _context.Etiquetas.AnyAsync(e =>
            e.UsuarioId == usuarioId.Value &&
            e.Activa &&
            e.Nombre.ToLower() == nombreNormalizado.ToLower());

        if (duplicado)
        {
            return Conflict(new { mensaje = $"Ya existe una etiqueta activa con el nombre '{nombreNormalizado}' para el usuario autenticado." });
        }

        var etiqueta = new Etiqueta
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId.Value,
            Nombre = nombreNormalizado,
            ColorHex = string.IsNullOrWhiteSpace(dto.ColorHex) ? "#000000" : dto.ColorHex,
            Activa = true
        };

        _context.Etiquetas.Add(etiqueta);
        await _context.SaveChangesAsync();

        var response = new EtiquetaResponseDto
        {
            Id = etiqueta.Id,
            UsuarioId = etiqueta.UsuarioId,
            Nombre = etiqueta.Nombre,
            ColorHex = etiqueta.ColorHex,
            Activa = etiqueta.Activa
        };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = etiqueta.Id }, response);
    }

    /// <summary>
    /// Obtiene todas las etiquetas del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EtiquetaResponseDto>>> Listar([FromQuery] bool? soloActivas = null)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var query = _context.Etiquetas
            .AsNoTracking()
            .Where(e => e.UsuarioId == usuarioId.Value);

        if (soloActivas.HasValue)
        {
            query = query.Where(e => e.Activa == soloActivas.Value);
        }

        var lista = await query
            .OrderBy(e => e.Nombre)
            .Select(e => new EtiquetaResponseDto
            {
                Id = e.Id,
                UsuarioId = e.UsuarioId,
                Nombre = e.Nombre,
                ColorHex = e.ColorHex,
                Activa = e.Activa
            })
            .ToListAsync();

        return Ok(lista);
    }

    /// <summary>
    /// Obtiene una etiqueta por su ID validando que pertenezca al usuario autenticado.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EtiquetaResponseDto>> ObtenerPorId(Guid id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var e = await _context.Etiquetas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId.Value);

        if (e == null)
        {
            return NotFound(new { mensaje = "Etiqueta no encontrada o no pertenece al usuario autenticado." });
        }

        return Ok(new EtiquetaResponseDto
        {
            Id = e.Id,
            UsuarioId = e.UsuarioId,
            Nombre = e.Nombre,
            ColorHex = e.ColorHex,
            Activa = e.Activa
        });
    }
}
