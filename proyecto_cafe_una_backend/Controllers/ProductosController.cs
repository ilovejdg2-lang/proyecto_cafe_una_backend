using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController(ProductosService productosService) : ControllerBase
{
    private static bool EsSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(rol => string.Equals(rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase)) == true;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> ObtenerProductos()
    {
        var productos = await productosService.ObtenerTodosAsync();
        return Ok(productos);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> CrearProducto([FromBody] CrearProductoRequest request)
    {
        try
        {
            var creado = await productosService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerProductos), new { id = creado.Id }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<Producto>> ActualizarProducto(long id, [FromBody] ActualizarProductoRequest cambios)
    {
        try
        {
            var actualizado = await productosService.ActualizarAsync(id, cambios);
            if (actualizado is null)
            {
                return NotFound();
            }

            return Ok(actualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("ajustar-stock")]
    public async Task<ActionResult<IEnumerable<Producto>>> AjustarStock([FromBody] List<AjustarStockProductoItemRequest> items)
    {
        try
        {
            var actualizados = await productosService.AjustarStockAsync(items);
            return Ok(actualizados);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> EliminarProducto(long id, [FromBody] EliminarProductoRequest? request)
    {
        if (!EsSuperAdmin(request?.ActorRoles))
        {
            return BadRequest(new { message = "Solo SuperAdmin puede eliminar productos." });
        }

        var deleted = await productosService.EliminarAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
