namespace proyecto_cafe_una_backend.Entities;

public class TarjetaInicio
{
    public string Clave { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Ruta { get; set; }
    public int Orden { get; set; }
}
