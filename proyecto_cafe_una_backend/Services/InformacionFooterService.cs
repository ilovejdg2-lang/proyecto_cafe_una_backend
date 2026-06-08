using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class InformacionFooterService
{
    private readonly InformacionFooter _footer = CrearFooterPorDefecto();
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<InformacionFooter> ObtenerAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return Copiar(_footer);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<InformacionFooter> ActualizarAsync(ActualizarInformacionFooterRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            if (cambios.FraseMarca is not null)
            {
                _footer.FraseMarca = cambios.FraseMarca.Trim();
            }

            if (cambios.Telefono is not null)
            {
                _footer.Telefono = cambios.Telefono.Trim();
            }

            if (cambios.Correo is not null)
            {
                _footer.Correo = cambios.Correo.Trim();
            }

            if (cambios.FacebookUrl is not null)
            {
                _footer.FacebookUrl = cambios.FacebookUrl.Trim();
            }

            if (cambios.InstagramUrl is not null)
            {
                _footer.InstagramUrl = cambios.InstagramUrl.Trim();
            }

            if (cambios.MapsUrl is not null)
            {
                _footer.MapsUrl = cambios.MapsUrl.Trim();
            }

            if (cambios.TextoCopyright is not null)
            {
                _footer.TextoCopyright = cambios.TextoCopyright.Trim();
            }

            return Copiar(_footer);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private static InformacionFooter CrearFooterPorDefecto() => new()
    {
        FraseMarca = "Frase",
        Telefono = "8599-7693",
        Correo = "cafeuna@una.cr",
        FacebookUrl = "https://www.facebook.com/p/Caf%C3%A9-UNA-100051575025767/",
        InstagramUrl = "https://www.instagram.com/cafeuna_/",
        MapsUrl = "https://www.google.com/maps/place/Finca+Experimental+Santa+Luc%C3%ADa+-+Universidad+Nacional/@10.0232346,-84.1121791,17z",
        TextoCopyright = "© 2026 Cafe UNA Todos los derechos reservados."
    };

    private static InformacionFooter Copiar(InformacionFooter footer) => new()
    {
        FraseMarca = footer.FraseMarca,
        Telefono = footer.Telefono,
        Correo = footer.Correo,
        FacebookUrl = footer.FacebookUrl,
        InstagramUrl = footer.InstagramUrl,
        MapsUrl = footer.MapsUrl,
        TextoCopyright = footer.TextoCopyright
    };
}
