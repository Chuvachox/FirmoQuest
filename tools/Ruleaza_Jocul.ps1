param(
    [string]$GodotPath,
    [switch]$SelfTest,
    [switch]$QaTest,
    [switch]$Gallery
)
$ErrorActionPreference = 'Stop'
if (([int]$SelfTest.IsPresent + [int]$QaTest.IsPresent + [int]$Gallery.IsPresent) -gt 1) {
    throw 'Alege un singur mod: SelfTest, QaTest sau Gallery.'
}
$projectRoot = Split-Path -Parent $PSScriptRoot
$gameRoot = Join-Path $projectRoot 'game'

. (Join-Path $PSScriptRoot 'Pregateste_Export.ps1')
$tools = Get-BusinessBuildTools -GodotPath $GodotPath -SkipTemplate
$GodotPath = $tools.GodotPath
& $tools.DotnetPath build (Join-Path $gameRoot 'BusinessSimulator.csproj') --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$godotArgs = @('--path', $gameRoot)
if ($SelfTest) { $godotArgs = @('--headless') + $godotArgs + @('--', '--selftest') }
elseif ($QaTest) { $godotArgs = @('--headless') + $godotArgs + @('--', '--qatest') }
elseif ($Gallery) { $godotArgs += @('--', '--gallery') }
& $GodotPath @godotArgs
exit $LASTEXITCODE
