# Ejecutar PowerShell como Administrador
Write-Host "Iniciando servicios de SQL Server..." -ForegroundColor Cyan

$servicios = @('MSSQLSERVER', 'SQLBrowser')
foreach ($nombre in $servicios) {
    $svc = Get-Service -Name $nombre -ErrorAction SilentlyContinue
    if ($null -eq $svc) {
        Write-Host "Servicio no encontrado: $nombre" -ForegroundColor Yellow
        continue
    }

    if ($svc.Status -ne 'Running') {
        Start-Service -Name $nombre
        Write-Host "Iniciado: $nombre" -ForegroundColor Green
    } else {
        Write-Host "Ya estaba activo: $nombre" -ForegroundColor Green
    }
}

$script = Join-Path $PSScriptRoot "script_crm.sql"
if (-not (Test-Path $script)) {
    Write-Host "No se encontro script_crm.sql en $PSScriptRoot" -ForegroundColor Red
    exit 1
}

Write-Host "Creando base de datos sge_crm (puede tardar unos minutos)..." -ForegroundColor Cyan
sqlcmd -S "." -i $script

if ($LASTEXITCODE -eq 0) {
    Write-Host "Base sge_crm lista. Reinicie la aplicacion SGE." -ForegroundColor Green
} else {
    Write-Host "Error al ejecutar script_crm.sql. Revise la salida de sqlcmd." -ForegroundColor Red
}
