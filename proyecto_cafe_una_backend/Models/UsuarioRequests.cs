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
    public string PasswordActual { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public int? ActorId { get; set; }
    public List<string> ActorRoles { get; set; } = [];
}

public class ActualizarPerfilRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? FotoPerfilUrl { get; set; }
    public string? FotoBannerUrl { get; set; }
    public string? FotoPerfilPosicion { get; set; }
    public string? FotoBannerPosicion { get; set; }
}

public class CambiarPasswordRequest
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}

public class UsuarioPerfilResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string? FotoPerfilUrl { get; set; }
    public string? FotoBannerUrl { get; set; }
    public string? FotoPerfilPosicion { get; set; }
    public string? FotoBannerPosicion { get; set; }
}

public class SolicitarCambioCorreoRequest
{
    public string NuevoCorreo { get; set; } = string.Empty;
}

public class ConfirmarCambioCorreoRequest
{
    public string NuevoCorreo { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class CambioCorreoResult
{
    public bool EmailEnviado { get; set; }
    public string? MensajeError { get; set; }
}

public class SolicitarCreacionUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

public class ConfirmarCreacionUsuarioRequest
{
    public string Correo { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
