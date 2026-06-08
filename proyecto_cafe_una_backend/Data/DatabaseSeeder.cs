using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Entities;

namespace proyecto_cafe_una_backend.Data;

public static class DatabaseSeeder
{
    private static readonly string[] ClavesTextoInstitucional = ["historia", "mission", "vision"];

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (!await db.Usuarios.AnyAsync())
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre = "Admin",
                Correo = "admin@cafeuna.com",
                PasswordHash = "admin123",
                Estado = "activo",
                Roles = ["SuperAdmin"]
            });
        }

        if (!await db.HeroPrincipal.AnyAsync())
        {
            db.HeroPrincipal.Add(new HeroPrincipal { Id = 1 });
        }

        if (!await db.InformacionNavbar.AnyAsync())
        {
            db.InformacionNavbar.Add(new InformacionNavbar { Id = 1 });
        }

        if (!await db.InformacionFooter.AnyAsync())
        {
            db.InformacionFooter.Add(new InformacionFooter { Id = 1 });
        }

        foreach (var clave in ClavesTextoInstitucional)
        {
            if (!await db.TextosInstitucionales.AnyAsync(t => t.Clave == clave))
            {
                db.TextosInstitucionales.Add(new TextoInstitucional { Clave = clave });
            }
        }

        if (!await db.EnlacesSitio.AnyAsync())
        {
            db.EnlacesSitio.AddRange(
                new EnlaceSitio { Id = 1, Etiqueta = "Sobre nosotros", Ruta = "/AboutUs", Seccion = "Navbar", Orden = 1 },
                new EnlaceSitio { Id = 2, Etiqueta = "Productos", Ruta = "/productos", Seccion = "Navbar", Orden = 2 },
                new EnlaceSitio { Id = 3, Etiqueta = "Voluntariado", Ruta = "/voluntariado/solicitar", Seccion = "Navbar", Orden = 3 },
                new EnlaceSitio { Id = 4, Etiqueta = "Nuestra Historia", Ruta = "/AboutUs", Seccion = "FooterExplorar", Orden = 1 },
                new EnlaceSitio { Id = 5, Etiqueta = "Tienda Online", Ruta = "/productos", Seccion = "FooterExplorar", Orden = 2 },
                new EnlaceSitio { Id = 6, Etiqueta = "Voluntariado", Ruta = "/voluntariado/solicitar", Seccion = "FooterExplorar", Orden = 3 },
                new EnlaceSitio { Id = 7, Etiqueta = "Mi Cuenta", Ruta = "/login", Seccion = "FooterExplorar", Orden = 4 }
            );
        }

        await db.SaveChangesAsync();

        if (await db.EnlacesSitio.AnyAsync())
        {
            await db.Database.ExecuteSqlRawAsync(
                """SELECT setval(pg_get_serial_sequence('enlaces_sitio', 'Id'), GREATEST((SELECT MAX("Id") FROM enlaces_sitio), 1))""");
        }
    }
}
