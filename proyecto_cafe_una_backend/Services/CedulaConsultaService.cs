using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class CedulaConsultaService(
    HttpClient httpClient,
    IOptions<CedulaConsultaSettings> options,
    ILogger<CedulaConsultaService> logger)
{
    private readonly CedulaConsultaSettings _settings = options.Value;

    public async Task<CedulaConsultaResponse?> ConsultarAsync(string numero, CancellationToken cancellationToken = default)
    {
        var cedula = NormalizarCedula(numero);
        if (cedula is null)
        {
            throw new ArgumentException("Ingrese una cédula válida de 9 dígitos.");
        }

        var provider = (_settings.Provider ?? "GoMeta").Trim();

        return provider.ToLowerInvariant() switch
        {
            "gometa" => await ConsultarGoMetaAsync(cedula, cancellationToken),
            "none" or "" => throw new InvalidOperationException(
                "La consulta de cédula no está configurada. Agregue CedulaConsulta en appsettings."),
            _ => throw new InvalidOperationException($"Proveedor de cédula desconocido: {provider}.")
        };
    }

    private static string? NormalizarCedula(string numero)
    {
        var soloDigitos = Regex.Replace(numero ?? string.Empty, @"\D", string.Empty);
        return soloDigitos.Length == 9 ? soloDigitos : null;
    }

<<<<<<< HEAD
    private static CedulaConsultaResponse ConsultarMock(string cedula)
    {
        if (cedula == "504680314")
        {
            return new CedulaConsultaResponse
            {
                Cedula = cedula,
                Nombre = "Maria Del Mar Diaz Ruiz",
            };
        }

        return new CedulaConsultaResponse
        {
            Cedula = cedula,
            Nombre = "Nombre De Prueba Apellido1 Apellido2",
        };
    }

    private async Task<CedulaConsultaResponse?> ConsultarApifyConRespaldoAsync(
=======
    private async Task<CedulaConsultaResponse?> ConsultarGoMetaAsync(
>>>>>>> origin/development
        string cedula,
        CancellationToken cancellationToken)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.GoMetaBaseUrl)
            ? "https://apis.gometa.org/cedulas"
            : _settings.GoMetaBaseUrl.TrimEnd('/');

<<<<<<< HEAD
    private bool PuedeUsarRespaldoMock(InvalidOperationException ex)
    {
        if (!EsErrorDeServicioExterno(ex))
        {
            return false;
        }

        return _settings.UseMockFallbackWhenUnavailable
            || (_settings.UseMockFallbackInDevelopment && hostEnvironment.IsDevelopment());
    }

    private static bool EsErrorDeServicioExterno(InvalidOperationException ex) =>
        ex.Message.Contains("suscripci", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("plan", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("No se pudo consultar", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("No se pudo conectar", StringComparison.OrdinalIgnoreCase);

    private async Task<CedulaConsultaResponse?> ConsultarApifyAsync(string cedula, CancellationToken cancellationToken)
    {
        AsegurarApiKeyConfigurada();

        var baseUrl = ObtenerBaseUrlApify();
        var url = ConstruirUrlConsultaApify(baseUrl, cedula);
        const int maxReintentos = 3;

        for (var intento = 0; intento <= maxReintentos; intento++)
        {
            await EsperarYReservarConsultaApifyAsync(cancellationToken);

            using var request = CrearSolicitudApify(url);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var segundos = ObtenerRetryAfterSegundos(response);
                logger.LogWarning(
                    "Apify TSE rate limit (intento {Intento}/{Max}). Reintentando en {Segundos}s.",
                    intento + 1,
                    maxReintentos + 1,
                    segundos);

                if (intento >= maxReintentos)
                {
                    throw new InvalidOperationException(
                        $"Demasiadas consultas. Espere {segundos} segundos e intente de nuevo.");
                }

                await Task.Delay(TimeSpan.FromSeconds(segundos), cancellationToken);
                continue;
            }

            return ProcesarRespuestaApify(response, body, cedula);
        }

        throw new InvalidOperationException("No se pudo consultar la cédula en este momento.");
    }

    private string ObtenerBaseUrlApify()
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.ApifyBaseUrl)
            ? "https://tse.apifycr.com/api/v2"
            : _settings.ApifyBaseUrl.TrimEnd('/');

        if (baseUrl.EndsWith("/api/v2", StringComparison.OrdinalIgnoreCase))
        {
            return baseUrl;
        }

        if (baseUrl.Equals("https://tse.apifycr.com", StringComparison.OrdinalIgnoreCase))
        {
            return $"{baseUrl}/api/v2";
        }

        return baseUrl;
    }

    private string ConstruirUrlConsultaApify(string baseUrl, string cedula)
    {
        if (!string.IsNullOrWhiteSpace(_settings.ApifyConsultaPath))
        {
            var ruta = _settings.ApifyConsultaPath.Replace("{cedula}", cedula, StringComparison.Ordinal);
            return ruta.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? ruta
                : $"{baseUrl}{ruta}";
        }

        return $"{baseUrl}/consulta/{Uri.EscapeDataString(cedula)}";
    }

    private static JsonElement ExtraerDatosApify(JsonElement root)
    {
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
        {
            return data;
        }

        return root;
    }

    private static bool EsRespuestaApifyFallida(JsonElement root)
    {
        if (root.TryGetProperty("status", out var status)
            && status.ValueKind == JsonValueKind.String)
        {
            var valor = status.GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(valor)
                && !valor.Equals("success", StringComparison.OrdinalIgnoreCase)
                && !valor.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private CedulaConsultaResponse? ProcesarRespuestaApify(HttpResponseMessage response, string body, string cedula)
    {
        if (EsErrorDeRutaApify(body))
        {
            logger.LogWarning("Ruta Apify no encontrada: {Body}", body);
            throw new InvalidOperationException("No se pudo conectar con el servicio de consulta de cédula.");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out var errorElement))
        {
            var mensaje = errorElement.GetString() ?? "No se pudo consultar la cédula.";

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new InvalidOperationException(mensaje);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (EsRespuestaApifyFallida(root))
        {
            var mensaje = ObtenerMensajeErrorApify(root)
                ?? "No se pudo consultar la cédula en este momento.";
            throw new InvalidOperationException(mensaje);
        }

        if (!response.IsSuccessStatusCode)
        {
            var mensaje = ObtenerMensajeErrorApify(root)
                ?? "No se pudo consultar la cédula en este momento.";
            logger.LogWarning("Apify TSE respondió {StatusCode}: {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException(mensaje);
        }

        return MapearRespuestaApify(ExtraerDatosApify(root), cedula);
    }

    private static string? ObtenerMensajeErrorApify(JsonElement root)
    {
        if (root.TryGetProperty("message", out var messageElement)
            && messageElement.ValueKind == JsonValueKind.String)
        {
            var mensaje = messageElement.GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                return mensaje;
            }
        }

        return null;
    }

    private static async Task EsperarYReservarConsultaApifyAsync(CancellationToken cancellationToken)
    {
        await ApifyRateLock.WaitAsync(cancellationToken);
        try
        {
            var transcurrido = DateTime.UtcNow - _ultimaConsultaApifyUtc;
            var intervaloMinimo = TimeSpan.FromSeconds(5);

            if (transcurrido < intervaloMinimo)
            {
                await Task.Delay(intervaloMinimo - transcurrido, cancellationToken);
            }

            _ultimaConsultaApifyUtc = DateTime.UtcNow;
        }
        finally
        {
            ApifyRateLock.Release();
        }
    }

    private static int ObtenerRetryAfterSegundos(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta)
        {
            return Math.Max(1, (int)Math.Ceiling(delta.TotalSeconds));
        }

        if (response.Headers.RetryAfter?.Date is { } fecha)
        {
            var segundos = (int)Math.Ceiling((fecha - DateTimeOffset.UtcNow).TotalSeconds);
            return Math.Max(1, segundos);
        }

        return 5;
    }

    private HttpRequestMessage CrearSolicitudApify(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey.Trim());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private static bool EsErrorDeRutaApify(string body) =>
        body.Contains("could not be found", StringComparison.OrdinalIgnoreCase);

    private CedulaConsultaResponse? MapearRespuestaApify(JsonElement data, string cedula)
    {
        var nombre = ConstruirNombre(
            ObtenerTexto(data, "nombre"),
            ObtenerTexto(data, "apellido1") ?? ObtenerTexto(data, "primer_apellido"),
            ObtenerTexto(data, "apellido2") ?? ObtenerTexto(data, "segundo_apellido"));

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return null;
        }

        return new CedulaConsultaResponse
        {
            Cedula = ObtenerTexto(data, "cedula") ?? cedula,
            Nombre = nombre,
        };
    }

    private async Task<CedulaConsultaResponse?> ConsultarVerifikAsync(string cedula, CancellationToken cancellationToken)
    {
        AsegurarApiKeyConfigurada();

        var baseUrl = string.IsNullOrWhiteSpace(_settings.VerifikBaseUrl)
            ? "https://api.verifik.co"
            : _settings.VerifikBaseUrl.TrimEnd('/');

        var url = $"{baseUrl}/v2/cr/cedula?documentType=CCCR&documentNumber={Uri.EscapeDataString(cedula)}";
=======
        var url = $"{baseUrl}/{Uri.EscapeDataString(cedula)}";
>>>>>>> origin/development

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new InvalidOperationException(
                "Demasiadas consultas de cédula. Espere unos minutos e intente de nuevo.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("GoMeta respondió {StatusCode}: {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException("No se pudo consultar la cédula en este momento.");
        }

        using var document = JsonDocument.Parse(body);
        return MapearRespuestaGoMeta(document.RootElement, cedula);
    }

    private CedulaConsultaResponse? MapearRespuestaGoMeta(JsonElement root, string cedula)
    {
        if (root.TryGetProperty("resultcount", out var countElement)
            && countElement.ValueKind == JsonValueKind.Number
            && countElement.GetInt32() == 0)
        {
            return null;
        }

        if (!root.TryGetProperty("results", out var results)
            || results.ValueKind != JsonValueKind.Array
            || results.GetArrayLength() == 0)
        {
            var nombreRaiz = ObtenerTexto(root, "nombre");
            if (string.IsNullOrWhiteSpace(nombreRaiz))
            {
                return null;
            }

            return new CedulaConsultaResponse
            {
                Cedula = ObtenerTexto(root, "cedula") ?? cedula,
                Nombre = FormatearNombreDesdeApellidosPrimero(nombreRaiz)
            };
        }

        var persona = SeleccionarPersonaFisicaGoMeta(results, cedula);
        if (persona is null)
        {
            return null;
        }

        var nombre = ConstruirNombre(
            ObtenerTexto(persona.Value, "firstname") ?? ObtenerTexto(persona.Value, "firstname1"),
            ObtenerTexto(persona.Value, "lastname1"),
            ObtenerTexto(persona.Value, "lastname2"));

        if (string.IsNullOrWhiteSpace(nombre))
        {
            var nombreCompleto = ObtenerTexto(persona.Value, "fullname") ?? ObtenerTexto(root, "nombre");
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return null;
            }

            nombre = FormatearNombreDesdeApellidosPrimero(nombreCompleto);
        }

        return new CedulaConsultaResponse
        {
            Cedula = ObtenerTexto(persona.Value, "cedula") ?? ObtenerTexto(root, "cedula") ?? cedula,
            Nombre = nombre
        };
    }

    private static JsonElement? SeleccionarPersonaFisicaGoMeta(JsonElement results, string cedula)
    {
        JsonElement? coincidenciaExacta = null;
        JsonElement? primeraFisica = null;

        foreach (var item in results.EnumerateArray())
        {
            var tipo = ObtenerTexto(item, "guess_type") ?? ObtenerTexto(item, "type");
            var esFisica = string.Equals(tipo, "FISICA", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipo, "F", StringComparison.OrdinalIgnoreCase);

            if (!esFisica)
            {
                continue;
            }

            primeraFisica ??= item;

            var cedulaResultado = ObtenerTexto(item, "cedula");
            if (string.Equals(cedulaResultado, cedula, StringComparison.Ordinal))
            {
                coincidenciaExacta = item;
                break;
            }
        }

        return coincidenciaExacta ?? primeraFisica;
    }

    private static string FormatearNombreDesdeApellidosPrimero(string valor)
    {
        var partes = valor.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length <= 2)
        {
            return FormatearNombre(valor);
        }

        var apellidos = partes.Take(2).ToArray();
        var nombres = partes.Skip(2).ToArray();
        return ConstruirNombre(string.Join(' ', nombres), apellidos.ElementAtOrDefault(0), apellidos.ElementAtOrDefault(1));
    }

    private static string? ObtenerTexto(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()) ? null : value.GetString()!.Trim(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static string ConstruirNombre(string? nombre, string? apellido1, string? apellido2)
    {
        var partes = new[] { nombre, apellido1, apellido2 }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => FormatearNombre(p!));

        return string.Join(" ", partes);
    }

    private static string FormatearNombre(string valor)
    {
        var texto = valor.Trim().ToLowerInvariant();
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto);
    }
}
