using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.DTOs;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/preferencias-usuario")]
public class PreferenciasUsuarioController : BaseApiController
{
    private readonly FinanzasDbContext _context;

    public PreferenciasUsuarioController(FinanzasDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea las preferencias para el usuario autenticado (relación 1:1).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PreferenciaUsuarioResponseDto>> Crear([FromBody] CrearPreferenciaUsuarioDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        if (dto.DiaInicioMes < 1 || dto.DiaInicioMes > 28)
        {
            return BadRequest(new { mensaje = "El día de inicio de mes debe estar comprendido entre 1 y 28." });
        }

        if (dto.AlertaLimitePorcentaje < 1 || dto.AlertaLimitePorcentaje > 100)
        {
            return BadRequest(new { mensaje = "El porcentaje de alerta debe estar comprendido entre 1 y 100." });
        }

        var yaExiste = await _context.PreferenciasUsuario.AnyAsync(p => p.UsuarioId == usuarioId.Value);
        if (yaExiste)
        {
            return Conflict(new { mensaje = "El usuario ya cuenta con preferencias registradas. Utilice la operación de actualización." });
        }

        var preferencia = new PreferenciaUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId.Value,
            MonedaPrincipal = string.IsNullOrWhiteSpace(dto.MonedaPrincipal) ? "BOB" : dto.MonedaPrincipal.ToUpper(),
            DiaInicioMes = dto.DiaInicioMes,
            AlertaLimitePorcentaje = dto.AlertaLimitePorcentaje,
            TemaUi = string.IsNullOrWhiteSpace(dto.TemaUi) ? "claro" : dto.TemaUi.ToLower()
        };

        _context.PreferenciasUsuario.Add(preferencia);
        await _context.SaveChangesAsync();

        var response = new PreferenciaUsuarioResponseDto
        {
            Id = preferencia.Id,
            UsuarioId = preferencia.UsuarioId,
            MonedaPrincipal = preferencia.MonedaPrincipal,
            DiaInicioMes = preferencia.DiaInicioMes,
            AlertaLimitePorcentaje = preferencia.AlertaLimitePorcentaje,
            TemaUi = preferencia.TemaUi
        };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = preferencia.Id }, response);
    }

    /// <summary>
    /// Obtiene las preferencias del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PreferenciaUsuarioResponseDto>> ObtenerPreferencias()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var p = await _context.PreferenciasUsuario
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId.Value);

        if (p == null)
        {
            return NotFound(new { mensaje = "No se encontraron preferencias configuradas para este usuario." });
        }

        return Ok(new PreferenciaUsuarioResponseDto
        {
            Id = p.Id,
            UsuarioId = p.UsuarioId,
            MonedaPrincipal = p.MonedaPrincipal,
            DiaInicioMes = p.DiaInicioMes,
            AlertaLimitePorcentaje = p.AlertaLimitePorcentaje,
            TemaUi = p.TemaUi
        });
    }

    /// <summary>
    /// Obtiene las preferencias por su identificador UUID validando que pertenezca al usuario autenticado.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PreferenciaUsuarioResponseDto>> ObtenerPorId(Guid id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var p = await _context.PreferenciasUsuario
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId.Value);

        if (p == null)
        {
            return NotFound(new { mensaje = "Preferencia no encontrada o no pertenece al usuario autenticado." });
        }

        return Ok(new PreferenciaUsuarioResponseDto
        {
            Id = p.Id,
            UsuarioId = p.UsuarioId,
            MonedaPrincipal = p.MonedaPrincipal,
            DiaInicioMes = p.DiaInicioMes,
            AlertaLimitePorcentaje = p.AlertaLimitePorcentaje,
            TemaUi = p.TemaUi
        });
    }
}
