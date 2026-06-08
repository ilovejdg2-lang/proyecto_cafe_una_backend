using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class EnlaceSitioService
{
    private readonly List<EnlaceSitio> _enlaces = CrearEnlacesPorDefecto();
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

    private static List<EnlaceSitio> CrearEnlacesPorDefecto() =>
    [
        new EnlaceSitio { Id = 1, Etiqueta = "Sobre nosotros", Ruta = "/AboutUs", Seccion = "Navbar", Orden = 1 },
        new EnlaceSitio { Id = 2, Etiqueta = "Productos", Ruta = "/productos", Seccion = "Navbar", Orden = 2 },
        new EnlaceSitio { Id = 3, Etiqueta = "Voluntariado", Ruta = "/voluntariado/solicitar", Seccion = "Navbar", Orden = 3 },
        new EnlaceSitio { Id = 4, Etiqueta = "Nuestra Historia", Ruta = "/AboutUs", Seccion = "FooterExplorar", Orden = 1 },
        new EnlaceSitio { Id = 5, Etiqueta = "Tienda Online", Ruta = "/productos", Seccion = "FooterExplorar", Orden = 2 },
        new EnlaceSitio { Id = 6, Etiqueta = "Voluntariado", Ruta = "/voluntariado/solicitar", Seccion = "FooterExplorar", Orden = 3 },
        new EnlaceSitio { Id = 7, Etiqueta = "Mi Cuenta", Ruta = "/login", Seccion = "FooterExplorar", Orden = 4 }
    ];

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
