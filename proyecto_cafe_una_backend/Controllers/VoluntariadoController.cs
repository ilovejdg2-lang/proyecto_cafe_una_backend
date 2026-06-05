using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/voluntariado/solicitudes")]
public class VoluntariadoController(VoluntariadoService voluntariadoService) : ControllerBase
{
    private static bool EsSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(rol => string.Equals(rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase)) == true;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudVoluntariado>>> ObtenerSolicitudes()
    {
        var solicitudes = await voluntariadoService.ObtenerSolicitudesAsync();
        return Ok(solicitudes);
    }

    [HttpGet("usuario/{userId}")]
    public async Task<ActionResult<IEnumerable<SolicitudVoluntariado>>> ObtenerSolicitudesDeUsuario(string userId)
    {
        var solicitudes = await voluntariadoService.ObtenerSolicitudesDeUsuarioAsync(userId);
        return Ok(solicitudes);
    }

    [HttpPost]
    public async Task<ActionResult<SolicitudVoluntariado>> CrearSolicitud([FromBody] CrearSolicitudVoluntariadoRequest request)
    {
        try
        {
            var creada = await voluntariadoService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerSolicitudes), new { id = creada.Id }, creada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<SolicitudVoluntariado>> ActualizarSolicitud(long id, [FromBody] ActualizarSolicitudVoluntariadoRequest cambios)
    {
        var actualizada = await voluntariadoService.ActualizarAsync(id, cambios);
        if (actualizada is null)
        {
            return NotFound();
        }

        return Ok(actualizada);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> EliminarSolicitud(long id, [FromBody] EliminarSolicitudVoluntariadoRequest? request)
    {
        if (!EsSuperAdmin(request?.ActorRoles))
        {
            return BadRequest(new { message = "Solo SuperAdmin puede eliminar solicitudes de voluntariado." });
        }

        var deleted = await voluntariadoService.EliminarAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
