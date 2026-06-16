using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Entities;

namespace proyecto_cafe_una_backend.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<SolicitudVoluntariado> SolicitudesVoluntariado => Set<SolicitudVoluntariado>();
    public DbSet<TextoInstitucional> TextosInstitucionales => Set<TextoInstitucional>();
    public DbSet<HeroPrincipal> HeroPrincipal => Set<HeroPrincipal>();
    public DbSet<InformacionNavbar> InformacionNavbar => Set<InformacionNavbar>();
    public DbSet<InformacionFooter> InformacionFooter => Set<InformacionFooter>();
    public DbSet<EnlaceSitio> EnlacesSitio => Set<EnlaceSitio>();
    public DbSet<GaleriaInstitucionalItem> GaleriaInstitucional => Set<GaleriaInstitucionalItem>();
    public DbSet<PasswordResetEntry> PasswordResetEntries => Set<PasswordResetEntry>();
    public DbSet<RegistroPendiente> RegistrosPendientes => Set<RegistroPendiente>();
    public DbSet<CambioCorreoPendiente> CambiosCorreoPendientes => Set<CambioCorreoPendiente>();
    public DbSet<UsuarioCreacionPendiente> UsuariosCreacionPendientes => Set<UsuarioCreacionPendiente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("productos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Descripcion).HasMaxLength(2000).IsRequired();
            entity.Property(p => p.Imagen).HasMaxLength(1000);
            entity.Property(p => p.PrecioNormal).HasPrecision(12, 2);
            entity.Property(p => p.PrecioConIVA).HasPrecision(12, 2);
            entity.Property(p => p.Estado).HasMaxLength(20).HasDefaultValue("Habilitado");
            entity.Property(p => p.Peso).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Correo).HasMaxLength(200).IsRequired();
            entity.HasIndex(u => u.Correo).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(u => u.Estado).HasMaxLength(20).HasDefaultValue("activo");
            entity.Property(u => u.Roles);
            entity.Property(u => u.FotoPerfilUrl).HasMaxLength(1000);
            entity.Property(u => u.FotoBannerUrl).HasMaxLength(1000);
            entity.Property(u => u.FotoPerfilPosicion).HasMaxLength(30);
            entity.Property(u => u.FotoBannerPosicion).HasMaxLength(30);
        });

        modelBuilder.Entity<SolicitudVoluntariado>(entity =>
        {
            entity.ToTable("solicitudes_voluntariado");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedOnAdd();
            entity.Property(s => s.UserId).HasMaxLength(100).IsRequired();
            entity.Property(s => s.FechaSolicitud).HasMaxLength(20).IsRequired();
            entity.Property(s => s.Estado).HasMaxLength(30).HasDefaultValue("Pendiente");
            entity.Property(s => s.Nombre).HasMaxLength(200);
            entity.Property(s => s.Email).HasMaxLength(200);
            entity.Property(s => s.Telefono).HasMaxLength(50);
            entity.Property(s => s.TipoVoluntariado).HasMaxLength(100);
            entity.Property(s => s.Identificacion).HasMaxLength(100);
            entity.Property(s => s.Institucion).HasMaxLength(200);
            entity.Property(s => s.Pais).HasMaxLength(100);
            entity.Property(s => s.Modalidad).HasMaxLength(100);
            entity.Property(s => s.Residencia).HasMaxLength(200);
            entity.Property(s => s.Horario).HasMaxLength(100);
            entity.Property(s => s.Dias).HasMaxLength(200);
            entity.Property(s => s.Area).HasMaxLength(200);
            entity.Property(s => s.Descripcion).HasMaxLength(2000);
            entity.Property(s => s.Motivacion).HasMaxLength(2000);
        });

        modelBuilder.Entity<TextoInstitucional>(entity =>
        {
            entity.ToTable("textos_institucionales");
            entity.HasKey(t => t.Clave);
            entity.Property(t => t.Clave).HasMaxLength(50);
            entity.Property(t => t.Title).HasMaxLength(500);
            entity.Property(t => t.Description).HasMaxLength(4000);
            entity.Property(t => t.Image).HasMaxLength(1000);
        });

        modelBuilder.Entity<HeroPrincipal>(entity =>
        {
            entity.ToTable("hero_principal");
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Title).HasMaxLength(500);
            entity.Property(h => h.Subtitle).HasMaxLength(1000);
            entity.Property(h => h.ButtonText).HasMaxLength(200);
            entity.Property(h => h.BackgroundImage).HasMaxLength(1000);
        });

        modelBuilder.Entity<InformacionNavbar>(entity =>
        {
            entity.ToTable("informacion_navbar");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.LogoUrl).HasMaxLength(1000);
            entity.Property(n => n.LogoClaroUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<InformacionFooter>(entity =>
        {
            entity.ToTable("informacion_footer");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.LogoUrl).HasMaxLength(1000);
            entity.Property(f => f.LogoClaroUrl).HasMaxLength(1000);
            entity.Property(f => f.FraseMarca).HasMaxLength(500);
            entity.Property(f => f.Telefono).HasMaxLength(50);
            entity.Property(f => f.Correo).HasMaxLength(200);
            entity.Property(f => f.FacebookUrl).HasMaxLength(500);
            entity.Property(f => f.InstagramUrl).HasMaxLength(500);
            entity.Property(f => f.MapsUrl).HasMaxLength(500);
            entity.Property(f => f.TextoCopyright).HasMaxLength(500);
        });

        modelBuilder.Entity<EnlaceSitio>(entity =>
        {
            entity.ToTable("enlaces_sitio");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Etiqueta).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Ruta).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Seccion).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<GaleriaInstitucionalItem>(entity =>
        {
            entity.ToTable("galeria_institucional");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Id).ValueGeneratedOnAdd();
            entity.Property(g => g.Title).HasMaxLength(500).IsRequired();
            entity.Property(g => g.Image).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<PasswordResetEntry>(entity =>
        {
            entity.ToTable("password_reset_entries");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.Token).HasMaxLength(20).IsRequired();
            entity.Property(p => p.Correo).HasMaxLength(200).IsRequired();
            entity.HasIndex(p => p.Token);
        });

        modelBuilder.Entity<RegistroPendiente>(entity =>
        {
            entity.ToTable("registros_pendientes");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            entity.Property(r => r.Token).HasMaxLength(20).IsRequired();
            entity.Property(r => r.Correo).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(r => r.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(r => r.Correo);
            entity.HasIndex(r => r.Token);
        });

        modelBuilder.Entity<CambioCorreoPendiente>(entity =>
        {
            entity.ToTable("cambios_correo_pendientes");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.Property(c => c.UsuarioId).IsRequired();
            entity.Property(c => c.NuevoCorreo).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Token).HasMaxLength(20).IsRequired();
            entity.HasIndex(c => c.UsuarioId);
            entity.HasIndex(c => c.NuevoCorreo);
        });

        modelBuilder.Entity<UsuarioCreacionPendiente>(entity =>
        {
            entity.ToTable("usuarios_creacion_pendientes");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.Token).HasMaxLength(20).IsRequired();
            entity.Property(u => u.Correo).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(u => u.Roles);
            entity.HasIndex(u => u.Correo);
            entity.HasIndex(u => u.Token);
        });
    }
}
