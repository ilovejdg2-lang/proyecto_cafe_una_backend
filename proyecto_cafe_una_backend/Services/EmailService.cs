using System.Net;
using System.Net.Mail;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class EmailService(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<EmailService> logger)
{
    private const string CodeEmailTemplatePath = "Templates/Email/codigo-verificacion.html";

    public async Task<bool> EnviarCodigoRegistroAsync(string destinatario, string nombre, string codigo)
    {
        var subject = "Código de verificación - Café UNA";
        var body = await BuildCodeEmailAsync(
            nombre,
            "Verifica tu cuenta",
            "Usá este código para completar tu registro en Café UNA:",
            codigo,
            "El código vence en 30 minutos. Si no creaste esta cuenta, ignorá este correo.");

        return await EnviarAsync(destinatario, subject, body);
    }

    public async Task<bool> EnviarCodigoRecuperacionAsync(string destinatario, string nombre, string codigo)
    {
        var subject = "Código de recuperación de contraseña - Café UNA";
        var body = await BuildCodeEmailAsync(
            nombre,
            "Recuperación de contraseña",
            "Usá este código para restablecer tu contraseña:",
            codigo,
            "El código vence en 30 minutos. Si no solicitaste este cambio, ignorá este correo.");

        return await EnviarAsync(destinatario, subject, body);
    }

    public async Task<bool> EnviarCodigoCambioCorreoAsync(string destinatario, string nombre, string codigo)
    {
        var subject = "Verifica tu nuevo correo - Café UNA";
        var body = await BuildCodeEmailAsync(
            nombre,
            "Cambio de correo",
            "Usá este código para confirmar tu nuevo correo en Café UNA:",
            codigo,
            "El código vence en 30 minutos. Si no solicitaste este cambio, ignorá este correo.");

        return await EnviarAsync(destinatario, subject, body);
    }

    private async Task<bool> EnviarAsync(string destinatario, string subject, string htmlBody)
    {
        var settings = GetSettings();
        if (settings is null || string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            logger.LogWarning("SMTP no configurado. No se envió correo a {Destinatario}.", destinatario);
            return false;
        }

        try
        {
            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(settings.Username))
            {
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(destinatario);
            await client.SendMailAsync(message);
            logger.LogInformation("Correo enviado correctamente a {Destinatario}.", destinatario);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo enviar correo a {Destinatario}.", destinatario);
            return false;
        }
    }

    private SmtpSettings? GetSettings() => configuration.GetSection("Smtp").Get<SmtpSettings>();

    private async Task<string> LoadCodeEmailTemplateAsync()
    {
        var path = Path.Combine(environment.ContentRootPath, CodeEmailTemplatePath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"No se encontró la plantilla de correo en {path}.");
        }

        return await File.ReadAllTextAsync(path);
    }

    private async Task<string> BuildCodeEmailAsync(string nombre, string titulo, string mensaje, string codigo, string nota)
    {
        var saludo = string.IsNullOrWhiteSpace(nombre) ? "Hola" : $"Hola, {WebUtility.HtmlEncode(nombre)}";
        var tituloSeguro = WebUtility.HtmlEncode(titulo);
        var mensajeSeguro = WebUtility.HtmlEncode(mensaje);
        var codigoSeguro = WebUtility.HtmlEncode(codigo);
        var notaSegura = WebUtility.HtmlEncode(nota);

        var template = await LoadCodeEmailTemplateAsync();

        return template
            .Replace("{{saludo}}", saludo, StringComparison.Ordinal)
            .Replace("{{titulo}}", tituloSeguro, StringComparison.Ordinal)
            .Replace("{{mensaje}}", mensajeSeguro, StringComparison.Ordinal)
            .Replace("{{codigo}}", codigoSeguro, StringComparison.Ordinal)
            .Replace("{{nota}}", notaSegura, StringComparison.Ordinal);
    }
}
