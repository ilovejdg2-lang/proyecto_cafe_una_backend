using Microsoft.EntityFrameworkCore;
using proyecto_cafe_una_backend.Data;
using proyecto_cafe_una_backend.Entities;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class AuthService(
    UsuariosService usuariosService,
    ApplicationDbContext db,
    EmailService emailService)
{
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan EmailCooldown = TimeSpan.FromMinutes(3);
    private const string MensajeEsperaCorreo = "No se puede mandar un correo seguido. Espera 3 minutos.";

    public async Task<Usuario?> AutenticarAsync(string identifier, string password)
    {
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var usuario = await usuariosService.ObtenerPorNombreOCorreoAsync(identifier);
        if (usuario is null || !string.Equals(usuario.Estado, "activo", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return usuario.PasswordHash == password ? usuario : null;
    }

    public async Task<RegisterCodeResult> SolicitarRegistroAsync(RegisterRequest request)
    {
        var nombre = request.Nombre.Trim();
        var correo = request.Correo.Trim().ToLowerInvariant();
        var password = request.Password;

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

        var now = DateTime.UtcNow;
        var nombreNormalizado = nombre.ToLowerInvariant();

        var pendienteActivo = await db.RegistrosPendientes
            .Where(entry => !entry.Usado && entry.ExpiraEnUtc > now && entry.Correo.ToLower() == correo)
            .OrderByDescending(entry => entry.ExpiraEnUtc)
            .FirstOrDefaultAsync();

        if (pendienteActivo is not null)
        {
            if (!string.Equals(pendienteActivo.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Ese correo ya tiene un registro en proceso.");
            }

            var mensajeEspera = ObtenerMensajeEsperaCorreo(pendienteActivo.ExpiraEnUtc);
            if (mensajeEspera is not null)
            {
                return new RegisterCodeResult { EmailEnviado = false, MensajeError = mensajeEspera };
            }
        }

        var entradasPrevias = await db.RegistrosPendientes
            .Where(entry =>
                entry.Usado ||
                entry.ExpiraEnUtc <= now ||
                entry.Correo.ToLower() == correo ||
                entry.Nombre.ToLower() == nombreNormalizado)
            .ToListAsync();

        if (entradasPrevias.Count > 0)
        {
            db.RegistrosPendientes.RemoveRange(entradasPrevias);
        }

        var token = GenerarCodigo();
        db.RegistrosPendientes.Add(new RegistroPendiente
        {
            Token = token,
            Correo = correo,
            Nombre = nombre,
            PasswordHash = password,
            ExpiraEnUtc = now.Add(_tokenLifetime),
            Usado = false
        });

        await db.SaveChangesAsync();

        var emailEnviado = await emailService.EnviarCodigoRegistroAsync(correo, nombre, token);
        return new RegisterCodeResult
        {
            EmailEnviado = emailEnviado
        };
    }

    public async Task<Usuario> ConfirmarRegistroAsync(VerifyRegistrationRequest request)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        var token = request.Token.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Correo y código son obligatorios.");
        }

        if (await usuariosService.ExisteCorreoAsync(correo))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        var now = DateTime.UtcNow;
        var entry = await db.RegistrosPendientes.FirstOrDefaultAsync(e =>
            !e.Usado &&
            e.Correo.ToLower() == correo &&
            e.Token.ToUpper() == token &&
            e.ExpiraEnUtc > now);

        if (entry is null)
        {
            throw new InvalidOperationException("Código inválido o expirado.");
        }

        if (await usuariosService.ExisteNombreAsync(entry.Nombre))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese nombre de usuario.");
        }

        var nombreNormalizado = entry.Nombre.ToLowerInvariant();
        var nombreOcupadoEnPendiente = await db.RegistrosPendientes.AnyAsync(e =>
            !e.Usado &&
            e.ExpiraEnUtc > now &&
            e.Id != entry.Id &&
            e.Nombre.ToLower() == nombreNormalizado);

        if (nombreOcupadoEnPendiente)
        {
            throw new InvalidOperationException("Ese nombre de usuario ya está en uso.");
        }

        var usuario = await usuariosService.CrearAsync(new Usuario
        {
            Nombre = entry.Nombre,
            Correo = entry.Correo,
            PasswordHash = entry.PasswordHash,
            Estado = "activo",
            Roles = ["Usuario"]
        });

        entry.Usado = true;
        await db.SaveChangesAsync();
        return usuario;
    }

    public async Task<ForgotPasswordResult> SolicitarRecuperacionAsync(ForgotPasswordRequest request)
    {
        var identifier = request.Identifier.Trim();
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return new ForgotPasswordResult { UsuarioEncontrado = false };
        }

        var usuario = await usuariosService.ObtenerPorNombreOCorreoAsync(identifier);
        if (usuario is null || !string.Equals(usuario.Estado, "activo", StringComparison.OrdinalIgnoreCase))
        {
            return new ForgotPasswordResult { UsuarioEncontrado = false };
        }

        var now = DateTime.UtcNow;

        var recuperacionActiva = await db.PasswordResetEntries
            .Where(entry => !entry.Usado && entry.ExpiraEnUtc > now && entry.Correo.ToLower() == usuario.Correo.ToLower())
            .OrderByDescending(entry => entry.ExpiraEnUtc)
            .FirstOrDefaultAsync();

        if (recuperacionActiva is not null)
        {
            var mensajeEspera = ObtenerMensajeEsperaCorreo(recuperacionActiva.ExpiraEnUtc);
            if (mensajeEspera is not null)
            {
                return new ForgotPasswordResult
                {
                    UsuarioEncontrado = true,
                    EmailEnviado = false,
                    MensajeError = mensajeEspera
                };
            }
        }

        var entradasPrevias = await db.PasswordResetEntries
            .Where(entry =>
                entry.Usado ||
                entry.ExpiraEnUtc <= now ||
                entry.Correo.ToLower() == usuario.Correo.ToLower())
            .ToListAsync();

        if (entradasPrevias.Count > 0)
        {
            db.PasswordResetEntries.RemoveRange(entradasPrevias);
        }

        var token = GenerarCodigo();
        db.PasswordResetEntries.Add(new PasswordResetEntry
        {
            Token = token,
            Correo = usuario.Correo,
            ExpiraEnUtc = now.Add(_tokenLifetime),
            Usado = false
        });

        await db.SaveChangesAsync();

        var emailEnviado = await emailService.EnviarCodigoRecuperacionAsync(usuario.Correo, usuario.Nombre, token);
        return new ForgotPasswordResult
        {
            UsuarioEncontrado = true,
            EmailEnviado = emailEnviado
        };
    }

    public async Task<bool> RestablecerPasswordAsync(ResetPasswordRequest request)
    {
        var token = request.Token.Trim().ToUpperInvariant();
        var nuevaPassword = request.NuevaPassword;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        UsuarioValidacion.ValidarPassword(nuevaPassword);

        var now = DateTime.UtcNow;
        var entry = await db.PasswordResetEntries.FirstOrDefaultAsync(e =>
            !e.Usado &&
            e.Token.ToUpper() == token &&
            e.ExpiraEnUtc > now);

        if (entry is null)
        {
            return false;
        }

        var actualizado = await usuariosService.ActualizarPasswordPorCorreoAsync(entry.Correo, nuevaPassword);
        if (actualizado is null)
        {
            return false;
        }

        entry.Usado = true;
        await db.SaveChangesAsync();
        return true;
    }

    private static string GenerarCodigo() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    private string? ObtenerMensajeEsperaCorreo(DateTime expiraEnUtc)
    {
        var enviadoEn = expiraEnUtc - _tokenLifetime;
        var transcurrido = DateTime.UtcNow - enviadoEn;
        return transcurrido >= EmailCooldown ? null : MensajeEsperaCorreo;
    }
}
