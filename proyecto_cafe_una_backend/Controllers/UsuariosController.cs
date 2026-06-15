using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(
    UsuariosService usuariosService,
    PerfilService perfilService,
    UsuariosAdminService usuariosAdminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerUsuarios()
    {
        var usuarios = await usuariosService.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerUsuariosActivos()
    {
        var usuarios = await usuariosService.ObtenerActivosAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> ObtenerUsuarioPorId(int id)
    {
        var usuario = await usuariosService.ObtenerPorIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    [HttpPost("solicitar-creacion")]
    public async Task<ActionResult> SolicitarCreacionUsuario([FromBody] SolicitarCreacionUsuarioRequest request)
    {
        try
        {
            var result = await usuariosAdminService.SolicitarCreacionUsuarioAsync(request);
            if (!string.IsNullOrWhiteSpace(result.MensajeError))
            {
                return BadRequest(new { message = result.MensajeError });
            }

            return Ok(new
            {
                message = result.EmailEnviado
                    ? "Se envió un código de verificación al correo indicado. Revise también la carpeta de spam."
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

    [HttpPost("confirmar-creacion")]
    public async Task<ActionResult<Usuario>> ConfirmarCreacionUsuario([FromBody] ConfirmarCreacionUsuarioRequest request)
    {
        try
        {
            var creado = await usuariosAdminService.ConfirmarCreacionUsuarioAsync(request);
            return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = creado.Id }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public ActionResult CrearUsuario([FromBody] Usuario nuevoUsuario)
    {
        return BadRequest(new
        {
            message = "Debe verificar el correo antes de crear el usuario. Use solicitar-creacion y confirmar-creacion."
        });
    }

    [HttpPut("{id:int}/solicitar-cambio-correo")]
    public async Task<ActionResult> SolicitarCambioCorreoUsuario(int id, [FromBody] SolicitarCambioCorreoRequest request)
    {
        try
        {
            var result = await perfilService.SolicitarCambioCorreoAsync(id, request.NuevoCorreo);
            if (!string.IsNullOrWhiteSpace(result.MensajeError))
            {
                return BadRequest(new { message = result.MensajeError });
            }

            return Ok(new
            {
                message = result.EmailEnviado
                    ? "Se envió un código de verificación al nuevo correo."
                    : "Se generó el código, pero no se pudo enviar el correo.",
                emailSent = result.EmailEnviado,
                requiresVerification = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/confirmar-cambio-correo")]
    public async Task<ActionResult<Usuario>> ConfirmarCambioCorreoUsuario(int id, [FromBody] ConfirmarCambioCorreoRequest request)
    {
        try
        {
            await perfilService.ConfirmarCambioCorreoAsync(id, request.NuevoCorreo, request.Token);
            var usuario = await usuariosService.ObtenerPorIdAsync(id);
            if (usuario is null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Usuario>> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioRequest cambios)
    {
        var payload = new Usuario
        {
            Nombre = cambios.Nombre,
            Correo = cambios.Correo,
            PasswordHash = cambios.PasswordHash,
            Estado = cambios.Estado,
            Roles = cambios.Roles
        };

        try
        {
            var actualizado = await usuariosService.ActualizarConActorAsync(
                id,
                payload,
                cambios.ActorId,
                cambios.ActorRoles,
                cambios.PasswordActual);
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

    [HttpPatch("{id:int}/estado")]
    public async Task<ActionResult<Usuario>> ToggleEstadoUsuario(int id, [FromBody] CambiarEstadoUsuarioRequest? request)
    {
        try
        {
            var actualizado = await usuariosService.ToggleEstadoAsync(id, request?.Estado, request?.ActorId, request?.ActorRoles);
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
}
