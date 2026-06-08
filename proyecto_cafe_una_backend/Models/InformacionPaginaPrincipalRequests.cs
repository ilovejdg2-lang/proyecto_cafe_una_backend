namespace proyecto_cafe_una_backend.Models;

public class CrearEnlaceSitioRequest
{
    public string Etiqueta { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string Seccion { get; set; } = string.Empty;
    public int? Orden { get; set; }
    public bool AbrirEnNuevaPestana { get; set; }
}

public class ActualizarEnlaceSitioRequest
{
    public string? Etiqueta { get; set; }
    public string? Ruta { get; set; }
    public string? Seccion { get; set; }
    public int? Orden { get; set; }
    public bool? AbrirEnNuevaPestana { get; set; }
}

public class ActualizarInformacionFooterRequest
{
    public string? FraseMarca { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? MapsUrl { get; set; }
    public string? TextoCopyright { get; set; }
}

public class EliminarEnlaceSitioRequest
{
    public List<string> ActorRoles { get; set; } = [];
}
