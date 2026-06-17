using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class ProductosService(ApplicationDbContext db)
{
    private const decimal IvaRate = 0.13m;
    private const string EstadoHabilitado = "Habilitado";
    private const string EstadoDeshabilitado = "Deshabilitado";
    private const int MaxProductosDestacados = 3;

    public async Task<List<Producto>> ObtenerTodosAsync() =>
        await db.Productos
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

    public async Task<Producto?> ObtenerPorIdAsync(long id) =>
        await db.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Producto> CrearAsync(CrearProductoRequest request)
    {
        ValidarDatosProducto(request.Nombre, request.Descripcion, request.PrecioNormal, request.Stock);

        if (request.EsDestacado)
        {
            await ValidarLimiteDestacadosAsync(excluirId: null);
            ValidarProductoDestacable(NormalizarEstado(request.Estado), request.Stock);
        }

        var producto = new Producto
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim(),
            Imagen = request.Imagen.Trim(),
            PrecioNormal = request.PrecioNormal,
            PrecioConIVA = CalcularPrecioConIVA(request.PrecioNormal),
            Stock = request.Stock,
            Estado = NormalizarEstado(request.Estado),
            Peso = request.Peso.Trim(),
            EsDestacado = request.EsDestacado
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        return producto;
    }

    public async Task<Producto?> ActualizarAsync(long id, ActualizarProductoRequest cambios)
    {
        var actual = await db.Productos.FirstOrDefaultAsync(p => p.Id == id);
        if (actual is null)
        {
            return null;
        }

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

            if (cambios.Stock.Value <= 0 && actual.EsDestacado)
            {
                throw new InvalidOperationException("Quita el producto de destacados antes de dejar el stock en cero.");
            }

            actual.Stock = cambios.Stock.Value;

            if (actual.Stock <= 0)
            {
                actual.EsDestacado = false;
            }
        }

        if (cambios.Peso is not null)
        {
            actual.Peso = cambios.Peso.Trim();
        }

        if (!string.IsNullOrWhiteSpace(cambios.Estado))
        {
            var nuevoEstado = NormalizarEstado(cambios.Estado);
            if (nuevoEstado == EstadoDeshabilitado && actual.EsDestacado)
            {
                throw new InvalidOperationException("Quita el producto de destacados antes de deshabilitarlo.");
            }

            actual.Estado = nuevoEstado;
        }

        if (cambios.EsDestacado.HasValue)
        {
            if (cambios.EsDestacado.Value && !actual.EsDestacado)
            {
                await ValidarLimiteDestacadosAsync(excluirId: actual.Id);
                ValidarProductoDestacable(actual.Estado, actual.Stock);
            }

            actual.EsDestacado = cambios.EsDestacado.Value;
        }

        await db.SaveChangesAsync();
        return actual;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
        {
            return false;
        }

        db.Productos.Remove(producto);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Producto>> AjustarStockAsync(IEnumerable<AjustarStockProductoItemRequest> items)
    {
        var solicitudes = (items ?? []).Where(item => item.Id > 0 && item.Units > 0).ToList();
        if (solicitudes.Count == 0)
        {
            return [];
        }

        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var actualizados = new List<Producto>();

            foreach (var solicitud in solicitudes)
            {
                var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == solicitud.Id);
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
                if (producto.Stock <= 0)
                {
                    producto.EsDestacado = false;
                }
                actualizados.Add(producto);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return actualizados;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static decimal CalcularPrecioConIVA(decimal precioNormal) =>
        Math.Round(precioNormal * (1 + IvaRate), 0, MidpointRounding.AwayFromZero);

    private async Task ValidarLimiteDestacadosAsync(long? excluirId)
    {
        var query = db.Productos.Where(p => p.EsDestacado);
        if (excluirId.HasValue)
        {
            query = query.Where(p => p.Id != excluirId.Value);
        }

        var destacadosActuales = await query.CountAsync();
        if (destacadosActuales >= MaxProductosDestacados)
        {
            throw new InvalidOperationException($"Solo se pueden destacar hasta {MaxProductosDestacados} productos en el inicio.");
        }
    }

    private static void ValidarProductoDestacable(string estado, int stock)
    {
        if (estado == EstadoDeshabilitado)
        {
            throw new InvalidOperationException("No se puede destacar un producto deshabilitado.");
        }

        if (stock <= 0)
        {
            throw new InvalidOperationException("No se puede destacar un producto sin stock.");
        }
    }

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
}
