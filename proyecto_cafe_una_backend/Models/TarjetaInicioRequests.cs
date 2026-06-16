namespace proyecto_cafe_una_backend.Models;

public class ActualizarTarjetaInicioItemRequest
{
    public string Clave { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Ruta { get; set; }
}

public class ActualizarTarjetasInicioRequest
{
    public List<ActualizarTarjetaInicioItemRequest> Tarjetas { get; set; } = [];
}
