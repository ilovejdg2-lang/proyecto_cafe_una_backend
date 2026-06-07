using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class TarjetaInicioService
{
    private readonly List<TarjetaInicio> _tarjetas = [];
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<List<TarjetaInicio>> ObtenerTodosAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _tarjetas.Select(Copiar).OrderBy(t => t.Orden).ThenBy(t => t.Id).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<TarjetaInicio> CrearAsync(CrearTarjetaInicioRequest request)
    {
        await _mutex.WaitAsync();
        try
        {
            var tarjeta = new TarjetaInicio
            {
                Id = ObtenerSiguienteId(),
                Clave = request.Clave.Trim(),
                Etiqueta = request.Etiqueta.Trim(),
                Titulo = request.Titulo.Trim(),
                Descripcion = request.Descripcion.Trim(),
                Ruta = string.IsNullOrWhiteSpace(request.Ruta) ? null : request.Ruta.Trim(),
                Orden = request.Orden ?? (_tarjetas.Count == 0 ? 1 : _tarjetas.Max(t => t.Orden) + 1)
            };

            _tarjetas.Add(tarjeta);
            return Copiar(tarjeta);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<TarjetaInicio?> ActualizarAsync(long id, ActualizarTarjetaInicioRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _tarjetas.FindIndex(t => t.Id == id);
            if (index < 0)
            {
                return null;
            }

            var actual = _tarjetas[index];

            if (!string.IsNullOrWhiteSpace(cambios.Clave))
            {
                actual.Clave = cambios.Clave.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Etiqueta))
            {
                actual.Etiqueta = cambios.Etiqueta.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Titulo))
            {
                actual.Titulo = cambios.Titulo.Trim();
            }

            if (cambios.Descripcion is not null)
            {
                actual.Descripcion = cambios.Descripcion.Trim();
            }

            if (cambios.Ruta is not null)
            {
                actual.Ruta = string.IsNullOrWhiteSpace(cambios.Ruta) ? null : cambios.Ruta.Trim();
            }

            if (cambios.Orden.HasValue)
            {
                actual.Orden = cambios.Orden.Value;
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
            var index = _tarjetas.FindIndex(t => t.Id == id);
            if (index < 0)
            {
                return false;
            }

            _tarjetas.RemoveAt(index);
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private long ObtenerSiguienteId() =>
        _tarjetas.Count == 0 ? 1 : _tarjetas.Max(t => t.Id) + 1;

    private static TarjetaInicio Copiar(TarjetaInicio tarjeta) => new()
    {
        Id = tarjeta.Id,
        Clave = tarjeta.Clave,
        Etiqueta = tarjeta.Etiqueta,
        Titulo = tarjeta.Titulo,
        Descripcion = tarjeta.Descripcion,
        Ruta = tarjeta.Ruta,
        Orden = tarjeta.Orden
    };
}
