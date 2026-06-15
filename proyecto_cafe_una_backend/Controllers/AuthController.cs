using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Helpers;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService, JwtSettings jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] AuthCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(credentials.Identifier) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            return BadRequest("Usuario y contraseña son requeridos.");
        }

        var usuario = await authService.AutenticarAsync(credentials.Identifier, credentials.Password);
        if (usuario is null)
        {
            return Unauthorized();
        }

        var token = TokenGenerator.GenerateToken(usuario, jwtSettings);
        return Ok(new LoginResponse { Token = token });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await authService.SolicitarRegistroAsync(request);
            if (!string.IsNullOrWhiteSpace(result.MensajeError))
            {
                return BadRequest(new { message = result.MensajeError });
            }

            return Ok(new
            {
                message = result.EmailEnviado
                    ? "Se envió el código de verificación al correo indicado. Revise también la carpeta de spam."
                    : "Se generó el código, pero no se pudo enviar el correo. Espere 3 minutos y use reenviar código, o contacte al administrador.",
                emailSent = result.EmailEnviado,
                requiresVerification = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-registration")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyRegistration([FromBody] VerifyRegistrationRequest request)
    {
        try
        {
            var usuario = await authService.ConfirmarRegistroAsync(request);
            return Ok(new
            {
                message = "Cuenta creada correctamente. Ya puede iniciar sesion.",
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                estado = usuario.Estado,
                roles = usuario.Roles
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var result = await authService.SolicitarRecuperacionAsync(request);
            if (!string.IsNullOrWhiteSpace(result.MensajeError))
            {
                return BadRequest(new { message = result.MensajeError });
            }

            if (!result.UsuarioEncontrado)
            {
                return Ok(new
                {
                    found = false,
                    message = "No hay ningún usuario con ese correo o nombre de usuario."
                });
            }

            return Ok(new
            {
                found = true,
                message = result.EmailEnviado
                    ? "Se envio el codigo de recuperacion al correo registrado."
                    : "Se genero el codigo de recuperacion.",
                emailSent = result.EmailEnviado
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await authService.RestablecerPasswordAsync(request);
        if (!success)
        {
            return BadRequest(new { message = "Token inválido/expirado o contraseña no válida." });
        }

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }
}
