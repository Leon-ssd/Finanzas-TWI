using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.DTOs;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/transferencias")]
public class TransferenciasController : BaseApiController
{
    private readonly FinanzasDbContext _context;

    public TransferenciasController(FinanzasDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registra una nueva transferencia entre dos cuentas del usuario autenticado.
    /// Descuenta monto + comision en la cuenta origen y suma monto en la cuenta destino sin crear registros en Movimiento.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TransferenciaResponseDto>> Crear([FromBody] CrearTransferenciaDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "No fue posible determinar la identidad del usuario autenticado." });
        }

        if (dto.CuentaOrigenId == dto.CuentaDestinoId)
        {
            return BadRequest(new { mensaje = "La cuenta origen y la cuenta destino deben ser diferentes." });
        }

        if (dto.Monto <= 0)
        {
            return BadRequest(new { mensaje = "El monto debe ser estrictamente mayor a 0." });
        }

        if (dto.Comision < 0)
        {
            return BadRequest(new { mensaje = "La comisión no puede ser un valor negativo." });
        }

        // Validar que ambas cuentas existan, pertenezcan al usuario autenticado y estén activas
        var cuentaOrigen = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == dto.CuentaOrigenId && c.UsuarioId == usuarioId.Value);
        if (cuentaOrigen == null)
        {
            return BadRequest(new { mensaje = "La cuenta origen no existe o no pertenece al usuario autenticado." });
        }
        if (!cuentaOrigen.Activa)
        {
            return BadRequest(new { mensaje = "La cuenta origen se encuentra inactiva." });
        }

        var cuentaDestino = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == dto.CuentaDestinoId && c.UsuarioId == usuarioId.Value);
        if (cuentaDestino == null)
        {
            return BadRequest(new { mensaje = "La cuenta destino no existe o no pertenece al usuario autenticado." });
        }
        if (!cuentaDestino.Activa)
        {
            return BadRequest(new { mensaje = "La cuenta destino se encuentra inactiva." });
        }

        var totalDescuento = dto.Monto + dto.Comision;
        if (cuentaOrigen.SaldoInicial < totalDescuento)
        {
            return BadRequest(new { mensaje = $"Saldo insuficiente en la cuenta origen. Saldo actual: {cuentaOrigen.SaldoInicial}, requerido: {totalDescuento}." });
        }

        var transferencia = new Transferencia
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId.Value,
            CuentaOrigenId = dto.CuentaOrigenId,
            CuentaDestinoId = dto.CuentaDestinoId,
            Monto = dto.Monto,
            Comision = dto.Comision,
            FechaTransferencia = dto.FechaTransferencia ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Concepto = dto.Concepto ?? string.Empty,
            FechaRegistro = DateTimeOffset.UtcNow
        };

        // Regla de negocio: Ajustar saldos de cuentas directamente sin tocar Movimiento
        cuentaOrigen.SaldoInicial -= totalDescuento;
        cuentaDestino.SaldoInicial += dto.Monto;

        _context.Transferencias.Add(transferencia);
        await _context.SaveChangesAsync();

        var response = new TransferenciaResponseDto
        {
            Id = transferencia.Id,
            UsuarioId = transferencia.UsuarioId,
            CuentaOrigenId = transferencia.CuentaOrigenId,
            NombreCuentaOrigen = cuentaOrigen.Nombre,
            CuentaDestinoId = transferencia.CuentaDestinoId,
            NombreCuentaDestino = cuentaDestino.Nombre,
            Monto = transferencia.Monto,
            Comision = transferencia.Comision,
            FechaTransferencia = transferencia.FechaTransferencia,
            Concepto = transferencia.Concepto,
            FechaRegistro = transferencia.FechaRegistro
        };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = transferencia.Id }, response);
    }

    /// <summary>
    /// Obtiene el listado de transferencias del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransferenciaResponseDto>>> Listar()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var transferencias = await _context.Transferencias
            .AsNoTracking()
            .Include(t => t.CuentaOrigen)
            .Include(t => t.CuentaDestino)
            .Where(t => t.UsuarioId == usuarioId.Value)
            .OrderByDescending(t => t.FechaTransferencia)
            .ThenByDescending(t => t.FechaRegistro)
            .Select(t => new TransferenciaResponseDto
            {
                Id = t.Id,
                UsuarioId = t.UsuarioId,
                CuentaOrigenId = t.CuentaOrigenId,
                NombreCuentaOrigen = t.CuentaOrigen != null ? t.CuentaOrigen.Nombre : null,
                CuentaDestinoId = t.CuentaDestinoId,
                NombreCuentaDestino = t.CuentaDestino != null ? t.CuentaDestino.Nombre : null,
                Monto = t.Monto,
                Comision = t.Comision,
                FechaTransferencia = t.FechaTransferencia,
                Concepto = t.Concepto,
                FechaRegistro = t.FechaRegistro
            })
            .ToListAsync();

        return Ok(transferencias);
    }

    /// <summary>
    /// Obtiene una transferencia específica por su ID validando que pertenezca al usuario autenticado.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransferenciaResponseDto>> ObtenerPorId(Guid id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        var t = await _context.Transferencias
            .AsNoTracking()
            .Include(tr => tr.CuentaOrigen)
            .Include(tr => tr.CuentaDestino)
            .FirstOrDefaultAsync(tr => tr.Id == id && tr.UsuarioId == usuarioId.Value);

        if (t == null)
        {
            return NotFound(new { mensaje = "Transferencia no encontrada o no pertenece al usuario autenticado." });
        }

        var response = new TransferenciaResponseDto
        {
            Id = t.Id,
            UsuarioId = t.UsuarioId,
            CuentaOrigenId = t.CuentaOrigenId,
            NombreCuentaOrigen = t.CuentaOrigen?.Nombre,
            CuentaDestinoId = t.CuentaDestinoId,
            NombreCuentaDestino = t.CuentaDestino?.Nombre,
            Monto = t.Monto,
            Comision = t.Comision,
            FechaTransferencia = t.FechaTransferencia,
            Concepto = t.Concepto,
            FechaRegistro = t.FechaRegistro
        };

        return Ok(response);
    }
}
