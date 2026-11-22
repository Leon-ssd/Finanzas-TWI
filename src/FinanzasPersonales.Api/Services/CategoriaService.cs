using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api;

public class CategoriaService
{
    private static readonly string[] TiposValidos = { "ingreso", "gasto" };
    private readonly ApiDbContext _db;

    public CategoriaService(ApiDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoriaResponseDto>> GetAllByUsuarioAsync(
        Guid usuarioId,
        string? tipo = null)
    {
        var query = _db.Categorias
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId);

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoFiltro = tipo.Trim().ToLowerInvariant();
            query = query.Where(c => c.Tipo == tipoFiltro);
        }

        return await query
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                UsuarioId = c.UsuarioId,
                NombreUsuario = c.Usuario.Nombre,
                Nombre = c.Nombre,
                Tipo = c.Tipo,
                LimiteMensual = c.LimiteMensual,
                Activa = c.Activa
            })
            .ToListAsync();
    }

    public async Task<CategoriaResponseDto?> GetByIdAsync(Guid id, Guid usuarioId)
    {
        return await _db.Categorias
            .AsNoTracking()
            .Where(c => c.Id == id && c.UsuarioId == usuarioId)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                UsuarioId = c.UsuarioId,
                NombreUsuario = c.Usuario.Nombre,
                Nombre = c.Nombre,
                Tipo = c.Tipo,
                LimiteMensual = c.LimiteMensual,
                Activa = c.Activa
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoriaResponseDto> CreateAsync(Guid usuarioId, CrearCategoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre de la categoría es obligatorio.");

        var tipoNormalizado = dto.Tipo.Trim().ToLowerInvariant();
        if (!TiposValidos.Contains(tipoNormalizado))
            throw new ArgumentException("Tipo de categoría inválido. Use: ingreso o gasto.");

        if (dto.LimiteMensual.HasValue && dto.LimiteMensual.Value < 0)
            throw new ArgumentException("El límite mensual no puede ser negativo.");

        if (tipoNormalizado == "ingreso" && dto.LimiteMensual.HasValue)
            throw new ArgumentException("Solo las categorías de gasto pueden tener límite mensual.");

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario is null)
            throw new ArgumentException("El usuario autenticado no existe.");

        if (!usuario.Activo)
            throw new ArgumentException("El usuario autenticado está inactivo.");

        var categoria = new Categoria
        {
            UsuarioId = usuarioId,
            Nombre = dto.Nombre.Trim(),
            Tipo = tipoNormalizado,
            LimiteMensual = dto.LimiteMensual,
            Activa = dto.Activa
        };

        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        return new CategoriaResponseDto
        {
            Id = categoria.Id,
            UsuarioId = categoria.UsuarioId,
            NombreUsuario = usuario.Nombre,
            Nombre = categoria.Nombre,
            Tipo = categoria.Tipo,
            LimiteMensual = categoria.LimiteMensual,
            Activa = categoria.Activa
        };
    }
}
