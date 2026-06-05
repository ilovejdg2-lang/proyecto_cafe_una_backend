namespace proyecto_cafe_una_backend.Entities;

public class PasswordResetEntry
{
    public string Token { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public bool Usado { get; set; }
}
