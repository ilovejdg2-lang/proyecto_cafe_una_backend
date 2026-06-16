using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController(UsuariosService usuariosService, PerfilService perfilService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UsuarioPerfilResponse>> ObtenerPerfil()
    {
        var userId = ObtenerUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var perfil = await usuariosService.ObtenerPerfilAsync(userId.Value);
        if (perfil is null)
        {
            return NotFound();
        }

        return Ok(perfil);
    }

    [HttpPut]
    public async Task<ActionResult<UsuarioPerfilResponse>> ActualizarPerfil([FromBody] ActualizarPerfilRequest request)
    {
        var userId = ObtenerUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var perfil = await usuariosService.ActualizarPerfilAsync(userId.Value, request);
            if (perfil is null)
            {
                return NotFound();
            }

            return Ok(perfil);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("solicitar-cambio-correo")]
    public async Task<ActionResult> SolicitarCambioCorreo([FromBody] SolicitarCambioCorreoRequest request)
    {
        var userId = ObtenerUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await perfilService.SolicitarCambioCorreoAsync(
                userId.Value,
                request.NuevoCorreo,
                request.PasswordActual);
            if (!string.IsNullOrWhiteSpace(result.MensajeError))
            {
                return BadRequest(new { message = result.MensajeError });
            }

            return Ok(new
            {
                message = result.EmailEnviado
                    ? "Se envió un código de verificación al nuevo correo. Revise también la carpeta de spam."
                    : "Se generó el código, pero no se pudo enviar el correo. Intente de nuevo en unos minutos.",
                emailSent = result.EmailEnviado,
                requiresVerification = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("confirmar-cambio-correo")]
    public async Task<ActionResult<UsuarioPerfilResponse>> ConfirmarCambioCorreo([FromBody] ConfirmarCambioCorreoRequest request)
    {
        var userId = ObtenerUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var perfil = await perfilService.ConfirmarCambioCorreoAsync(userId.Value, request.NuevoCorreo, request.Token);
            return Ok(perfil);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("password")]
    public async Task<ActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
    {
        var userId = ObtenerUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var actualizado = await usuariosService.CambiarPasswordAsync(
                userId.Value,
                request.PasswordActual,
                request.PasswordNueva);

            if (!actualizado)
            {
                return BadRequest(new { message = "No se pudo actualizar la contraseña." });
            }

            return Ok(new { message = "Contraseña actualizada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int? ObtenerUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(sub, out var id) ? id : null;
    }
}
