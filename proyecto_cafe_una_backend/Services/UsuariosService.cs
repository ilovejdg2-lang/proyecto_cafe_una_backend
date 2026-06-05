using proyecto_cafe_una_backend.Entities;

namespace proyecto_cafe_una_backend.Services;

public class UsuariosService
{
    private readonly List<Usuario> _usuarios = DefaultUsuarios();
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private const string EstadoActivo = "activo";
    private const string EstadoInactivo = "inactivo";
    private const string RolSuperAdmin = "SuperAdmin";

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _usuarios.Select(Copiar).OrderBy(u => u.Id).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<List<Usuario>> ObtenerActivosAsync()
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.Where(u => EsActivo(u.Estado)).ToList();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.FirstOrDefault(u => u.Id == id);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        var normalized = correo.Trim().ToLowerInvariant();
        var usuarios = await ObtenerTodosAsync();
        return usuarios.FirstOrDefault(u => u.Correo.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Usuario?> ObtenerPorNombreOCorreoAsync(string identifier)
    {
        var normalized = identifier.Trim().ToLowerInvariant();
        var usuarios = await ObtenerTodosAsync();
        return usuarios.FirstOrDefault(u =>
            u.Correo.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
            u.Nombre.Equals(normalized, StringComparison.OrdinalIgnoreCase)
        );
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        return await ObtenerPorCorreoAsync(correo) is not null;
    }

    public async Task<Usuario> CrearAsync(Usuario nuevoUsuario)
    {
        await _mutex.WaitAsync();
        try
        {
            var nextId = _usuarios.Count == 0 ? 1 : _usuarios.Max(u => u.Id) + 1;
            var usuarioCompleto = new Usuario
            {
                Id = nextId,
                Nombre = nuevoUsuario.Nombre.Trim(),
                Correo = nuevoUsuario.Correo.Trim().ToLowerInvariant(),
                PasswordHash = nuevoUsuario.PasswordHash,
                Estado = EstadoActivo,
                Roles = nuevoUsuario.Roles.Count == 0 ? ["Usuario"] : [.. nuevoUsuario.Roles]
            };

            _usuarios.Add(usuarioCompleto);
            return Copiar(usuarioCompleto);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Usuario?> ActualizarConActorAsync(int id, Usuario cambios, int? actorId, IEnumerable<string>? actorRoles = null)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _usuarios.FindIndex(u => u.Id == id);
            if (index < 0)
            {
                return null;
            }

            var actual = _usuarios[index];
            var puedeCambiarPassword = actorId.HasValue && actorId.Value == id;
            var actorEsSuperAdmin = EsSuperAdmin(actorRoles);
            var correoSolicitado = string.IsNullOrWhiteSpace(cambios.Correo)
                ? actual.Correo
                : cambios.Correo.Trim().ToLowerInvariant();

            var correoDuplicado = _usuarios.Any(u =>
                u.Id != id &&
                u.Correo.Equals(correoSolicitado, StringComparison.OrdinalIgnoreCase)
            );
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

            return Copiar(actual);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Usuario?> ToggleEstadoAsync(int id, string? forzarEstado = null, int? actorId = null, IEnumerable<string>? actorRoles = null)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _usuarios.FindIndex(u => u.Id == id);
            if (index < 0)
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

            var actual = _usuarios[index];
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
            return Copiar(actual);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Usuario?> ActualizarPasswordPorCorreoAsync(string correo, string nuevaPassword)
    {
        await _mutex.WaitAsync();
        try
        {
            var normalized = correo.Trim().ToLowerInvariant();
            var actual = _usuarios.FirstOrDefault(u => u.Correo.Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (actual is null)
            {
                return null;
            }

            actual.PasswordHash = nuevaPassword;
            return Copiar(actual);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private static List<Usuario> DefaultUsuarios()
    {
        return
        [
            new Usuario
            {
                Id = 1,
                Nombre = "Admin",
                Correo = "admin@cafeuna.com",
                PasswordHash = "admin123",
                Estado = "activo",
                Roles = ["SuperAdmin"]
            }
        ];
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
