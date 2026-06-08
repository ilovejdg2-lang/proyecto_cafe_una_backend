using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class InformacionFooterService(ApplicationDbContext db)
{
    private const int SingletonId = 1;

    public async Task<InformacionFooter> ObtenerAsync()
    {
        var footer = await db.InformacionFooter.AsNoTracking().FirstOrDefaultAsync(f => f.Id == SingletonId);
        if (footer is null)
        {
            footer = new InformacionFooter { Id = SingletonId };
            db.InformacionFooter.Add(footer);
            await db.SaveChangesAsync();
        }

        return Copiar(footer);
    }

    public async Task<InformacionFooter> ActualizarAsync(ActualizarInformacionFooterRequest cambios)
    {
        var footer = await db.InformacionFooter.FirstOrDefaultAsync(f => f.Id == SingletonId);
        if (footer is null)
        {
            footer = new InformacionFooter { Id = SingletonId };
            db.InformacionFooter.Add(footer);
        }

        if (cambios.LogoUrl is not null) footer.LogoUrl = cambios.LogoUrl.Trim();
        if (cambios.LogoClaroUrl is not null) footer.LogoClaroUrl = cambios.LogoClaroUrl.Trim();
        if (cambios.FraseMarca is not null) footer.FraseMarca = cambios.FraseMarca.Trim();
        if (cambios.Telefono is not null) footer.Telefono = cambios.Telefono.Trim();
        if (cambios.Correo is not null) footer.Correo = cambios.Correo.Trim();
        if (cambios.FacebookUrl is not null) footer.FacebookUrl = cambios.FacebookUrl.Trim();
        if (cambios.InstagramUrl is not null) footer.InstagramUrl = cambios.InstagramUrl.Trim();
        if (cambios.MapsUrl is not null) footer.MapsUrl = cambios.MapsUrl.Trim();
        if (cambios.TextoCopyright is not null) footer.TextoCopyright = cambios.TextoCopyright.Trim();

        await db.SaveChangesAsync();
        return Copiar(footer);
    }

    private static InformacionFooter Copiar(InformacionFooter footer) => new()
    {
        Id = footer.Id,
        LogoUrl = footer.LogoUrl,
        LogoClaroUrl = footer.LogoClaroUrl,
        FraseMarca = footer.FraseMarca,
        Telefono = footer.Telefono,
        Correo = footer.Correo,
        FacebookUrl = footer.FacebookUrl,
        InstagramUrl = footer.InstagramUrl,
        MapsUrl = footer.MapsUrl,
        TextoCopyright = footer.TextoCopyright
    };
}
