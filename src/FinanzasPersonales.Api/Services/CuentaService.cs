using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api;

public class CuentaService
{
    private static readonly string[] TiposValidos = { "efectivo", "banco", "billetera_digital" };
    private readonly ApiDbContext _db;

    public CuentaService(ApiDbContext db)
    {
        _db = db;
    }

    public async Task<List<CuentaResponseDto>> GetAllByUsuarioAsync(Guid usuarioId)
    {
        return await _db.Cuentas
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nombre)
            .Select(c => new CuentaResponseDto
            {
                Id = c.Id,
                UsuarioId = c.UsuarioId,
                NombreUsuario = c.Usuario.Nombre,
                Nombre = c.Nombre,
                Tipo = c.Tipo,
                Moneda = c.Moneda,
                SaldoInicial = c.SaldoInicial,
                Activa = c.Activa
            })
            .ToListAsync();
    }

    public async Task<CuentaResponseDto?> GetByIdAsync(Guid id, Guid usuarioId)
    {
        return await _db.Cuentas
            .AsNoTracking()
            .Where(c => c.Id == id && c.UsuarioId == usuarioId)
            .Select(c => new CuentaResponseDto
            {
                Id = c.Id,
                UsuarioId = c.UsuarioId,
                NombreUsuario = c.Usuario.Nombre,
                Nombre = c.Nombre,
                Tipo = c.Tipo,
                Moneda = c.Moneda,
                SaldoInicial = c.SaldoInicial,
                Activa = c.Activa
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CuentaResponseDto> CreateAsync(Guid usuarioId, CrearCuentaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre de la cuenta es obligatorio.");

        var tipoNormalizado = dto.Tipo.Trim().ToLowerInvariant();
        if (!TiposValidos.Contains(tipoNormalizado))
            throw new ArgumentException("Tipo de cuenta inválido. Use: efectivo, banco o billetera_digital.");

        if (dto.SaldoInicial < 0)
            throw new ArgumentException("El saldo inicial no puede ser negativo.");

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario is null)
            throw new ArgumentException("El usuario autenticado no existe.");

        if (!usuario.Activo)
            throw new ArgumentException("El usuario autenticado está inactivo.");

        var cuenta = new Cuenta
        {
            UsuarioId = usuarioId,
            Nombre = dto.Nombre.Trim(),
            Tipo = tipoNormalizado,
            Moneda = string.IsNullOrWhiteSpace(dto.Moneda) ? "BOB" : dto.Moneda.Trim().ToUpperInvariant(),
            SaldoInicial = dto.SaldoInicial,
            Activa = dto.Activa
        };

        _db.Cuentas.Add(cuenta);
        await _db.SaveChangesAsync();

        return new CuentaResponseDto
        {
            Id = cuenta.Id,
            UsuarioId = cuenta.UsuarioId,
            NombreUsuario = usuario.Nombre,
            Nombre = cuenta.Nombre,
            Tipo = cuenta.Tipo,
            Moneda = cuenta.Moneda,
            SaldoInicial = cuenta.SaldoInicial,
            Activa = cuenta.Activa
        };
    }
}
