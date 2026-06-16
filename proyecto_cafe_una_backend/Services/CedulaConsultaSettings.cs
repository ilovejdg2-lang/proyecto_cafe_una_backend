namespace proyecto_cafe_una_backend.Services;

public class CedulaConsultaSettings
{
    /// <summary>
    /// Proveedor: Apify, Verifik, Mock o None.
    /// </summary>
    public string Provider { get; set; } = "None";

    public string ApiKey { get; set; } = string.Empty;

    public string ApifyBaseUrl { get; set; } = "https://tse.apifycr.com/api/v2";

    /// <summary>
    /// Ruta opcional relativa a ApifyBaseUrl o URL absoluta. Ej: /consulta/{cedula}
    /// </summary>
    public string? ApifyConsultaPath { get; set; }

    /// <summary>
    /// Si Apify falla en Development, usa datos locales de respaldo (útil sin suscripción activa).
    /// </summary>
    public bool UseMockFallbackInDevelopment { get; set; } = true;

    /// <summary>
    /// Si Apify falla por suscripción/plan caído, usa datos de prueba en lugar de error 503.
    /// Útil en staging; en producción real conviene tener suscripción Apify activa.
    /// </summary>
    public bool UseMockFallbackWhenUnavailable { get; set; } = true;

    public string VerifikBaseUrl { get; set; } = "https://api.verifik.co";
}
