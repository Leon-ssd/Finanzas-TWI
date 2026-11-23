using FinanzasPersonales.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientoController : ControllerBase
    {
        private static readonly Guid UsuarioSimuladoId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly MovimientoService _service;

    public MovimientoController(MovimientoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovimientoResponseDto>>> GetAll(
        [FromQuery] Guid? cuentaId,
        [FromQuery] Guid? categoriaId)
    {
        var movimientos = await _service.GetAllByUsuarioAsync(UsuarioSimuladoId, cuentaId, categoriaId);
        return Ok(movimientos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MovimientoResponseDto>> GetById(Guid id)
    {
        var movimiento = await _service.GetByIdAsync(id, UsuarioSimuladoId);
        return movimiento is null ? NotFound() : Ok(movimiento);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoResponseDto>> Create(CrearMovimientoDto dto)
    {
        try
        {
            var creado = await _service.CreateAsync(UsuarioSimuladoId, dto);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    }
}
