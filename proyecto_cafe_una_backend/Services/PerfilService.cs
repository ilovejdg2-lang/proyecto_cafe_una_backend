using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class PerfilService(
    ApplicationDbContext db,
    UsuariosService usuariosService,
    EmailService emailService)
{
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan EmailCooldown = TimeSpan.FromMinutes(3);
    private const string MensajeEsperaCorreo = "No se puede mandar un correo seguido. Espera 3 minutos.";

    public async Task<CambioCorreoResult> SolicitarCambioCorreoAsync(int usuarioId, string nuevoCorreo, string passwordActual)
    {
        var correo = nuevoCorreo.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new InvalidOperationException("El correo nuevo es obligatorio.");
        }

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario is null)
        {
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        UsuarioValidacion.ValidarPasswordActual(usuario.PasswordHash, passwordActual);

        if (string.Equals(usuario.Correo, correo, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Ese ya es su correo actual.");
        }

        if (await usuariosService.ExisteCorreoAsync(correo))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        var now = DateTime.UtcNow;
        var pendienteActivo = await db.CambiosCorreoPendientes
            .Where(entry =>
                !entry.Usado &&
                entry.ExpiraEnUtc > now &&
                entry.UsuarioId == usuarioId &&
                entry.NuevoCorreo.ToLower() == correo)
            .OrderByDescending(entry => entry.ExpiraEnUtc)
            .FirstOrDefaultAsync();

        if (pendienteActivo is not null)
        {
            var mensajeEspera = ObtenerMensajeEsperaCorreo(pendienteActivo.ExpiraEnUtc);
            if (mensajeEspera is not null)
            {
                return new CambioCorreoResult { EmailEnviado = false, MensajeError = mensajeEspera };
            }
        }

        var entradasPrevias = await db.CambiosCorreoPendientes
            .Where(entry =>
                entry.UsuarioId == usuarioId &&
                (entry.Usado || entry.ExpiraEnUtc <= now || entry.NuevoCorreo.ToLower() == correo))
            .ToListAsync();

        if (entradasPrevias.Count > 0)
        {
            db.CambiosCorreoPendientes.RemoveRange(entradasPrevias);
        }

        var token = GenerarCodigo();
        db.CambiosCorreoPendientes.Add(new CambioCorreoPendiente
        {
            UsuarioId = usuarioId,
            NuevoCorreo = correo,
            Token = token,
            ExpiraEnUtc = now.Add(_tokenLifetime),
            Usado = false
        });

        await db.SaveChangesAsync();

        var emailEnviado = await emailService.EnviarCodigoCambioCorreoAsync(correo, usuario.Nombre, token);
        return new CambioCorreoResult { EmailEnviado = emailEnviado };
    }

    public async Task<UsuarioPerfilResponse> ConfirmarCambioCorreoAsync(int usuarioId, string nuevoCorreo, string token)
    {
        var correo = nuevoCorreo.Trim().ToLowerInvariant();
        var codigo = token.Trim();

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(codigo))
        {
            throw new InvalidOperationException("Correo y código son obligatorios.");
        }

        var now = DateTime.UtcNow;
        var entry = await db.CambiosCorreoPendientes.FirstOrDefaultAsync(e =>
            !e.Usado &&
            e.ExpiraEnUtc > now &&
            e.UsuarioId == usuarioId &&
            e.NuevoCorreo.ToLower() == correo &&
            e.Token == codigo);

        if (entry is null)
        {
            throw new InvalidOperationException("Código inválido o expirado.");
        }

        if (await usuariosService.ExisteCorreoAsync(correo))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario is null)
        {
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        entry.Usado = true;
        usuario.Correo = correo;
        await db.SaveChangesAsync();

        var perfil = await usuariosService.ObtenerPerfilAsync(usuarioId);
        return perfil ?? throw new InvalidOperationException("No se pudo actualizar el perfil.");
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
