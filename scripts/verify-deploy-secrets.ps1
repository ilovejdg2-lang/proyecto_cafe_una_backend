$ErrorActionPreference = 'Stop'

$required = @(
    'WEBSITE_NAME',
    'SERVER_COMPUTER_NAME',
    'SERVER_USERNAME',
    'SERVER_PASSWORD',
    'CONNECTION_STRING',
    'JWT_SECRET_KEY',
    'SITE_URL'
)

$missing = @($required | Where-Object { [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_)) })
if ($missing.Count -gt 0) {
    throw @"
Faltan Repository secrets en GitHub:
Settings -> Secrets and variables -> Actions -> Repository secrets

Faltan: $($missing -join ', ')
"@
}

Write-Host 'Todos los secrets obligatorios estan configurados.'
