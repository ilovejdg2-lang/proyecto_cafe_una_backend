using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/cedula")]
public class CedulaController(CedulaConsultaService cedulaConsultaService) : ControllerBase
{
    [HttpGet("{numero}")]
    [AllowAnonymous]
    public async Task<IActionResult> Consultar(string numero, CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await cedulaConsultaService.ConsultarAsync(numero, cancellationToken);
            if (resultado is null)
            {
                return NotFound(new { message = "No se encontraron datos para esa cédula." });
            }

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("Espere", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("Demasiadas consultas", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new { message = ex.Message });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }
}
