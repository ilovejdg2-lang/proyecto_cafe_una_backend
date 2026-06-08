using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class VoluntariadoService(ApplicationDbContext db)
{
    public async Task<List<SolicitudVoluntariado>> ObtenerSolicitudesAsync() =>
        await db.SolicitudesVoluntariado
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync();

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

        var solicitud = new SolicitudVoluntariado
        {
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

        db.SolicitudesVoluntariado.Add(solicitud);
        await db.SaveChangesAsync();
        return solicitud;
    }

    public async Task<SolicitudVoluntariado?> ActualizarAsync(long id, ActualizarSolicitudVoluntariadoRequest cambios)
    {
        var actual = await db.SolicitudesVoluntariado.FirstOrDefaultAsync(s => s.Id == id);
        if (actual is null)
        {
            return null;
        }

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

        await db.SaveChangesAsync();
        return actual;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var solicitud = await db.SolicitudesVoluntariado.FirstOrDefaultAsync(s => s.Id == id);
        if (solicitud is null)
        {
            return false;
        }

        db.SolicitudesVoluntariado.Remove(solicitud);
        await db.SaveChangesAsync();
        return true;
    }
}
