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

    private async Task<CedulaConsultaResponse?> ConsultarGoMetaAsync(
        string cedula,
        CancellationToken cancellationToken)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.GoMetaBaseUrl)
            ? "https://apis.gometa.org/cedulas"
            : _settings.GoMetaBaseUrl.TrimEnd('/');

        var url = $"{baseUrl}/{Uri.EscapeDataString(cedula)}";

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
