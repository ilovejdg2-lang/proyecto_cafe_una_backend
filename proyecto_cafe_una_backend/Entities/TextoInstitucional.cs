namespace proyecto_cafe_una_backend.Entities;

public class TextoInstitucional
{
    public string Clave { get; set; } = string.Empty;
    public string? Eyebrow { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
}
