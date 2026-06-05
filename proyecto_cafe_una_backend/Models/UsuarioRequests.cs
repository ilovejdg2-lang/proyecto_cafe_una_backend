namespace proyecto_cafe_una_backend.Models;

public class CambiarEstadoUsuarioRequest
{
    public string? Estado { get; set; }
    public int? ActorId { get; set; }
    public List<string> ActorRoles { get; set; } = [];
}

public class ActualizarUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public int? ActorId { get; set; }
    public List<string> ActorRoles { get; set; } = [];
}
