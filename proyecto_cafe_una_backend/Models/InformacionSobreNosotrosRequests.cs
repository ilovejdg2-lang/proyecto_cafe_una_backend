namespace proyecto_cafe_una_backend.Models;

public class ActualizarTextoInstitucionalRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class CrearGaleriaInstitucionalItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int? Orden { get; set; }
}

public class ActualizarGaleriaInstitucionalItemRequest
{
    public string? Title { get; set; }
    public string? Image { get; set; }
    public int? Orden { get; set; }
}

public class EliminarGaleriaInstitucionalItemRequest
{
    public List<string> ActorRoles { get; set; } = [];
}
