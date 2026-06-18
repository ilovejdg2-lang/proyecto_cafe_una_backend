using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class HeroService(ApplicationDbContext db)
{
    private const int SingletonId = 1;

    public async Task<HeroPrincipal> ObtenerAsync()
    {
        var hero = await db.HeroPrincipal.AsNoTracking().FirstOrDefaultAsync(h => h.Id == SingletonId);
        if (hero is null)
        {
            hero = new HeroPrincipal { Id = SingletonId };
            db.HeroPrincipal.Add(hero);
            await db.SaveChangesAsync();
        }

        return Copiar(hero);
    }

    public async Task<HeroPrincipal> ActualizarAsync(ActualizarHeroRequest cambios)
    {
        var hero = await db.HeroPrincipal.FirstOrDefaultAsync(h => h.Id == SingletonId);
        if (hero is null)
        {
            hero = new HeroPrincipal { Id = SingletonId };
            db.HeroPrincipal.Add(hero);
        }

        if (cambios.Eyebrow is not null)
        {
            hero.Eyebrow = cambios.Eyebrow.Trim();
        }

        if (!string.IsNullOrWhiteSpace(cambios.Title))
        {
            hero.Title = cambios.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(cambios.Subtitle))
        {
            hero.Subtitle = cambios.Subtitle.Trim();
        }

        if (cambios.PrimaryButtonText is not null)
        {
            hero.PrimaryButtonText = cambios.PrimaryButtonText.Trim();
        }

        if (cambios.PrimaryButtonUrl is not null)
        {
            hero.PrimaryButtonUrl = cambios.PrimaryButtonUrl.Trim();
        }

        if (cambios.ButtonText is not null)
        {
            hero.ButtonText = cambios.ButtonText.Trim();
        }

        if (cambios.ButtonUrl is not null)
        {
            hero.ButtonUrl = cambios.ButtonUrl.Trim();
        }

        if (cambios.BackgroundImage is not null)
        {
            hero.BackgroundImage = cambios.BackgroundImage.Trim();
        }

        await db.SaveChangesAsync();
        return Copiar(hero);
    }

    private static HeroPrincipal Copiar(HeroPrincipal hero) => new()
    {
        Id = hero.Id,
        Eyebrow = hero.Eyebrow,
        Title = hero.Title,
        Subtitle = hero.Subtitle,
        PrimaryButtonText = hero.PrimaryButtonText,
        PrimaryButtonUrl = hero.PrimaryButtonUrl,
        ButtonText = hero.ButtonText,
        ButtonUrl = hero.ButtonUrl,
        BackgroundImage = hero.BackgroundImage
    };
}
