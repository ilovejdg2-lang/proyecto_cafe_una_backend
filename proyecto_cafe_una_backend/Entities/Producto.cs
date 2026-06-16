namespace proyecto_cafe_una_backend.Entities;

public class Producto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
    public decimal PrecioNormal { get; set; }
    public decimal PrecioConIVA { get; set; }
    public int Stock { get; set; }
    public string Estado { get; set; } = "Habilitado";
    public string Peso { get; set; } = string.Empty;
    public bool EsDestacado { get; set; }
}
