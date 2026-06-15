using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection es requerido.");

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings es requerido.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JwtSettings:SecretKey es requerido.");
}

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // No bloquear endpoints [AllowAnonymous] si el cliente manda un token viejo o inválido.
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<UsuariosAdminService>();
builder.Services.AddScoped<PerfilService>();
builder.Services.AddScoped<VoluntariadoService>();
builder.Services.AddScoped<HeroService>();
builder.Services.AddScoped<TextoInstitucionalService>();
builder.Services.AddScoped<GaleriaInstitucionalService>();
builder.Services.AddScoped<InformacionFooterService>();
builder.Services.AddScoped<InformacionNavbarService>();
builder.Services.AddScoped<EnlaceSitioService>();
builder.Services.AddScoped<ProductosService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.Configure<CedulaConsultaSettings>(builder.Configuration.GetSection("CedulaConsulta"));
builder.Services.AddHttpClient<CedulaConsultaService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync("""
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
            """);
        logger.LogInformation("Conexion a Supabase establecida correctamente.");
    }
    catch (Exception ex)
    {
        
        logger.LogError(ex, "No se pudo conectar o migrar la base de datos de Supabase. La app continua sin migracion.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cafe UNA API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raiz "/"
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
