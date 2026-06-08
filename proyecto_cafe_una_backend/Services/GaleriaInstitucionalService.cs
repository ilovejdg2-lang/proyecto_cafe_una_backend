using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class GaleriaInstitucionalService(ApplicationDbContext db)
{
    public async Task<List<GaleriaInstitucionalItem>> ObtenerTodosAsync() =>
        await db.GaleriaInstitucional
            .AsNoTracking()
            .OrderBy(item => item.Orden)
            .ThenBy(item => item.Id)
            .ToListAsync();

    public async Task<GaleriaInstitucionalItem> CrearAsync(CrearGaleriaInstitucionalItemRequest request)
    {
        var maxOrden = await db.GaleriaInstitucional.MaxAsync(g => (int?)g.Orden) ?? 0;
        var item = new GaleriaInstitucionalItem
        {
            Title = request.Title.Trim(),
            Image = request.Image.Trim(),
            Orden = request.Orden ?? (maxOrden + 1)
        };

        db.GaleriaInstitucional.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<GaleriaInstitucionalItem?> ActualizarAsync(long id, ActualizarGaleriaInstitucionalItemRequest cambios)
    {
        var actual = await db.GaleriaInstitucional.FirstOrDefaultAsync(item => item.Id == id);
        if (actual is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(cambios.Title)) actual.Title = cambios.Title.Trim();
        if (!string.IsNullOrWhiteSpace(cambios.Image)) actual.Image = cambios.Image.Trim();
        if (cambios.Orden.HasValue) actual.Orden = cambios.Orden.Value;

        await db.SaveChangesAsync();
        return actual;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var item = await db.GaleriaInstitucional.FirstOrDefaultAsync(g => g.Id == id);
        if (item is null)
        {
            return false;
        }

        db.GaleriaInstitucional.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }
}
