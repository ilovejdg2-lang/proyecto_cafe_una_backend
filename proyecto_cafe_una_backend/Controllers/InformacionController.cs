using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/informacion")]
public class InformacionController(
    HeroService heroService,
    TextoInstitucionalService textoInstitucionalService,
    GaleriaInstitucionalService galeriaService,
    InformacionFooterService footerService,
    InformacionNavbarService navbarService,
    EnlaceSitioService enlaceSitioService,
    TarjetaInicioService tarjetaInicioService) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static bool EsSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(rol => string.Equals(rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase)) == true;

    [HttpGet]
    public async Task<ActionResult<object>> ObtenerInformacion()
    {
        var hero = await heroService.ObtenerAsync();
        var historia = await textoInstitucionalService.ObtenerAsync("historia");
        var mission = await textoInstitucionalService.ObtenerAsync("mission");
        var vision = await textoInstitucionalService.ObtenerAsync("vision");
        var gallery = await galeriaService.ObtenerTodosAsync();
        var footer = await footerService.ObtenerAsync();
        var navbar = await navbarService.ObtenerAsync();
        var enlaces = await enlaceSitioService.ObtenerTodosAsync();

        return Ok(new
        {
            hero,
            historia,
            mission,
            vision,
            gallery,
            footer,
            navbar,
            enlaces
        });
    }

    [HttpGet("hero")]
    public async Task<ActionResult<HeroPrincipal>> ObtenerHero()
    {
        var hero = await heroService.ObtenerAsync();
        return Ok(hero);
    }

    [HttpGet("{seccion}")]
    public async Task<IActionResult> ObtenerSeccion(string seccion)
    {
        if (seccion.Equals("hero", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(await heroService.ObtenerAsync());
        }

        if (seccion.Equals("gallery", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(await galeriaService.ObtenerTodosAsync());
        }

        if (textoInstitucionalService.EsClaveValida(seccion))
        {
            var texto = await textoInstitucionalService.ObtenerAsync(seccion);
            if (texto is null)
            {
                return Ok(new TextoInstitucional { Clave = seccion.ToLowerInvariant() });
            }

            return Ok(texto);
        }

        return NotFound();
    }

    [HttpPatch("{seccion}")]
    public async Task<IActionResult> ActualizarSeccion(string seccion, [FromBody] JsonElement cambios)
    {
        if (seccion.Equals("hero", StringComparison.OrdinalIgnoreCase))
        {
            var request = JsonSerializer.Deserialize<ActualizarHeroRequest>(cambios, JsonOptions);
            if (request is null)
            {
                return BadRequest(new { message = "Cuerpo de solicitud invalido." });
            }

            var hero = await heroService.ActualizarAsync(request);
            return Ok(hero);
        }

        if (textoInstitucionalService.EsClaveValida(seccion))
        {
            var request = JsonSerializer.Deserialize<ActualizarTextoInstitucionalRequest>(cambios, JsonOptions);
            if (request is null)
            {
                return BadRequest(new { message = "Cuerpo de solicitud invalido." });
            }

            var texto = await textoInstitucionalService.ActualizarAsync(seccion, request);
            return texto is null ? NotFound() : Ok(texto);
        }

        return NotFound();
    }

    [HttpPost("galeria")]
    public async Task<ActionResult<GaleriaInstitucionalItem>> CrearGaleriaItem([FromBody] CrearGaleriaInstitucionalItemRequest request)
    {
        var creado = await galeriaService.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerSeccion), new { seccion = "gallery" }, creado);
    }

    [HttpPut("galeria/{id:long}")]
    public async Task<ActionResult<GaleriaInstitucionalItem>> ActualizarGaleriaItem(long id, [FromBody] ActualizarGaleriaInstitucionalItemRequest cambios)
    {
        var actualizado = await galeriaService.ActualizarAsync(id, cambios);
        if (actualizado is null)
        {
            return NotFound();
        }

        return Ok(actualizado);
    }

    [HttpDelete("galeria/{id:long}")]
    public async Task<IActionResult> EliminarGaleriaItem(long id, [FromBody] EliminarGaleriaInstitucionalItemRequest? request)
    {
        if (!EsSuperAdmin(request?.ActorRoles))
        {
            return BadRequest(new { message = "Solo SuperAdmin puede eliminar items de la galeria." });
        }

        var deleted = await galeriaService.EliminarAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("tarjetas-inicio")]
    public async Task<ActionResult<IEnumerable<TarjetaInicio>>> ObtenerTarjetasInicio()
    {
        return Ok(await tarjetaInicioService.ObtenerTodasAsync());
    }

    [HttpPatch("tarjetas-inicio")]
    public async Task<ActionResult<IEnumerable<TarjetaInicio>>> ActualizarTarjetasInicio([FromBody] ActualizarTarjetasInicioRequest request)
    {
        var tarjetas = await tarjetaInicioService.ActualizarTodasAsync(request.Tarjetas);
        return Ok(tarjetas);
    }

    [HttpGet("navbar")]
    public async Task<ActionResult<InformacionNavbar>> ObtenerNavbar()
    {
        return Ok(await navbarService.ObtenerAsync());
    }

    [HttpPatch("navbar")]
    public async Task<ActionResult<InformacionNavbar>> ActualizarNavbar([FromBody] ActualizarInformacionNavbarRequest cambios)
    {
        return Ok(await navbarService.ActualizarAsync(cambios));
    }

    [HttpGet("footer")]
    public async Task<ActionResult<InformacionFooter>> ObtenerFooter()
    {
        return Ok(await footerService.ObtenerAsync());
    }

    [HttpPatch("footer")]
    public async Task<ActionResult<InformacionFooter>> ActualizarFooter([FromBody] ActualizarInformacionFooterRequest cambios)
    {
        return Ok(await footerService.ActualizarAsync(cambios));
    }

    [HttpGet("enlaces")]
    public async Task<ActionResult<IEnumerable<EnlaceSitio>>> ObtenerEnlaces([FromQuery] string? seccion)
    {
        return Ok(await enlaceSitioService.ObtenerTodosAsync(seccion));
    }

    [HttpPost("enlaces")]
    public async Task<ActionResult<EnlaceSitio>> CrearEnlace([FromBody] CrearEnlaceSitioRequest request)
    {
        var creado = await enlaceSitioService.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerEnlaces), creado);
    }

    [HttpPut("enlaces/{id:long}")]
    public async Task<ActionResult<EnlaceSitio>> ActualizarEnlace(long id, [FromBody] ActualizarEnlaceSitioRequest cambios)
    {
        var actualizado = await enlaceSitioService.ActualizarAsync(id, cambios);
        if (actualizado is null)
        {
            return NotFound();
        }

        return Ok(actualizado);
    }

    [HttpDelete("enlaces/{id:long}")]
    public async Task<IActionResult> EliminarEnlace(long id, [FromBody] EliminarEnlaceSitioRequest? request)
    {
        if (!EsSuperAdmin(request?.ActorRoles))
        {
            return BadRequest(new { message = "Solo SuperAdmin puede eliminar enlaces del sitio." });
        }

        var deleted = await enlaceSitioService.EliminarAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
