using FinanzasPersonales.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentaController : ControllerBase
    {
        private static readonly Guid UsuarioSimuladoId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly CuentaService _service;

    public CuentaController(CuentaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CuentaResponseDto>>> GetAll()
    {
        var cuentas = await _service.GetAllByUsuarioAsync(UsuarioSimuladoId);
        return Ok(cuentas);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CuentaResponseDto>> GetById(Guid id)
    {
        var cuenta = await _service.GetByIdAsync(id, UsuarioSimuladoId);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    [HttpPost]
    public async Task<ActionResult<CuentaResponseDto>> Create(CrearCuentaDto dto)
    {
        try
        {
            var creada = await _service.CreateAsync(UsuarioSimuladoId, dto);
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    }
}
