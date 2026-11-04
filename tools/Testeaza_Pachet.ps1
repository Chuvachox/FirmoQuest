param([Parameter(Mandatory=$true)][string]$ExePath, [switch]$QaTest)
$ErrorActionPreference = 'Stop'
if ($PSEdition -eq 'Desktop') {
    $env:PSModulePath = (Join-Path $PSHOME 'Modules') + [IO.Path]::PathSeparator + $env:PSModulePath
}
$ExePath = (Resolve-Path -LiteralPath $ExePath).Path
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('business-sim-test-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory -Force | Out-Null
$emptyDotnet = Join-Path $testDirectory 'no-dotnet'
New-Item -ItemType Directory -Path $emptyDotnet -Force | Out-Null
$startInfo = New-Object Diagnostics.ProcessStartInfo
$startInfo.FileName = $ExePath
$startInfo.Arguments = if ($QaTest) { '--headless -- --qatest' } else { '--headless -- --selftest' }
$startInfo.WorkingDirectory = $testDirectory
$startInfo.UseShellExecute = $false
$startInfo.CreateNoWindow = $true
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
$startInfo.StandardOutputEncoding = [Text.Encoding]::UTF8
$startInfo.StandardErrorEncoding = [Text.Encoding]::UTF8
$startInfo.EnvironmentVariables['PATH'] = Join-Path $env:SystemRoot 'System32'
$startInfo.EnvironmentVariables['DOTNET_ROOT'] = $emptyDotnet
$startInfo.EnvironmentVariables['DOTNET_ROOT_X64'] = $emptyDotnet
$startInfo.EnvironmentVariables['DOTNET_MULTILEVEL_LOOKUP'] = '0'
$process = New-Object Diagnostics.Process
$process.StartInfo = $startInfo
try {
    if (-not $process.Start()) { throw 'Nu pot porni pachetul.' }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(180000)) {
        $process.Kill()
        throw 'Testul pachetului a depasit 180 de secunde.'
    }
    $output = $stdoutTask.GetAwaiter().GetResult() + $stderrTask.GetAwaiter().GetResult()
    $logDir = Join-Path (Split-Path -Parent $ExePath) '../validation'
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    $reportName = if ($QaTest) { 'portable-ui-test' } else { 'portable-test' }
    $logName = if ($QaTest) { 'portable-ui-test.log' } else { 'portable-selftest.log' }
    [IO.File]::WriteAllText((Join-Path $logDir $logName), $output, [Text.UTF8Encoding]::new($false))
    $pattern = if ($QaTest) { 'QA:\s+(\d+) treceri, 0 e\u0219ecuri' } else { 'Rezultat:\s+(\d+) treceri, 0 e\u0219ecuri' }
    $match = [regex]::Match($output, $pattern)
    if ($process.ExitCode -ne 0 -or -not $match.Success) {
        throw "Testul pachetului a esuat. Vezi $logDir/$logName"
    }
    $report = [ordered]@{
        executable = [IO.Path]::GetFileName($ExePath)
        sha256 = (Get-FileHash -LiteralPath $ExePath -Algorithm SHA256).Hash.ToLowerInvariant()
        bytes = (Get-Item -LiteralPath $ExePath).Length
        passed = [int]$match.Groups[1].Value
        failed = 0
        exitCode = $process.ExitCode
        dotnetRootWasEmpty = $true
        godotAndDotnetNotInPath = $true
        workingDirectoryWasDifferent = $true
        separateCleanComputerTested = $false
        mode = if ($QaTest) { 'ui' } else { 'logic' }
    }
    [IO.File]::WriteAllText((Join-Path $logDir ($reportName + '.json')), (($report | ConvertTo-Json) + "`n"), [Text.UTF8Encoding]::new($false))
    Write-Host ('Pachet verificat: ' + $match.Groups[1].Value + ' teste, 0 esecuri, fara Godot/dotnet in PATH.')
} finally {
    $process.Dispose()
}
exit 0
