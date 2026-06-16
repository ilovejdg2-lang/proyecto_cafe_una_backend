using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class UsuariosAdminService(
    ApplicationDbContext db,
    UsuariosService usuariosService,
    EmailService emailService)
{
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan EmailCooldown = TimeSpan.FromMinutes(3);
    private const string MensajeEsperaCorreo = "No se puede mandar un correo seguido. Espera 3 minutos.";

    public async Task<CambioCorreoResult> SolicitarCreacionUsuarioAsync(SolicitarCreacionUsuarioRequest request)
    {
        var nombre = request.Nombre.Trim();
        var correo = request.Correo.Trim().ToLowerInvariant();
        var password = request.PasswordHash;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre es obligatorio.");
        }

        UsuarioValidacion.ValidarNombre(nombre);

        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new InvalidOperationException("El correo es obligatorio.");
        }

        UsuarioValidacion.ValidarPassword(password);

        if (await usuariosService.ExisteCorreoAsync(correo))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        if (await usuariosService.ExisteNombreAsync(nombre))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese nombre de usuario.");
        }

        var roles = request.Roles.Count == 0
            ? new List<string> { "Usuario" }
            : request.Roles.ToList();
        var now = DateTime.UtcNow;

        var pendienteActivo = await db.UsuariosCreacionPendientes
            .Where(entry => !entry.Usado && entry.ExpiraEnUtc > now && entry.Correo.ToLower() == correo)
            .OrderByDescending(entry => entry.ExpiraEnUtc)
            .FirstOrDefaultAsync();

        if (pendienteActivo is not null)
        {
            if (!string.Equals(pendienteActivo.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Ese correo ya tiene una creación en proceso.");
            }

            var mensajeEspera = ObtenerMensajeEsperaCorreo(pendienteActivo.ExpiraEnUtc);
            if (mensajeEspera is not null)
            {
                return new CambioCorreoResult { EmailEnviado = false, MensajeError = mensajeEspera };
            }
        }

        var entradasPrevias = await db.UsuariosCreacionPendientes
            .Where(entry =>
                entry.Usado ||
                entry.ExpiraEnUtc <= now ||
                entry.Correo.ToLower() == correo ||
                entry.Nombre.ToLower() == nombre.ToLowerInvariant())
            .ToListAsync();

        if (entradasPrevias.Count > 0)
        {
            db.UsuariosCreacionPendientes.RemoveRange(entradasPrevias);
        }

        var token = GenerarCodigo();
        db.UsuariosCreacionPendientes.Add(new UsuarioCreacionPendiente
        {
            Token = token,
            Correo = correo,
            Nombre = nombre,
            PasswordHash = password,
            Roles = roles,
            ExpiraEnUtc = now.Add(_tokenLifetime),
            Usado = false
        });

        await db.SaveChangesAsync();

        var emailEnviado = await emailService.EnviarCodigoCambioCorreoAsync(correo, nombre, token);
        return new CambioCorreoResult { EmailEnviado = emailEnviado };
    }

    public async Task<Usuario> ConfirmarCreacionUsuarioAsync(ConfirmarCreacionUsuarioRequest request)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        var codigo = request.Token.Trim();

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(codigo))
        {
            throw new InvalidOperationException("Correo y código son obligatorios.");
        }

        var now = DateTime.UtcNow;
        var entry = await db.UsuariosCreacionPendientes.FirstOrDefaultAsync(e =>
            !e.Usado &&
            e.ExpiraEnUtc > now &&
            e.Correo.ToLower() == correo &&
            e.Token == codigo);

        if (entry is null)
        {
            throw new InvalidOperationException("Código inválido o expirado.");
        }

        if (await usuariosService.ExisteCorreoAsync(correo))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        if (await usuariosService.ExisteNombreAsync(entry.Nombre))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese nombre de usuario.");
        }

        entry.Usado = true;
        await db.SaveChangesAsync();

        return await usuariosService.CrearAsync(new Usuario
        {
            Nombre = entry.Nombre,
            Correo = entry.Correo,
            PasswordHash = entry.PasswordHash,
            Roles = entry.Roles
        });
    }

    private static string GenerarCodigo() => Random.Shared.Next(100000, 999999).ToString();

    private static string? ObtenerMensajeEsperaCorreo(DateTime expiraEnUtc)
    {
        var segundosRestantes = (int)Math.Ceiling((expiraEnUtc - DateTime.UtcNow).TotalSeconds);
        if (segundosRestantes <= 0)
        {
            return null;
        }

        var minutosRestantes = (int)Math.Ceiling(segundosRestantes / 60.0);
        if (minutosRestantes >= EmailCooldown.TotalMinutes)
        {
            return null;
        }

        return $"{MensajeEsperaCorreo} Faltan {minutosRestantes} min.";
    }
}
