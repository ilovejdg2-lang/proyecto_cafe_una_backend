namespace proyecto_cafe_una_backend.Entities;

public class UsuarioCreacionPendiente
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public DateTime ExpiraEnUtc { get; set; }
    public bool Usado { get; set; }
}
