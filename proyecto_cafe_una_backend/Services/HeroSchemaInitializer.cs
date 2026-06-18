using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using proyecto_cafe_una_backend.Data;

namespace proyecto_cafe_una_backend.Services;

public static class HeroSchemaInitializer
{
    private const string Sql = """
        ALTER TABLE hero_principal
            ADD COLUMN IF NOT EXISTS "Eyebrow" character varying(200) NOT NULL DEFAULT '';

        ALTER TABLE hero_principal
            ADD COLUMN IF NOT EXISTS "PrimaryButtonText" character varying(200) NOT NULL DEFAULT '';
        """;

    public static async Task EnsureAsync(ApplicationDbContext db, ILogger logger)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(Sql);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudieron verificar las columnas del hero en Supabase.");
            throw;
        }
    }
}
