namespace proyecto_cafe_una_backend.Entities;

// Tarjeta individual debajo del texto introductorio (donaciones, visitas, voluntariado).
public class TarjetaInicio
{
    public long Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Ruta { get; set; }
    public int Orden { get; set; }
}
