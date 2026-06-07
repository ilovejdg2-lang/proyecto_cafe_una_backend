namespace proyecto_cafe_una_backend.Entities;

// Link del menu (navbar o footer). No es el texto de las secciones de la pagina principal.
public class EnlaceSitio
{
    public long Id { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string Seccion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool AbrirEnNuevaPestana { get; set; }
}
