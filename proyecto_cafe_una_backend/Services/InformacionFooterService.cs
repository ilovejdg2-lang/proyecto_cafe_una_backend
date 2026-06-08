using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class InformacionFooterService
{
    private readonly InformacionFooter _footer = new();
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
