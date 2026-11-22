using FinanzasPersonales.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private static readonly Guid UsuarioSimuladoId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly CategoriaService _service;

    public CategoriaController(CategoriaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoriaResponseDto>>> GetAll([FromQuery] string? tipo)
    {
        var categorias = await _service.GetAllByUsuarioAsync(UsuarioSimuladoId, tipo);
        return Ok(categorias);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoriaResponseDto>> GetById(Guid id)
    {
        var categoria = await _service.GetByIdAsync(id, UsuarioSimuladoId);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaResponseDto>> Create(CrearCategoriaDto dto)
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
