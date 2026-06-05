using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class VoluntariadoService
{
    private readonly List<SolicitudVoluntariado> _solicitudes = [];
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<List<SolicitudVoluntariado>> ObtenerSolicitudesAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _solicitudes.Select(Copiar).OrderBy(s => s.Id).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<List<SolicitudVoluntariado>> ObtenerSolicitudesDeUsuarioAsync(string userId)
    {
        var solicitudes = await ObtenerSolicitudesAsync();
        return solicitudes.Where(s => s.UserId == userId).ToList();
    }

    public async Task<SolicitudVoluntariado> CrearAsync(CrearSolicitudVoluntariadoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || request.UserId.Equals("anonimo", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La solicitud de voluntariado debe estar asociada a un usuario.");
        }

        await _mutex.WaitAsync();
        try
        {
            var solicitud = new SolicitudVoluntariado
            {
                Id = ObtenerSiguienteId(),
                UserId = request.UserId.Trim(),
                FechaSolicitud = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
                Estado = "Pendiente",
                Nombre = request.Nombre,
                Email = request.Email,
                Telefono = request.Telefono,
                TipoVoluntariado = request.TipoVoluntariado,
                Identificacion = request.Identificacion,
                Institucion = request.Institucion,
                Pais = request.Pais,
                Modalidad = request.Modalidad,
                CantidadParticipantes = request.CantidadParticipantes,
                Residencia = request.Residencia,
                Horario = request.Horario,
                Dias = request.Dias,
                Area = request.Area,
                Descripcion = request.Descripcion,
                Motivacion = request.Motivacion
            };

            _solicitudes.Add(solicitud);
            return Copiar(solicitud);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<SolicitudVoluntariado?> ActualizarAsync(long id, ActualizarSolicitudVoluntariadoRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _solicitudes.FindIndex(s => s.Id == id);
            if (index < 0)
            {
                return null;
            }

            var actual = _solicitudes[index];
            actual.UserId = string.IsNullOrWhiteSpace(cambios.UserId) ? actual.UserId : cambios.UserId.Trim();
            actual.FechaSolicitud = string.IsNullOrWhiteSpace(cambios.FechaSolicitud) ? actual.FechaSolicitud : cambios.FechaSolicitud;
            actual.Estado = string.IsNullOrWhiteSpace(cambios.Estado) ? actual.Estado : cambios.Estado;
            actual.Nombre = cambios.Nombre ?? actual.Nombre;
            actual.Email = cambios.Email ?? actual.Email;
            actual.Telefono = cambios.Telefono ?? actual.Telefono;
            actual.TipoVoluntariado = cambios.TipoVoluntariado ?? actual.TipoVoluntariado;
            actual.Identificacion = cambios.Identificacion ?? actual.Identificacion;
            actual.Institucion = cambios.Institucion ?? actual.Institucion;
            actual.Pais = cambios.Pais ?? actual.Pais;
            actual.Modalidad = cambios.Modalidad ?? actual.Modalidad;
            actual.CantidadParticipantes = cambios.CantidadParticipantes ?? actual.CantidadParticipantes;
            actual.Residencia = cambios.Residencia ?? actual.Residencia;
            actual.Horario = cambios.Horario ?? actual.Horario;
            actual.Dias = cambios.Dias ?? actual.Dias;
            actual.Area = cambios.Area ?? actual.Area;
            actual.Descripcion = cambios.Descripcion ?? actual.Descripcion;
            actual.Motivacion = cambios.Motivacion ?? actual.Motivacion;

            return Copiar(actual);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> EliminarAsync(long id)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _solicitudes.FindIndex(s => s.Id == id);
            if (index < 0)
            {
                return false;
            }

            _solicitudes.RemoveAt(index);
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private long ObtenerSiguienteId() =>
        _solicitudes.Count == 0 ? 1 : _solicitudes.Max(s => s.Id) + 1;

    private static SolicitudVoluntariado Copiar(SolicitudVoluntariado solicitud) => new()
    {
        Id = solicitud.Id,
        UserId = solicitud.UserId,
        FechaSolicitud = solicitud.FechaSolicitud,
        Estado = solicitud.Estado,
        Nombre = solicitud.Nombre,
        Email = solicitud.Email,
        Telefono = solicitud.Telefono,
        TipoVoluntariado = solicitud.TipoVoluntariado,
        Identificacion = solicitud.Identificacion,
        Institucion = solicitud.Institucion,
        Pais = solicitud.Pais,
        Modalidad = solicitud.Modalidad,
        CantidadParticipantes = solicitud.CantidadParticipantes,
        Residencia = solicitud.Residencia,
        Horario = solicitud.Horario,
        Dias = solicitud.Dias,
        Area = solicitud.Area,
        Descripcion = solicitud.Descripcion,
        Motivacion = solicitud.Motivacion
    };
}
