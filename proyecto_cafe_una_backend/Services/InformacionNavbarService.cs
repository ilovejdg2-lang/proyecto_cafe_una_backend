using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class InformacionNavbarService(ApplicationDbContext db)
{
    private const int SingletonId = 1;

    public async Task<InformacionNavbar> ObtenerAsync()
    {
        var navbar = await db.InformacionNavbar.AsNoTracking().FirstOrDefaultAsync(n => n.Id == SingletonId);
        if (navbar is null)
        {
            navbar = new InformacionNavbar { Id = SingletonId };
            db.InformacionNavbar.Add(navbar);
            await db.SaveChangesAsync();
        }

        return Copiar(navbar);
    }

    public async Task<InformacionNavbar> ActualizarAsync(ActualizarInformacionNavbarRequest cambios)
    {
        var navbar = await db.InformacionNavbar.FirstOrDefaultAsync(n => n.Id == SingletonId);
        if (navbar is null)
        {
            navbar = new InformacionNavbar { Id = SingletonId };
            db.InformacionNavbar.Add(navbar);
        }

        if (cambios.LogoUrl is not null)
        {
            navbar.LogoUrl = cambios.LogoUrl.Trim();
        }

        if (cambios.LogoClaroUrl is not null)
        {
            navbar.LogoClaroUrl = cambios.LogoClaroUrl.Trim();
        }

        await db.SaveChangesAsync();
        return Copiar(navbar);
    }

    private static InformacionNavbar Copiar(InformacionNavbar navbar) => new()
    {
        Id = navbar.Id,
        LogoUrl = navbar.LogoUrl,
        LogoClaroUrl = navbar.LogoClaroUrl
    };
}
