param([string]$GodotPath, [string]$AndroidSdkPath = $env:ANDROID_HOME,
    [string]$JavaHome = $env:JAVA_HOME, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'Pregateste_Export.ps1')
$tools = Get-BusinessBuildTools -GodotPath $GodotPath -SkipTemplate
if (-not (@(& $tools.DotnetPath --list-sdks | Where-Object { $_ -match '^(9|[1-9][0-9])\.' }).Count)) {
    throw 'Build-ul Android cere .NET SDK 9 sau mai nou. Jucatorul nu instaleaza .NET.'
}
if (-not $AndroidSdkPath -or -not $JavaHome) { throw 'Configureaza AndroidSdkPath si JavaHome (JDK 17).' }
if (-not (Test-Path -LiteralPath (Join-Path $AndroidSdkPath 'platform-tools/adb.exe')) -or
    -not (Test-Path -LiteralPath (Join-Path $JavaHome 'bin/java.exe'))) {
    throw 'Pentru build configureaza AndroidSdkPath si JavaHome (JDK 17). Vezi README.md.'
}
foreach ($name in @('GODOT_ANDROID_KEYSTORE_RELEASE_PATH', 'GODOT_ANDROID_KEYSTORE_RELEASE_USER', 'GODOT_ANDROID_KEYSTORE_RELEASE_PASSWORD')) {
    if (-not [Environment]::GetEnvironmentVariable($name)) { throw "Lipseste variabila de semnare: $name" }
}
& $tools.DotnetPath build (Join-Path $root 'game/BusinessSimulator.csproj') --configuration ExportRelease --nologo -p:GodotTargetPlatform=android
if ($LASTEXITCODE -ne 0) { throw 'Compilarea C# Android a esuat.' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'builds/android' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$output = Join-Path (Resolve-Path -LiteralPath $OutputDirectory).Path 'FirmoQuest-Android.apk'
$settings = Join-Path $env:APPDATA 'Godot/editor_settings-4.7.tres'
if (-not (Test-Path -LiteralPath $settings)) { throw 'Porneste o data Godot .NET pentru initializarea setarilor editorului.' }
$original = [IO.File]::ReadAllText($settings)
$configured = $original
foreach ($item in @(@('java_sdk_path', $JavaHome), @('android_sdk_path', $AndroidSdkPath))) {
    $value = $item[1].Replace('\', '/')
    $line = 'export/android/' + $item[0] + ' = "' + $value + '"'
    $configured = [regex]::Replace($configured, '(?m)^export/android/' + $item[0] + ' = .*$', [Text.RegularExpressions.MatchEvaluator]{ param($m) $line })
}
try {
    [IO.File]::WriteAllText($settings, $configured, [Text.UTF8Encoding]::new($false))
    try {
        $ErrorActionPreference = 'Continue'
        $exportLines = & $tools.GodotPath --headless --path (Join-Path $root 'game') --export-release Android $output 2>&1
        $exportCode = $LASTEXITCODE
    } finally { $ErrorActionPreference = 'Stop' }
    $exportLines | ForEach-Object { Write-Host $_ }
    if ($exportCode -ne 0 -or -not (Test-Path -LiteralPath $output) -or
        ($exportLines | Select-String -Pattern 'Failed to build project|Export .NET Project:|Project export .* failed')) {
        throw 'Exportul Android a esuat.'
    }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($output)
    try {
        foreach ($entry in @('assets/.godot/mono/publish/arm64/BusinessSimulator.dll',
            'assets/.godot/mono/publish/x86_64/BusinessSimulator.dll',
            'lib/arm64-v8a/libmonosgen-2.0.so', 'lib/x86_64/libmonosgen-2.0.so')) {
            if (-not $archive.GetEntry($entry)) { throw "APK incomplet: lipseste $entry" }
        }
    } finally { $archive.Dispose() }
    $signer = Get-ChildItem -LiteralPath (Join-Path $AndroidSdkPath 'build-tools') -Filter apksigner.bat -Recurse | Sort-Object FullName | Select-Object -Last 1
    & $signer.FullName verify --verbose $output
    if ($LASTEXITCODE -ne 0) { throw 'Semnatura APK nu este valida.' }
    $sha = [Security.Cryptography.SHA256]::Create()
    $stream = [IO.File]::OpenRead($output)
    try { $hash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '').ToLowerInvariant() }
    finally { $stream.Dispose(); $sha.Dispose() }
    [IO.File]::WriteAllText(($output + '.sha256.txt'), ($hash + '  FirmoQuest-Android.apk' + "`n"), [Text.UTF8Encoding]::new($false))
    Write-Host "APK semnat: $output"
} finally {
    [IO.File]::WriteAllText($settings, $original, [Text.UTF8Encoding]::new($false))
}
