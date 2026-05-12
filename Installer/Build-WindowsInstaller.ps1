param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$iconPath = Join-Path $PSScriptRoot "assets/GuitarTui.ico"
$payloadDir = Join-Path $PSScriptRoot "payload"
$publishDir = Join-Path $repoRoot "artifacts/windows-app"
$installerOut = Join-Path $repoRoot "artifacts/windows-installer"

& (Join-Path $PSScriptRoot "Generate-Icon.ps1") -OutputPath $iconPath

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $payloadDir, $publishDir, $installerOut
New-Item -ItemType Directory -Force -Path $payloadDir, $publishDir, $installerOut | Out-Null

dotnet publish (Join-Path $repoRoot "guitar-resources-tui.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:AssemblyName="Guitar TUI" `
    -p:ApplicationIcon=$iconPath `
    -o $publishDir

Copy-Item -LiteralPath (Join-Path $publishDir "Guitar TUI.exe") -Destination (Join-Path $payloadDir "Guitar TUI.exe") -Force

dotnet publish (Join-Path $PSScriptRoot "GuitarTuiInstaller.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $installerOut

$setupPath = Join-Path $installerOut "GuitarTuiSetup.exe"
$finalPath = Join-Path $repoRoot "artifacts/GuitarTUI-Setup-win-x64.exe"
Copy-Item -LiteralPath $setupPath -Destination $finalPath -Force
Write-Host "Built installer: $finalPath"
