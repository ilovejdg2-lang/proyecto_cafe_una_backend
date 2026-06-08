using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class HeroService
{
    private readonly HeroPrincipal _hero = new();
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<HeroPrincipal> ObtenerAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return Copiar(_hero);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<HeroPrincipal> ActualizarAsync(ActualizarHeroRequest cambios)
    {
        await _mutex.WaitAsync();
        try
        {
            if (!string.IsNullOrWhiteSpace(cambios.Title))
            {
                _hero.Title = cambios.Title.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cambios.Subtitle))
            {
                _hero.Subtitle = cambios.Subtitle.Trim();
            }

            if (cambios.ButtonText is not null)
            {
                _hero.ButtonText = cambios.ButtonText.Trim();
            }

            if (cambios.BackgroundImage is not null)
            {
                _hero.BackgroundImage = cambios.BackgroundImage.Trim();
            }

            return Copiar(_hero);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private static HeroPrincipal Copiar(HeroPrincipal hero) => new()
    {
        Title = hero.Title,
        Subtitle = hero.Subtitle,
        ButtonText = hero.ButtonText,
        BackgroundImage = hero.BackgroundImage
    };
}
