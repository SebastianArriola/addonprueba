<#
.SYNOPSIS
    Compila AddonPrueba en Release|x64 y genera el paquete ZIP listo para el Extension Manager de SAP B1.

.DESCRIPTION
    Uso:
        .\Build-Package.ps1                  # Release
        .\Build-Package.ps1 -Configuration Debug

    El ZIP resultante (AddonPrueba.zip) contiene:
        AddonPrueba.exe
        AddonPrueba.b1s
    y puede importarse directamente desde el Extension Manager de SAP Business One 10.0.

.NOTES
    Requisitos:
        - Visual Studio 2019/2022 o Build Tools para .NET Framework 4.8 instalados.
        - La DLL SAPbouiCOM.dll debe estar en AddonPrueba\lib\ antes de compilar.
          (ver AddonPrueba\lib\README.md)
#>

param(
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Rutas ──────────────────────────────────────────────────────────────────────
$ScriptDir  = $PSScriptRoot
$ProjectDir = Join-Path $ScriptDir "AddonPrueba"
$CsprojPath = Join-Path $ProjectDir "AddonPrueba.csproj"
$OutputDir  = Join-Path $ProjectDir "bin\$Configuration"
$PackageDir = Join-Path $ScriptDir  "package_temp"
$ZipPath    = Join-Path $ScriptDir  "AddonPrueba.zip"

# ── Buscar MSBuild ─────────────────────────────────────────────────────────────
function Find-MSBuild {
    # 1. Visual Studio 2022
    $vs2022 = "${env:ProgramFiles}\Microsoft Visual Studio\2022"
    foreach ($edition in @("Enterprise","Professional","Community","BuildTools")) {
        $msbuild = "$vs2022\$edition\MSBuild\Current\Bin\amd64\MSBuild.exe"
        if (Test-Path $msbuild) { return $msbuild }
    }
    # 2. Visual Studio 2019
    $vs2019 = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019"
    foreach ($edition in @("Enterprise","Professional","Community","BuildTools")) {
        $msbuild = "$vs2019\$edition\MSBuild\Current\Bin\amd64\MSBuild.exe"
        if (Test-Path $msbuild) { return $msbuild }
    }
    # 3. PATH
    $fromPath = Get-Command "msbuild.exe" -ErrorAction SilentlyContinue
    if ($fromPath) { return $fromPath.Source }

    throw "No se encontro MSBuild. Instala Visual Studio o Build Tools para .NET Framework."
}

# ── Verificar SAPbouiCOM.dll ───────────────────────────────────────────────────
$sdkDll = Join-Path $ProjectDir "lib\SAPbouiCOM.dll"
if (-not (Test-Path $sdkDll)) {
    Write-Error @"
ERROR: No se encontro '$sdkDll'.
Por favor copia SAPbouiCOM.dll a AddonPrueba\lib\ antes de compilar.
Ver AddonPrueba\lib\README.md para instrucciones.
"@
}

# ── Compilar ───────────────────────────────────────────────────────────────────
$msbuild = Find-MSBuild
Write-Host ""
Write-Host "=== Compilando AddonPrueba ($Configuration|x64) ===" -ForegroundColor Cyan
Write-Host "MSBuild: $msbuild"
Write-Host ""

& $msbuild $CsprojPath `
    /p:Configuration=$Configuration `
    /p:Platform=x64 `
    /t:Clean,Build `
    /v:minimal `
    /nologo

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion fallo. Revisa los errores de MSBuild."
}

Write-Host ""
Write-Host "Compilacion exitosa." -ForegroundColor Green

# ── Crear directorio temporal del paquete ─────────────────────────────────────
if (Test-Path $PackageDir) { Remove-Item $PackageDir -Recurse -Force }
New-Item -ItemType Directory -Path $PackageDir | Out-Null

# ── Copiar archivos al paquete ─────────────────────────────────────────────────
$filesToPack = @(
    (Join-Path $OutputDir  "AddonPrueba.exe"),
    (Join-Path $ProjectDir "AddonPrueba.b1s")
)

foreach ($f in $filesToPack) {
    if (-not (Test-Path $f)) {
        throw "Archivo esperado no encontrado: $f"
    }
    Copy-Item $f $PackageDir
    Write-Host "  + $(Split-Path $f -Leaf)"
}

# ── Generar ZIP ───────────────────────────────────────────────────────────────
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($PackageDir, $ZipPath)

# ── Limpiar directorio temporal ────────────────────────────────────────────────
Remove-Item $PackageDir -Recurse -Force

# ── Resultado ─────────────────────────────────────────────────────────────────
$size = (Get-Item $ZipPath).Length
Write-Host ""
Write-Host "=== Paquete generado ===" -ForegroundColor Green
Write-Host "  Archivo : $ZipPath"
Write-Host "  Tamano  : $([math]::Round($size/1KB, 1)) KB"
Write-Host ""
Write-Host "Pasos siguientes:" -ForegroundColor Yellow
Write-Host "  1. Abrir SAP Business One 10.0"
Write-Host "  2. Ir a: Administracion -> Extension Manager"
Write-Host "  3. Importar AddonPrueba.zip"
Write-Host "  4. Asignar el addon a la sociedad deseada"
Write-Host "  5. Reiniciar SAP B1 para que el addon se active"
Write-Host ""
