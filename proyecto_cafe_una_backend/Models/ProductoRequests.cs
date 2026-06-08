namespace proyecto_cafe_una_backend.Models;

public class CrearProductoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
    public decimal PrecioNormal { get; set; }
    public decimal PrecioConIVA { get; set; }
    public int Stock { get; set; }
    public string Estado { get; set; } = "Habilitado";
    public string Peso { get; set; } = string.Empty;
    public List<string> ActorRoles { get; set; } = [];
}

public class ActualizarProductoRequest
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Imagen { get; set; }
    public decimal? PrecioNormal { get; set; }
    public decimal? PrecioConIVA { get; set; }
    public int? Stock { get; set; }
    public string? Estado { get; set; }
    public string? Peso { get; set; }
    public List<string> ActorRoles { get; set; } = [];
}

public class EliminarProductoRequest
{
    public List<string> ActorRoles { get; set; } = [];
}

public class AjustarStockProductoItemRequest
{
    public long Id { get; set; }
    public int Units { get; set; }
}
