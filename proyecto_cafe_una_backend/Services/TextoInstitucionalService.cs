using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class TextoInstitucionalService
{
    private static readonly HashSet<string> ClavesValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "historia",
        "mission",
        "vision"
    };

    private readonly Dictionary<string, TextoInstitucional> _textos = ClavesValidas.ToDictionary(
        clave => clave,
        _ => new TextoInstitucional(),
        StringComparer.OrdinalIgnoreCase
    );
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public bool EsClaveValida(string clave) => ClavesValidas.Contains(clave);

    public async Task<TextoInstitucional?> ObtenerAsync(string clave)
    {
        if (!EsClaveValida(clave))
        {
            return null;
        }

        await _mutex.WaitAsync();
        try
        {
            return Copiar(_textos[clave]);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<TextoInstitucional?> ActualizarAsync(string clave, ActualizarTextoInstitucionalRequest cambios)
    {
        if (!EsClaveValida(clave))
        {
            return null;
        }

        await _mutex.WaitAsync();
        try
        {
            var actual = _textos[clave];

            if (!string.IsNullOrWhiteSpace(cambios.Title))
            {
                actual.Title = cambios.Title.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Description))
            {
                actual.Description = cambios.Description.Trim();
            }

            return Copiar(actual);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private static TextoInstitucional Copiar(TextoInstitucional texto) => new()
    {
        Title = texto.Title,
        Description = texto.Description
    };
}
