param([string]$GodotPath, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$gameRoot = Join-Path $projectRoot 'game'
$config = Get-Content -LiteralPath (Join-Path $projectRoot 'config/windows-build.json') -Raw | ConvertFrom-Json
. (Join-Path $PSScriptRoot 'Pregateste_Export.ps1')
$tools = Get-BusinessBuildTools -GodotPath $GodotPath
& $tools.DotnetPath build (Join-Path $gameRoot 'BusinessSimulator.csproj') --configuration ExportRelease --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot 'builds/windows' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$outputExe = Join-Path $OutputDirectory $config.executable
& $tools.GodotPath --headless --path $gameRoot --export-release Windows $outputExe
$exportCode = $LASTEXITCODE
if ($exportCode -ne 0 -or -not (Test-Path -LiteralPath $outputExe)) {
    throw "Export esuat, cod $exportCode."
}
& (Join-Path $PSScriptRoot 'Testeaza_Pachet.ps1') -ExePath $outputExe
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$hash = (Get-FileHash -LiteralPath $outputExe -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText(($outputExe + '.sha256.txt'), ($hash + '  ' + $config.executable + "`n"), [Text.UTF8Encoding]::new($false))
Write-Host "Jocul este pregatit: $outputExe"
Write-Host 'Pentru jucator este suficient acest singur EXE. Nu necesita Godot sau .NET instalat.'
exit 0
