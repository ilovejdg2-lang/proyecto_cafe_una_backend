namespace proyecto_cafe_una_backend.Models;

public class CedulaConsultaResponse
{
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Residencia { get; set; }
    public string? Provincia { get; set; }
    public string? Canton { get; set; }
    public string? Distrito { get; set; }
    public string? FechaNacimiento { get; set; }
}
