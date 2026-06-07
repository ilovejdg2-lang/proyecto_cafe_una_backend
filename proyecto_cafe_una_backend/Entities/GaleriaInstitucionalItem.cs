namespace proyecto_cafe_una_backend.Entities;

public class GaleriaInstitucionalItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Orden { get; set; }
}
