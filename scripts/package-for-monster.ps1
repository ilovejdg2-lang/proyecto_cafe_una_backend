param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDir
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$csproj = Join-Path $repoRoot 'proyecto_cafe_una_backend\proyecto_cafe_una_backend.csproj'
$projDir = Split-Path $csproj -Parent
$assemblyName = 'proyecto_cafe_una_backend'

if (-not (Test-Path $csproj)) {
    throw "No se encontro $csproj"
}

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null

$jwtIssuer = if ([string]::IsNullOrWhiteSpace($env:JWT_ISSUER)) { 'CafeUNA' } else { $env:JWT_ISSUER }
$jwtAudience = if ([string]::IsNullOrWhiteSpace($env:JWT_AUDIENCE)) { 'cafe-una-api' } else { $env:JWT_AUDIENCE }
$smtpHost = if ([string]::IsNullOrWhiteSpace($env:SMTP_HOST)) { 'smtp.gmail.com' } else { $env:SMTP_HOST }
$smtpPort = if ([string]::IsNullOrWhiteSpace($env:SMTP_PORT)) { 587 } else { [int]$env:SMTP_PORT }
$smtpFromName = if ([string]::IsNullOrWhiteSpace($env:SMTP_FROM_NAME)) { 'Cafe UNA' } else { $env:SMTP_FROM_NAME }
$cedulaProvider = if ([string]::IsNullOrWhiteSpace($env:CEDULA_API_KEY)) { 'None' } else { 'Apify' }

$settings = [ordered]@{
    ConnectionStrings = [ordered]@{
        DefaultConnection = $env:CONNECTION_STRING
    }
    JwtSettings = [ordered]@{
        Issuer    = $jwtIssuer
        Audience  = $jwtAudience
        SecretKey = $env:JWT_SECRET_KEY
    }
    Smtp = [ordered]@{
        Host      = $smtpHost
        Port      = $smtpPort
        Username  = $env:SMTP_USERNAME
        Password  = $env:SMTP_PASSWORD
        EnableSsl = $true
        FromEmail = $env:SMTP_FROM_EMAIL
        FromName  = $smtpFromName
    }
    CedulaConsulta = [ordered]@{
        Provider                     = $cedulaProvider
        ApiKey                       = $env:CEDULA_API_KEY
        ApifyBaseUrl                 = 'https://tse.apifycr.com/api/v2'
        VerifikBaseUrl               = 'https://api.verifik.co'
        UseMockFallbackInDevelopment = $false
    }
    Logging = [ordered]@{
        LogLevel = [ordered]@{
            Default                = 'Information'
            'Microsoft.AspNetCore' = 'Warning'
        }
    }
}

$productionSettingsPath = Join-Path $projDir 'appsettings.Production.json'
$settings | ConvertTo-Json -Depth 6 | Set-Content -Path $productionSettingsPath -Encoding utf8
Write-Host "Generado $productionSettingsPath"

Write-Host "Compilando $assemblyName (Release, win-x86) ..."
dotnet publish $csproj -c Release -o $PublishDir --runtime win-x86 --self-contained false
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish fallo con codigo $LASTEXITCODE"
}

$dllPath = Join-Path $PublishDir "$assemblyName.dll"
if (-not (Test-Path $dllPath)) {
    throw "No se encontro $dllPath despues del publish"
}

$devSettings = Join-Path $PublishDir 'appsettings.Development.json'
if (Test-Path $devSettings) {
    Remove-Item $devSettings -Force
    Write-Host 'Eliminado appsettings.Development.json del paquete.'
}

$webConfigPath = Join-Path $PublishDir 'web.config'
@'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore
        processPath="dotnet"
        arguments=".\{0}.dll"
        stdoutLogEnabled="true"
        stdoutLogFile=".\logs\stdout"
        hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
'@ -f $assemblyName | Set-Content -Path $webConfigPath -Encoding utf8

Write-Host "web.config generado para dotnet .\$assemblyName.dll"
Write-Host "Paquete listo en $PublishDir"
