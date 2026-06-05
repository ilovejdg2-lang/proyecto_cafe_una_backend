namespace proyecto_cafe_una_backend.Models;

public class CrearSolicitudVoluntariadoRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? TipoVoluntariado { get; set; }
    public string? Identificacion { get; set; }
    public string? Institucion { get; set; }
    public string? Pais { get; set; }
    public string? Modalidad { get; set; }
    public int? CantidadParticipantes { get; set; }
    public string? Residencia { get; set; }
    public string? Horario { get; set; }
    public string? Dias { get; set; }
    public string? Area { get; set; }
    public string? Descripcion { get; set; }
    public string? Motivacion { get; set; }
}

public class ActualizarSolicitudVoluntariadoRequest
{
    public string? UserId { get; set; }
    public string? FechaSolicitud { get; set; }
    public string? Estado { get; set; }
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? TipoVoluntariado { get; set; }
    public string? Identificacion { get; set; }
    public string? Institucion { get; set; }
    public string? Pais { get; set; }
    public string? Modalidad { get; set; }
    public int? CantidadParticipantes { get; set; }
    public string? Residencia { get; set; }
    public string? Horario { get; set; }
    public string? Dias { get; set; }
    public string? Area { get; set; }
    public string? Descripcion { get; set; }
    public string? Motivacion { get; set; }
}

public class EliminarSolicitudVoluntariadoRequest
{
    public List<string> ActorRoles { get; set; } = [];
}
