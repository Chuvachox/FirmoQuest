$ErrorActionPreference = 'Stop'
if ($PSEdition -eq 'Desktop') {
    $env:PSModulePath = (Join-Path $PSHOME 'Modules') + [IO.Path]::PathSeparator + $env:PSModulePath
}

function Get-BusinessBuildTools {
    param([string]$GodotPath, [string]$TemplateDirectory, [switch]$SkipTemplate)
    $repoRoot = Split-Path -Parent $PSScriptRoot
    $ProgressPreference = 'SilentlyContinue'
    $config = Get-Content -LiteralPath (Join-Path $repoRoot 'config/windows-build.json') -Raw | ConvertFrom-Json
    $cache = Join-Path $repoRoot '.tools'
    New-Item -ItemType Directory -Path $cache -Force | Out-Null
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    function Get-VerifiedArchive {
        param([string]$Url, [string]$Path, [string]$Hash, [string]$Algorithm)
        if (-not (Test-Path -LiteralPath $Path)) {
            Write-Host ('Descarc instrumentul oficial: ' + [IO.Path]::GetFileName($Path))
            Invoke-WebRequest -UseBasicParsing -Uri $Url -OutFile $Path -TimeoutSec 300
        }
        if ((Get-FileHash -LiteralPath $Path -Algorithm $Algorithm).Hash -ine $Hash) {
            throw "Controlul de integritate a esuat: $Path"
        }
    }

    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    $dotnetPath = if ($dotnet) { $dotnet.Source } else { $null }
    $hasSdk = $false
    $hasRuntime = $false
    if ($dotnetPath) {
        $hasSdk = @(& $dotnetPath --list-sdks | Where-Object { $_ -match '^(8|9|[1-9][0-9])\.' }).Count -gt 0
        $hasRuntime = @(& $dotnetPath --list-runtimes | Where-Object { $_ -match '^Microsoft.NETCore.App 8\.' }).Count -gt 0
    }
    if (-not ($hasSdk -and $hasRuntime)) {
        $sdkDir = Join-Path $cache ('dotnet-' + $config.sdkVersion)
        $dotnetPath = Join-Path $sdkDir 'dotnet.exe'
        if (-not (Test-Path -LiteralPath $dotnetPath)) {
            $sdkZip = Join-Path $cache ('dotnet-' + $config.sdkVersion + '.zip')
            Get-VerifiedArchive $config.sdkUrl $sdkZip $config.sdkSha512 'SHA512'
            Expand-Archive -LiteralPath $sdkZip -DestinationPath $sdkDir -Force
        }
    }
    $dotnetRoot = Split-Path -Parent $dotnetPath
    $env:DOTNET_ROOT = $dotnetRoot
    $env:PATH = $dotnetRoot + [IO.Path]::PathSeparator + $env:PATH

    if (-not $GodotPath) { $GodotPath = $env:GODOT_BIN }
    if (-not $GodotPath) {
        $godotDir = Join-Path $cache ('godot-' + $config.godotVersion)
        $GodotPath = Join-Path $godotDir ('Godot_v' + $config.godotVersion + '-stable_mono_win64/Godot_v' + $config.godotVersion + '-stable_mono_win64_console.exe')
        if (-not (Test-Path -LiteralPath $GodotPath)) {
            $godotZip = Join-Path $cache ('godot-' + $config.godotVersion + '.zip')
            Get-VerifiedArchive $config.godotUrl $godotZip $config.godotSha256 'SHA256'
            Expand-Archive -LiteralPath $godotZip -DestinationPath $godotDir -Force
        }
    }
    if (-not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) { throw 'Nu gasesc Godot .NET.' }
    $GodotPath = (Resolve-Path -LiteralPath $GodotPath).Path
    $version = (& $GodotPath --version | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or $version -notmatch '^4\.7\.2\.stable\.mono\.') {
        throw "Este necesar Godot 4.7.2 .NET. Gasit: $version"
    }
    if ($SkipTemplate) {
        return [PSCustomObject]@{GodotPath=$GodotPath; DotnetPath=$dotnetPath; TemplatePath=$null}
    }

    if (-not $TemplateDirectory) {
        $TemplateDirectory = Join-Path $env:APPDATA ('Godot/export_templates/' + $config.godotVersion + '.stable.mono')
    }
    New-Item -ItemType Directory -Path $TemplateDirectory -Force | Out-Null
    $template = Join-Path $TemplateDirectory 'windows_release_x86_64.exe'
    if (-not (Test-Path -LiteralPath $template)) {
        Write-Host 'Descarc numai sablonul Windows necesar, nu toate platformele.'
        Add-Type -AssemblyName System.Net.Http
        $client = New-Object Net.Http.HttpClient
        $request = New-Object Net.Http.HttpRequestMessage([Net.Http.HttpMethod]::Get, $config.templateUrl)
        $request.Headers.Range = New-Object Net.Http.Headers.RangeHeaderValue([long]$config.templateOffset, ([long]$config.templateOffset + [long]$config.templateCompressedBytes - 1))
        $response = $null
        $inputStream = $null
        $inflater = $null
        $outputStream = $null
        try {
            $response = $client.SendAsync($request).GetAwaiter().GetResult()
            if ([int]$response.StatusCode -ne 206) { throw 'Serverul nu a acceptat descarcarea partiala.' }
            $packed = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
            if ($packed.LongLength -ne [long]$config.templateCompressedBytes) { throw 'Sablon descarcat incomplet.' }
            $inputStream = New-Object IO.MemoryStream(,$packed)
            $inflater = New-Object IO.Compression.DeflateStream($inputStream, [IO.Compression.CompressionMode]::Decompress)
            $outputStream = New-Object IO.MemoryStream
            $inflater.CopyTo($outputStream)
            if ($outputStream.Length -ne [long]$config.templateBytes) { throw 'Dimensiunea sablonului este incorecta.' }
            $bytes = $outputStream.ToArray()
            $sha = [Security.Cryptography.SHA256]::Create()
            try { $hash = [BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant() }
            finally { $sha.Dispose() }
            if ($hash -cne $config.templateSha256) { throw 'Sablonul nu corespunde controlului SHA256.' }
            [IO.File]::WriteAllBytes($template, $bytes)
        } finally {
            if ($outputStream) { $outputStream.Dispose() }
            if ($inflater) { $inflater.Dispose() }
            if ($inputStream) { $inputStream.Dispose() }
            if ($response) { $response.Dispose() }
            $request.Dispose()
            $client.Dispose()
        }
    }
    if ((Get-FileHash -LiteralPath $template -Algorithm SHA256).Hash -ine $config.templateSha256) {
        throw 'Sablonul existent difera de cel oficial fixat; nu il suprascriu.'
    }
    [IO.File]::WriteAllText((Join-Path $TemplateDirectory 'version.txt'), '4.7.2.stable.mono', [Text.UTF8Encoding]::new($false))
    return [PSCustomObject]@{GodotPath=$GodotPath; DotnetPath=$dotnetPath; TemplatePath=$template}
}
