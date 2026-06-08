using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class EnlaceSitioService(ApplicationDbContext db)
{
    public async Task<List<EnlaceSitio>> ObtenerTodosAsync(string? seccion = null)
    {
        var query = db.EnlacesSitio.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(seccion))
        {
            var seccionNormalizada = seccion.Trim();
            query = query.Where(e => e.Seccion.ToLower() == seccionNormalizada.ToLower());
        }

        return await query
            .OrderBy(e => e.Orden)
            .ThenBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<EnlaceSitio> CrearAsync(CrearEnlaceSitioRequest request)
    {
        var maxOrden = await db.EnlacesSitio.MaxAsync(e => (int?)e.Orden) ?? 0;
        var enlace = new EnlaceSitio
        {
            Etiqueta = request.Etiqueta.Trim(),
            Ruta = request.Ruta.Trim(),
            Seccion = request.Seccion.Trim(),
            Orden = request.Orden ?? (maxOrden + 1),
            AbrirEnNuevaPestana = request.AbrirEnNuevaPestana
        };

        db.EnlacesSitio.Add(enlace);
        await db.SaveChangesAsync();
        return enlace;
    }

    public async Task<EnlaceSitio?> ActualizarAsync(long id, ActualizarEnlaceSitioRequest cambios)
    {
        var actual = await db.EnlacesSitio.FirstOrDefaultAsync(e => e.Id == id);
        if (actual is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(cambios.Etiqueta)) actual.Etiqueta = cambios.Etiqueta.Trim();
        if (!string.IsNullOrWhiteSpace(cambios.Ruta)) actual.Ruta = cambios.Ruta.Trim();
        if (!string.IsNullOrWhiteSpace(cambios.Seccion)) actual.Seccion = cambios.Seccion.Trim();
        if (cambios.Orden.HasValue) actual.Orden = cambios.Orden.Value;
        if (cambios.AbrirEnNuevaPestana.HasValue) actual.AbrirEnNuevaPestana = cambios.AbrirEnNuevaPestana.Value;

        await db.SaveChangesAsync();
        return actual;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var enlace = await db.EnlacesSitio.FirstOrDefaultAsync(e => e.Id == id);
        if (enlace is null)
        {
            return false;
        }

        db.EnlacesSitio.Remove(enlace);
        await db.SaveChangesAsync();
        return true;
    }
}
