using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class GaleriaInstitucionalService
{
    private readonly List<GaleriaInstitucionalItem> _items = [];
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<List<GaleriaInstitucionalItem>> ObtenerTodosAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _items.Select(Copiar).OrderBy(item => item.Orden).ThenBy(item => item.Id).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<GaleriaInstitucionalItem> CrearAsync(CrearGaleriaInstitucionalItemRequest request)
    {
        await _mutex.WaitAsync();
        try
        {
            var item = new GaleriaInstitucionalItem
            {
                Id = ObtenerSiguienteId(),
                Title = request.Title.Trim(),
                Image = request.Image.Trim(),
                Orden = request.Orden ?? (_items.Count == 0 ? 1 : _items.Max(g => g.Orden) + 1)
            };

            _items.Add(item);
            return Copiar(item);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<GaleriaInstitucionalItem?> ActualizarAsync(long id, ActualizarGaleriaInstitucionalItemRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return null;
            }

            var actual = _items[index];

            if (!string.IsNullOrWhiteSpace(cambios.Title))
            {
                actual.Title = cambios.Title.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Image))
            {
                actual.Image = cambios.Image.Trim();
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
            var index = _items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return false;
            }

            _items.RemoveAt(index);
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private long ObtenerSiguienteId() =>
        _items.Count == 0 ? 1 : _items.Max(item => item.Id) + 1;

    private static GaleriaInstitucionalItem Copiar(GaleriaInstitucionalItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Image = item.Image,
        Orden = item.Orden
    };
}
