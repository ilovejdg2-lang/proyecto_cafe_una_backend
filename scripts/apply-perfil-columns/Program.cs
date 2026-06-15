using System.Text.Json;
using Npgsql;

static string? FindDevSettingsPath()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null)
    {
        var candidate = Path.Combine(dir.FullName, "proyecto_cafe_una_backend", "appsettings.Development.json");
        if (File.Exists(candidate))
        {
            return candidate;
        }

        candidate = Path.Combine(dir.FullName, "appsettings.Development.json");
        if (File.Exists(candidate))
        {
            return candidate;
        }

        dir = dir.Parent;
    }

    return null;
}

var devSettingsPath = FindDevSettingsPath();
if (devSettingsPath is null)
{
    Console.Error.WriteLine("No se encontró appsettings.Development.json.");
    return 1;
}

using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(devSettingsPath));
var connectionString = doc.RootElement
    .GetProperty("ConnectionStrings")
    .GetProperty("DefaultConnection")
    .GetString();

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Connection string vacía.");
    return 1;
}

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

const string sql = """
    ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS "FotoPerfilUrl" character varying(1000);
    ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS "FotoBannerUrl" character varying(1000);
    ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS "FotoPerfilPosicion" character varying(30);
    ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS "FotoBannerPosicion" character varying(30);
    CREATE TABLE IF NOT EXISTS cambios_correo_pendientes (
        "Id" SERIAL PRIMARY KEY,
        "UsuarioId" integer NOT NULL,
        "NuevoCorreo" character varying(200) NOT NULL,
        "Token" character varying(20) NOT NULL,
        "ExpiraEnUtc" timestamp with time zone NOT NULL,
        "Usado" boolean NOT NULL DEFAULT FALSE
    );
    CREATE TABLE IF NOT EXISTS usuarios_creacion_pendientes (
        "Id" SERIAL PRIMARY KEY,
        "Token" character varying(20) NOT NULL,
        "Correo" character varying(200) NOT NULL,
        "Nombre" character varying(200) NOT NULL,
        "PasswordHash" character varying(500) NOT NULL,
        "Roles" text[] NOT NULL DEFAULT ARRAY['Usuario']::text[],
        "ExpiraEnUtc" timestamp with time zone NOT NULL,
        "Usado" boolean NOT NULL DEFAULT FALSE
    );
    """;

await using (var cmd = new NpgsqlCommand(sql, conn))
{
    await cmd.ExecuteNonQueryAsync();
}

Console.WriteLine("Esquema de perfil aplicado en Supabase.");
return 0;
