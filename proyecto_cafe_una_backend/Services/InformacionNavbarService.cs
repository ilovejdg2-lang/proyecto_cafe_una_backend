using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class InformacionNavbarService
{
    private readonly InformacionNavbar _navbar = new();
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<InformacionNavbar> ObtenerAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return Copiar(_navbar);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<InformacionNavbar> ActualizarAsync(ActualizarInformacionNavbarRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            if (cambios.LogoUrl is not null)
            {
                _navbar.LogoUrl = cambios.LogoUrl.Trim();
            }

            if (cambios.LogoClaroUrl is not null)
            {
                _navbar.LogoClaroUrl = cambios.LogoClaroUrl.Trim();
            }

            return Copiar(_navbar);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private static InformacionNavbar Copiar(InformacionNavbar navbar) => new()
    {
        LogoUrl = navbar.LogoUrl,
        LogoClaroUrl = navbar.LogoClaroUrl
    };
}
