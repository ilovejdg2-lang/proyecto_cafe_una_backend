using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class ProductosService
{
    private const decimal IvaRate = 0.13m;
    private const string EstadoHabilitado = "Habilitado";
    private const string EstadoDeshabilitado = "Deshabilitado";

    private readonly List<Producto> _productos = [];
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _productos.Select(Copiar).OrderBy(p => p.Id).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Producto?> ObtenerPorIdAsync(long id)
    {
        await _mutex.WaitAsync();
        try
        {
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            return producto is null ? null : Copiar(producto);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Producto> CrearAsync(CrearProductoRequest request)
    {
        ValidarDatosProducto(request.Nombre, request.Descripcion, request.PrecioNormal, request.Stock);

        await _mutex.WaitAsync();
        try
        {
            var producto = new Producto
            {
                Id = ObtenerSiguienteId(),
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion.Trim(),
                Imagen = request.Imagen.Trim(),
                PrecioNormal = request.PrecioNormal,
                PrecioConIVA = CalcularPrecioConIVA(request.PrecioNormal),
                Stock = request.Stock,
                Estado = NormalizarEstado(request.Estado),
                Peso = request.Peso.Trim()
            };

            _productos.Add(producto);
            return Copiar(producto);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<Producto?> ActualizarAsync(long id, ActualizarProductoRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            var index = _productos.FindIndex(p => p.Id == id);
            if (index < 0)
            {
                return null;
            }

            var actual = _productos[index];

            if (!string.IsNullOrWhiteSpace(cambios.Nombre))
            {
                actual.Nombre = cambios.Nombre.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Descripcion))
            {
                actual.Descripcion = cambios.Descripcion.Trim();
            }

            if (cambios.Imagen is not null)
            {
                actual.Imagen = cambios.Imagen.Trim();
            }

            if (cambios.PrecioNormal.HasValue)
            {
                if (cambios.PrecioNormal.Value < 0)
                {
                    throw new InvalidOperationException("El precio normal no puede ser negativo.");
                }

                actual.PrecioNormal = cambios.PrecioNormal.Value;
                actual.PrecioConIVA = CalcularPrecioConIVA(actual.PrecioNormal);
            }
            else if (cambios.PrecioConIVA.HasValue)
            {
                actual.PrecioConIVA = cambios.PrecioConIVA.Value;
            }

            if (cambios.Stock.HasValue)
            {
                if (cambios.Stock.Value < 0)
                {
                    throw new InvalidOperationException("El stock no puede ser negativo.");
                }

                actual.Stock = cambios.Stock.Value;
            }

            if (cambios.Peso is not null)
            {
                actual.Peso = cambios.Peso.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Estado))
            {
                actual.Estado = NormalizarEstado(cambios.Estado);
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
            var index = _productos.FindIndex(p => p.Id == id);
            if (index < 0)
            {
                return false;
            }

            _productos.RemoveAt(index);
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<List<Producto>> AjustarStockAsync(IEnumerable<AjustarStockProductoItemRequest> items)
    {
        var solicitudes = (items ?? []).Where(item => item.Id > 0 && item.Units > 0).ToList();
        if (solicitudes.Count == 0)
        {
            return [];
        }

        await _mutex.WaitAsync();
        try
        {
            var actualizados = new List<Producto>();

            foreach (var solicitud in solicitudes)
            {
                var producto = _productos.FirstOrDefault(p => p.Id == solicitud.Id);
                if (producto is null)
                {
                    throw new InvalidOperationException($"No se encontró el producto con id {solicitud.Id}.");
                }

                if (producto.Estado == EstadoDeshabilitado)
                {
                    throw new InvalidOperationException($"El producto {producto.Nombre} está deshabilitado.");
                }

                if (producto.Stock < solicitud.Units)
                {
                    throw new InvalidOperationException($"No hay stock suficiente para {producto.Nombre}.");
                }

                producto.Stock -= solicitud.Units;
                actualizados.Add(Copiar(producto));
            }

            return actualizados;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private long ObtenerSiguienteId() =>
        _productos.Count == 0 ? 1 : _productos.Max(p => p.Id) + 1;

    private static decimal CalcularPrecioConIVA(decimal precioNormal) =>
        Math.Round(precioNormal * (1 + IvaRate), 0, MidpointRounding.AwayFromZero);

    private static string NormalizarEstado(string? estado) =>
        string.Equals(estado?.Trim(), EstadoDeshabilitado, StringComparison.OrdinalIgnoreCase)
            ? EstadoDeshabilitado
            : EstadoHabilitado;

    private static void ValidarDatosProducto(string nombre, string descripcion, decimal precioNormal, int stock)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new InvalidOperationException("La descripción del producto es obligatoria.");
        }

        if (precioNormal < 0)
        {
            throw new InvalidOperationException("El precio normal no puede ser negativo.");
        }

        if (stock < 0)
        {
            throw new InvalidOperationException("El stock no puede ser negativo.");
        }
    }

    private static Producto Copiar(Producto producto) => new()
    {
        Id = producto.Id,
        Nombre = producto.Nombre,
        Descripcion = producto.Descripcion,
        Imagen = producto.Imagen,
        PrecioNormal = producto.PrecioNormal,
        PrecioConIVA = producto.PrecioConIVA,
        Stock = producto.Stock,
        Estado = producto.Estado,
        Peso = producto.Peso
    };
}
