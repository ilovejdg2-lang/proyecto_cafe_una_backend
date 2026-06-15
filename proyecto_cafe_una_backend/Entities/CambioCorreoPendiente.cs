namespace proyecto_cafe_una_backend.Entities;

public class CambioCorreoPendiente
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NuevoCorreo { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public bool Usado { get; set; }
}
