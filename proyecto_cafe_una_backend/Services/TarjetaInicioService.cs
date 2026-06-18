using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class TarjetaInicioService(ApplicationDbContext db)
{
    private static readonly HashSet<string> ClavesValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "donaciones",
        "visitas",
        "voluntariado"
    };

    private static readonly Dictionary<string, int> OrdenPorClave = new(StringComparer.OrdinalIgnoreCase)
    {
        ["donaciones"] = 1,
        ["visitas"] = 2,
        ["voluntariado"] = 3
    };

    public async Task<List<TarjetaInicio>> ObtenerTodasAsync() =>
        await db.TarjetasInicio
            .AsNoTracking()
            .OrderBy(t => t.Orden)
            .ToListAsync();

    public async Task<List<TarjetaInicio>> ActualizarTodasAsync(IEnumerable<ActualizarTarjetaInicioItemRequest> items)
    {
        foreach (var item in items ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.Clave))
            {
                continue;
            }

            var clave = item.Clave.Trim().ToLowerInvariant();
            if (!ClavesValidas.Contains(clave))
            {
                continue;
            }

            var actual = await db.TarjetasInicio.FirstOrDefaultAsync(t => t.Clave == clave);
            if (actual is null)
            {
                actual = new TarjetaInicio
                {
                    Clave = clave,
                    Orden = OrdenPorClave[clave]
                };
                db.TarjetasInicio.Add(actual);
            }

            if (!string.IsNullOrWhiteSpace(item.Etiqueta))
            {
                actual.Etiqueta = item.Etiqueta.Trim();
            }

            if (!string.IsNullOrWhiteSpace(item.Titulo))
            {
                actual.Titulo = item.Titulo.Trim();
            }

            if (!string.IsNullOrWhiteSpace(item.Descripcion))
            {
                actual.Descripcion = item.Descripcion.Trim();
            }

            if (item.Ruta is not null)
            {
                actual.Ruta = string.IsNullOrWhiteSpace(item.Ruta) ? null : item.Ruta.Trim();
            }

            if (item.TextoBoton is not null)
            {
                actual.TextoBoton = item.TextoBoton.Trim();
            }
        }

        await db.SaveChangesAsync();
        return await ObtenerTodasAsync();
    }
}
