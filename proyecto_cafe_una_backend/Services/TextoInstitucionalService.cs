using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class TextoInstitucionalService(ApplicationDbContext db)
{
    private static readonly HashSet<string> ClavesValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "historia",
        "mission",
        "vision"
    };

    public bool EsClaveValida(string clave) => ClavesValidas.Contains(clave);

    public async Task<TextoInstitucional?> ObtenerAsync(string clave)
    {
        if (!EsClaveValida(clave))
        {
            return null;
        }

        var texto = await db.TextosInstitucionales
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Clave == clave.ToLowerInvariant());

        return texto is null ? null : Copiar(texto);
    }

    public async Task<TextoInstitucional?> ActualizarAsync(string clave, ActualizarTextoInstitucionalRequest cambios)
    {
        if (!EsClaveValida(clave))
        {
            return null;
        }

        var claveNormalizada = clave.ToLowerInvariant();
        var actual = await db.TextosInstitucionales.FirstOrDefaultAsync(t => t.Clave == claveNormalizada);
        if (actual is null)
        {
            actual = new TextoInstitucional { Clave = claveNormalizada };
            db.TextosInstitucionales.Add(actual);
        }

        if (!string.IsNullOrWhiteSpace(cambios.Title))
        {
            actual.Title = cambios.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(cambios.Description))
        {
            actual.Description = cambios.Description.Trim();
        }

        await db.SaveChangesAsync();
        return Copiar(actual);
    }

    private static TextoInstitucional Copiar(TextoInstitucional texto) => new()
    {
        Clave = texto.Clave,
        Title = texto.Title,
        Description = texto.Description
    };
}
