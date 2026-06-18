namespace proyecto_cafe_una_backend.Services;

public class CedulaConsultaSettings
{
    /// <summary>
    /// Proveedor: GoMeta o None.
    /// </summary>
    public string Provider { get; set; } = "GoMeta";

    public string GoMetaBaseUrl { get; set; } = "https://apis.gometa.org/cedulas";
}
