using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;

namespace proyecto_cafe_una_backend.Services;

public class UsuariosService(ApplicationDbContext db)
{
    private const string EstadoActivo = "activo";
    private const string EstadoInactivo = "inactivo";
    private const string RolSuperAdmin = "SuperAdmin";

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        var usuarios = await db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();
        return usuarios.Select(Copiar).ToList();
    }

    public async Task<List<Usuario>> ObtenerActivosAsync()
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.Where(u => EsActivo(u.Estado)).ToList();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return usuario is null ? null : Copiar(usuario);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        var normalized = correo.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == normalized);
        return usuario is null ? null : Copiar(usuario);
    }

    public async Task<Usuario?> ObtenerPorNombreOCorreoAsync(string identifier)
    {
        var normalized = identifier.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Correo.ToLower() == normalized ||
                u.Nombre.ToLower() == normalized);
        return usuario is null ? null : Copiar(usuario);
    }

    public async Task<bool> ExisteCorreoAsync(string correo) =>
        await ObtenerPorCorreoAsync(correo) is not null;

    public async Task<bool> ExisteNombreAsync(string nombre)
    {
        var normalized = nombre.Trim().ToLowerInvariant();
        return await db.Usuarios.AsNoTracking()
            .AnyAsync(u => u.Nombre.ToLower() == normalized);
    }

    public async Task<Usuario> CrearAsync(Usuario nuevoUsuario)
    {
        var usuarioCompleto = new Usuario
        {
            Nombre = nuevoUsuario.Nombre.Trim(),
            Correo = nuevoUsuario.Correo.Trim().ToLowerInvariant(),
            PasswordHash = nuevoUsuario.PasswordHash,
            Estado = EstadoActivo,
            Roles = nuevoUsuario.Roles.Count == 0 ? ["Usuario"] : [.. nuevoUsuario.Roles]
        };

        db.Usuarios.Add(usuarioCompleto);
        await db.SaveChangesAsync();
        return Copiar(usuarioCompleto);
    }

    public async Task<Usuario?> ActualizarConActorAsync(int id, Usuario cambios, int? actorId, IEnumerable<string>? actorRoles = null)
    {
        var actual = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (actual is null)
        {
            return null;
        }

        var puedeCambiarPassword = actorId.HasValue && actorId.Value == id;
        var actorEsSuperAdmin = EsSuperAdmin(actorRoles);
        var correoSolicitado = string.IsNullOrWhiteSpace(cambios.Correo)
            ? actual.Correo
            : cambios.Correo.Trim().ToLowerInvariant();

        var correoDuplicado = await db.Usuarios.AnyAsync(u =>
            u.Id != id &&
            u.Correo.ToLower() == correoSolicitado);
        if (correoDuplicado)
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        actual.Nombre = string.IsNullOrWhiteSpace(cambios.Nombre) ? actual.Nombre : cambios.Nombre.Trim();
        actual.Correo = correoSolicitado;
        actual.PasswordHash = string.IsNullOrWhiteSpace(cambios.PasswordHash) || !puedeCambiarPassword
            ? actual.PasswordHash
            : cambios.PasswordHash;
        actual.Estado = actorEsSuperAdmin && !string.IsNullOrWhiteSpace(cambios.Estado) ? cambios.Estado : actual.Estado;
        actual.Roles = actorEsSuperAdmin && cambios.Roles.Count > 0 ? [.. cambios.Roles] : [.. actual.Roles];

        await db.SaveChangesAsync();
        return Copiar(actual);
    }

    public async Task<Usuario?> ToggleEstadoAsync(int id, string? forzarEstado = null, int? actorId = null, IEnumerable<string>? actorRoles = null)
    {
        var actual = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (actual is null)
        {
            return null;
        }

        if (!EsSuperAdmin(actorRoles))
        {
            throw new InvalidOperationException("Solo un SuperAdmin puede inactivar o activar usuarios.");
        }

        if (actorId.HasValue && actorId.Value == id)
        {
            throw new InvalidOperationException("No puedes cambiar tu propio estado.");
        }

        var estadoSolicitado = NormalizarEstado(forzarEstado);
        if (string.Equals(estadoSolicitado, EstadoActivo, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Solo se permite inactivar usuarios.");
        }

        if (!EsActivo(actual.Estado))
        {
            return Copiar(actual);
        }

        actual.Estado = EstadoInactivo;
        await db.SaveChangesAsync();
        return Copiar(actual);
    }

    public async Task<Usuario?> ActualizarPasswordPorCorreoAsync(string correo, string nuevaPassword)
    {
        var normalized = correo.Trim().ToLowerInvariant();
        var actual = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo.ToLower() == normalized);
        if (actual is null)
        {
            return null;
        }

        actual.PasswordHash = nuevaPassword;
        await db.SaveChangesAsync();
        return Copiar(actual);
    }

    private static Usuario Copiar(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Correo = usuario.Correo,
        PasswordHash = usuario.PasswordHash,
        Estado = usuario.Estado,
        Roles = [.. usuario.Roles]
    };

    private static bool EsActivo(string? estado) =>
        string.Equals((estado ?? string.Empty).Trim(), EstadoActivo, StringComparison.OrdinalIgnoreCase);

    private static bool EsSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(r => string.Equals((r ?? string.Empty).Trim(), RolSuperAdmin, StringComparison.OrdinalIgnoreCase)) == true;

    private static string? NormalizarEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return null;
        }

        return string.Equals(estado.Trim(), EstadoInactivo, StringComparison.OrdinalIgnoreCase)
            ? EstadoInactivo
            : EstadoActivo;
    }
}
