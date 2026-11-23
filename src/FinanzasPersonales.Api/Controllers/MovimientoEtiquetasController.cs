using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.DTOs;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/movimiento-etiquetas")]
public class MovimientoEtiquetasController : BaseApiController
{
    private readonly FinanzasDbContext _context;

    public MovimientoEtiquetasController(FinanzasDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Asocia una etiqueta a un movimiento.
    /// Regla de negocio: Verificar que tanto el Movimiento como la Etiqueta pertenezcan al usuario autenticado.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MovimientoEtiquetaResponseDto>> Asociar([FromBody] CrearMovimientoEtiquetaDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        // 1. Validar que el Movimiento exista y pertenezca al usuario autenticado
        var movimiento = await _context.Movimientos
            .FirstOrDefaultAsync(m => m.Id == dto.MovimientoId && m.UsuarioId == usuarioId.Value);

        if (movimiento == null)
        {
            return BadRequest(new { mensaje = "El movimiento no existe o no pertenece al usuario autenticado." });
        }

        // 2. Validar que la Etiqueta exista, esté activa y pertenezca al usuario autenticado
        var etiqueta = await _context.Etiquetas
            .FirstOrDefaultAsync(e => e.Id == dto.EtiquetaId && e.UsuarioId == usuarioId.Value);

        if (etiqueta == null)
        {
            return BadRequest(new { mensaje = "La etiqueta no existe o no pertenece al usuario autenticado." });
        }

        if (!etiqueta.Activa)
        {
            return BadRequest(new { mensaje = "La etiqueta se encuentra inactiva." });
        }

        // 3. Verificar si la relación ya existe
        var yaExiste = await _context.MovimientoEtiquetas
            .AnyAsync(me => me.MovimientoId == dto.MovimientoId && me.EtiquetaId == dto.EtiquetaId);

        if (yaExiste)
        {
            return Conflict(new { mensaje = "El movimiento ya tiene asociada esta etiqueta." });
        }

        var relacion = new MovimientoEtiqueta
        {
            MovimientoId = dto.MovimientoId,
            EtiquetaId = dto.EtiquetaId
        };

        _context.MovimientoEtiquetas.Add(relacion);
        await _context.SaveChangesAsync();

        var response = new MovimientoEtiquetaResponseDto
        {
            MovimientoId = relacion.MovimientoId,
            ConceptoMovimiento = movimiento.Concepto,
            EtiquetaId = relacion.EtiquetaId,
            NombreEtiqueta = etiqueta.Nombre,
            ColorHexEtiqueta = etiqueta.ColorHex
        };

        return CreatedAtAction(
            nameof(ObtenerPorClaveCompuesta),
            new { movimientoId = relacion.MovimientoId, etiquetaId = relacion.EtiquetaId },
            response);
    }

    /// <summary>
    /// Lista todas las relaciones movimiento-etiqueta pertenecientes al usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoEtiquetaResponseDto>>> Listar()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var lista = await _context.MovimientoEtiquetas
            .AsNoTracking()
            .Include(me => me.Movimiento)
            .Include(me => me.Etiqueta)
            .Where(me => me.Movimiento != null && me.Movimiento.UsuarioId == usuarioId.Value)
            .Select(me => new MovimientoEtiquetaResponseDto
            {
                MovimientoId = me.MovimientoId,
                ConceptoMovimiento = me.Movimiento != null ? me.Movimiento.Concepto : null,
                EtiquetaId = me.EtiquetaId,
                NombreEtiqueta = me.Etiqueta != null ? me.Etiqueta.Nombre : null,
                ColorHexEtiqueta = me.Etiqueta != null ? me.Etiqueta.ColorHex : null
            })
            .ToListAsync();

        return Ok(lista);
    }

    /// <summary>
    /// Obtiene una relación movimiento-etiqueta por su clave compuesta validando que pertenezca al usuario autenticado.
    /// </summary>
    [HttpGet("{movimientoId:guid}/{etiquetaId:guid}")]
    public async Task<ActionResult<MovimientoEtiquetaResponseDto>> ObtenerPorClaveCompuesta(Guid movimientoId, Guid etiquetaId)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var me = await _context.MovimientoEtiquetas
            .AsNoTracking()
            .Include(x => x.Movimiento)
            .Include(x => x.Etiqueta)
            .FirstOrDefaultAsync(x => x.MovimientoId == movimientoId &&
                                      x.EtiquetaId == etiquetaId &&
                                      x.Movimiento != null &&
                                      x.Movimiento.UsuarioId == usuarioId.Value);

        if (me == null)
        {
            return NotFound(new { mensaje = "Asociación movimiento-etiqueta no encontrada o no pertenece al usuario autenticado." });
        }

        return Ok(new MovimientoEtiquetaResponseDto
        {
            MovimientoId = me.MovimientoId,
            ConceptoMovimiento = me.Movimiento?.Concepto,
            EtiquetaId = me.EtiquetaId,
            NombreEtiqueta = me.Etiqueta?.Nombre,
            ColorHexEtiqueta = me.Etiqueta?.ColorHex
        });
    }
}
