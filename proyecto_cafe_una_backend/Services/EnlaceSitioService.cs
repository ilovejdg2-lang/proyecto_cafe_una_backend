using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class EnlaceSitioService
{
    private readonly List<EnlaceSitio> _enlaces = [];
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<List<EnlaceSitio>> ObtenerTodosAsync(string? seccion = null)
    {
        await _mutex.WaitAsync();
        try
        {
            var query = _enlaces.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(seccion))
            {
                query = query.Where(e => e.Seccion.Equals(seccion.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query.Select(Copiar).OrderBy(e => e.Orden).ThenBy(e => e.Id).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<EnlaceSitio> CrearAsync(CrearEnlaceSitioRequest request)
    {
        await _mutex.WaitAsync();
        try
        {
            var enlace = new EnlaceSitio
            {
                Id = ObtenerSiguienteId(),
                Etiqueta = request.Etiqueta.Trim(),
                Ruta = request.Ruta.Trim(),
                Seccion = request.Seccion.Trim(),
                Orden = request.Orden ?? (_enlaces.Count == 0 ? 1 : _enlaces.Max(e => e.Orden) + 1),
                AbrirEnNuevaPestana = request.AbrirEnNuevaPestana
            };

            _enlaces.Add(enlace);
            return Copiar(enlace);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<EnlaceSitio?> ActualizarAsync(long id, ActualizarEnlaceSitioRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _enlaces.FindIndex(e => e.Id == id);
            if (index < 0)
            {
                return null;
            }

            var actual = _enlaces[index];

            if (!string.IsNullOrWhiteSpace(cambios.Etiqueta))
            {
                actual.Etiqueta = cambios.Etiqueta.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Ruta))
            {
                actual.Ruta = cambios.Ruta.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Seccion))
            {
                actual.Seccion = cambios.Seccion.Trim();
            }

            if (cambios.Orden.HasValue)
            {
                actual.Orden = cambios.Orden.Value;
            }

            if (cambios.AbrirEnNuevaPestana.HasValue)
            {
                actual.AbrirEnNuevaPestana = cambios.AbrirEnNuevaPestana.Value;
            }

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
            var index = _enlaces.FindIndex(e => e.Id == id);
            if (index < 0)
            {
                return false;
            }

            _enlaces.RemoveAt(index);
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private long ObtenerSiguienteId() =>
        _enlaces.Count == 0 ? 1 : _enlaces.Max(e => e.Id) + 1;

    private static EnlaceSitio Copiar(EnlaceSitio enlace) => new()
    {
        Id = enlace.Id,
        Etiqueta = enlace.Etiqueta,
        Ruta = enlace.Ruta,
        Seccion = enlace.Seccion,
        Orden = enlace.Orden,
        AbrirEnNuevaPestana = enlace.AbrirEnNuevaPestana
    };
}
