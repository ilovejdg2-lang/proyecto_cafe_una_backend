namespace proyecto_cafe_una_backend.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Estado { get; set; } = "activo";
    public List<string> Roles { get; set; } = ["Usuario"];
    public string? FotoPerfilUrl { get; set; }
    public string? FotoBannerUrl { get; set; }
    public string? FotoPerfilPosicion { get; set; }
    public string? FotoBannerPosicion { get; set; }
}
