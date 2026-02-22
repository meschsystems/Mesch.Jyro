<#
.SYNOPSIS
    Runs the Jyro regression test suite using the CLI's built-in test subcommand.

.DESCRIPTION
    All test triplets (<name>.jyro + O-<name>.json, optionally D-<name>.json) under
    the tests root are discovered automatically.  Each script is executed via
    "jyro test -i <script> [-d <data>] -o <expected>".  The CLI performs JSON
    comparison internally; this runner checks exit codes.

    An optional E-<name>.txt file may contain the expected exit code (e.g. "1"
    for tests that exercise the fail keyword).  When absent, exit code 0 is expected.

.PARAMETER BinaryA
    Path to the first Jyro binary.  Overrides config.json.

.PARAMETER BinaryB
    Path to the second Jyro binary (optional).  Overrides config.json.

.PARAMETER Config
    Path to configuration file.  Defaults to config.json in the script directory.

.PARAMETER Timeout
    Per-test timeout in seconds.  Overrides config.json.  Default: 30.

.PARAMETER Category
    Run only tests in the specified folder name (e.g. "for", "literals").

.EXAMPLE
    .\Run-Tests.ps1

.EXAMPLE
    .\Run-Tests.ps1 -Category "for"

.EXAMPLE
    .\Run-Tests.ps1 -BinaryA "C:\bin\jyro.exe"
#>
[CmdletBinding()]
param(
    [string]$BinaryA,
    [string]$BinaryB,
    [string]$Config,
    [int]$Timeout,
    [string]$Category
)

$ErrorActionPreference = "Stop"
$SuiteRoot = if ($PSScriptRoot) { $PSScriptRoot } else { $PWD.Path }
if (-not $Config) { $Config = Join-Path $SuiteRoot "config.json" }

# -- Configuration ----------------------------------------------------------------

$cfg = @{ BinaryA = ""; BinaryB = ""; TimeoutSeconds = 30 }

if (Test-Path $Config) {
    $json = Get-Content $Config -Raw | ConvertFrom-Json
    if ($json.BinaryA)        { $cfg.BinaryA        = $json.BinaryA }
    if ($json.BinaryB)        { $cfg.BinaryB        = $json.BinaryB }
    if ($json.TimeoutSeconds) { $cfg.TimeoutSeconds  = $json.TimeoutSeconds }
}

if ($BinaryA) { $cfg.BinaryA = $BinaryA }
if ($BinaryB) { $cfg.BinaryB = $BinaryB }
if ($PSBoundParameters.ContainsKey('Timeout')) { $cfg.TimeoutSeconds = $Timeout }

$binaries = [ordered]@{}
if ($cfg.BinaryA) { $binaries["A"] = $cfg.BinaryA }
if ($cfg.BinaryB) { $binaries["B"] = $cfg.BinaryB }

if ($binaries.Count -eq 0) {
    Write-Host "ERROR: No binaries configured." -ForegroundColor Red
    Write-Host "Supply -BinaryA / -BinaryB or populate config.json."
    exit 1
}

# Resolve relative paths against the suite root and validate
foreach ($key in @($binaries.Keys)) {
    $p = $binaries[$key]
    if (-not [System.IO.Path]::IsPathRooted($p)) {
        $p = Join-Path $SuiteRoot $p
    }
    if (-not (Test-Path $p)) {
        Write-Host "ERROR: Binary $key not found: $p" -ForegroundColor Red
        exit 1
    }
    $binaries[$key] = (Resolve-Path $p).Path
}

$timeoutMs = $cfg.TimeoutSeconds * 1000
$plaintextDir = Join-Path $SuiteRoot "plaintext"

if (-not (Test-Path $plaintextDir)) {
    Write-Host "ERROR: Plaintext directory not found: $plaintextDir" -ForegroundColor Red
    exit 1
}

# -- Test Discovery ----------------------------------------------------------------

$tests = @()

Get-ChildItem $plaintextDir -Recurse -Filter "*.jyro" | ForEach-Object {
    $folder   = $_.DirectoryName
    $baseName = $_.BaseName
    $group    = Split-Path $folder -Leaf

    # Category filter
    if ($Category -and $group -ne $Category) { return }

    $dataFile     = Join-Path $folder "D-$baseName.json"
    $outFile      = Join-Path $folder "O-$baseName.json"
    $exitCodeFile = Join-Path $folder "E-$baseName.txt"

    if (-not (Test-Path $outFile)) {
        Write-Warning "Skipping $group/$baseName - missing O-$baseName.json"
        return
    }

    # D- file is optional; E- file is optional (default exit code 0)
    $hasData = Test-Path $dataFile
    $expectedExitCode = 0
    if (Test-Path $exitCodeFile) {
        $expectedExitCode = [int](Get-Content $exitCodeFile -Raw).Trim()
    }

    $tests += [PSCustomObject]@{
        Group            = $group
        Name             = $baseName
        Script           = $_.FullName
        Data             = if ($hasData) { $dataFile } else { $null }
        Expected         = $outFile
        ExpectedExitCode = $expectedExitCode
    }
}

$tests = @($tests | Sort-Object Group, Name)

if ($tests.Count -eq 0) {
    Write-Host "No test triplets found under $SuiteRoot" -ForegroundColor Red
    if ($Category) { Write-Host "  (filtered by category: $Category)" }
    exit 1
}

# -- Execution ---------------------------------------------------------------------

$runTimestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
$logDir       = Join-Path $SuiteRoot "logs"


$results      = @()
$currentGroup = ""

Write-Host ""
Write-Host "  Jyro Regression Test Suite" -ForegroundColor Cyan
Write-Host ("  " + "=" * 58) -ForegroundColor Cyan
foreach ($key in $binaries.Keys) {
    Write-Host "  Binary ${key}: $($binaries[$key])"
}
Write-Host "  Tests:   $($tests.Count)"
Write-Host "  Timeout: $($cfg.TimeoutSeconds)s"
if ($Category) { Write-Host "  Category: $Category" }
Write-Host ""

try {
    foreach ($test in $tests) {
        if ($test.Group -ne $currentGroup) {
            $currentGroup = $test.Group
            Write-Host ""
            Write-Host "  $currentGroup" -ForegroundColor Yellow
        }

        Write-Host -NoNewline ("    " + $test.Name.PadRight(36))

        $testResults = @{}

        foreach ($key in $binaries.Keys) {
            $binary = $binaries[$key]
            $status     = "FAIL"
            $detail     = ""
            $sw = [System.Diagnostics.Stopwatch]::StartNew()

            try {
                # Build args for "jyro test -i <script> [-d <data>] -o <expected>"
                $procArgs = "test -i `"$($test.Script)`" -o `"$($test.Expected)`""
                if ($test.Data) {
                    $procArgs += " -d `"$($test.Data)`""
                }

                $psi = [System.Diagnostics.ProcessStartInfo]::new()
                $psi.FileName = $binary
                $psi.Arguments = $procArgs
                $psi.UseShellExecute = $false
                $psi.RedirectStandardOutput = $true
                $psi.RedirectStandardError = $true
                $psi.CreateNoWindow = $true

                $proc = [System.Diagnostics.Process]::new()
                $proc.StartInfo = $psi
                [void]$proc.Start()

                # Read stdout/stderr before WaitForExit to avoid deadlock
                $stdout = $proc.StandardOutput.ReadToEnd()
                $stderr = $proc.StandardError.ReadToEnd()

                $exited = $proc.WaitForExit($timeoutMs)
                $exitCode = if ($exited) { $proc.ExitCode } else { -1 }
                $proc.Dispose()

                if (-not $exited) {
                    try { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue } catch {}
                    $detail = "timeout"
                }
                elseif ($exitCode -eq $test.ExpectedExitCode) {
                    $status = "PASS"
                    if ($test.ExpectedExitCode -ne 0) {
                        $detail = "exit $exitCode (expected)"
                    }
                }
                else {
                    $errText = $stdout.Trim()
                    $detail = "exit $exitCode"
                    if ($test.ExpectedExitCode -ne 0) {
                        $detail += " (expected $($test.ExpectedExitCode))"
                    }
                    if ($errText) { $detail += " - $errText" }
                }
            }
            catch {
                $detail = $_.Exception.Message
            }

            $sw.Stop()
            $elapsedMs = $sw.ElapsedMilliseconds

            $testResults[$key] = @{ Status = $status; Detail = $detail; ElapsedMs = $elapsedMs }

            $color = if ($status -eq "PASS") { "Green" } else { "Red" }
            $tag = "${key}: $status ${elapsedMs}ms"
            if ($detail) { $tag += " ($detail)" }
            Write-Host -NoNewline "  $tag" -ForegroundColor $color
        }

        if ($binaries.Count -eq 2 -and $testResults["A"] -and $testResults["B"]) {
            $diff = $testResults["A"].ElapsedMs - $testResults["B"].ElapsedMs
            $sign = if ($diff -ge 0) { "+" } else { "" }
            $diffColor = if ($diff -gt 0) { "DarkYellow" } elseif ($diff -lt 0) { "DarkCyan" } else { "Gray" }
            Write-Host -NoNewline "  ${sign}${diff}ms" -ForegroundColor $diffColor
        }

        Write-Host ""
        $results += [PSCustomObject]@{
            Group   = $test.Group
            Name    = $test.Name
            Results = $testResults
        }
    }
}
finally {
    # No temp file cleanup needed - using in-memory stream capture
}

# -- Summary -----------------------------------------------------------------------

Write-Host ""
Write-Host ("  " + "=" * 58) -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan

$anyFailed = $false

foreach ($key in $binaries.Keys) {
    $passed  = @($results | Where-Object { $_.Results[$key].Status -eq "PASS" }).Count
    $total   = $results.Count
    $totalMs = ($results | ForEach-Object { $_.Results[$key].ElapsedMs } | Measure-Object -Sum).Sum
    $color   = if ($passed -eq $total) { "Green" } else { "Yellow" }
    Write-Host "  Binary ${key}: $passed / $total passed  (${totalMs}ms total)" -ForegroundColor $color

    # Per-category breakdown
    $groups = $results | Group-Object -Property Group
    foreach ($g in $groups) {
        $gPassed = @($g.Group | Where-Object { $_.Results[$key].Status -eq "PASS" }).Count
        # Need to actually count from the items
        $gItems = $g.Group
        $gPassed = @($gItems | Where-Object { $_.Results[$key].Status -eq "PASS" }).Count
        $gTotal  = $gItems.Count
        if ($gPassed -ne $gTotal) {
            Write-Host "    $($g.Name): $gPassed / $gTotal" -ForegroundColor DarkYellow
        }
    }

    $failures = @($results | Where-Object { $_.Results[$key].Status -ne "PASS" })
    if ($failures.Count -gt 0) {
        $anyFailed = $true
        Write-Host ""
        Write-Host "  Failed (Binary ${key}):" -ForegroundColor Red
        foreach ($f in $failures) {
            $r = $f.Results[$key]
            $label = "$($f.Group)/$($f.Name)"
            Write-Host "    $($label.PadRight(44)) $($r.Detail)" -ForegroundColor Red
        }
    }
}

if ($binaries.Count -eq 2) {
    $sumA = ($results | ForEach-Object { $_.Results["A"].ElapsedMs } | Measure-Object -Sum).Sum
    $sumB = ($results | ForEach-Object { $_.Results["B"].ElapsedMs } | Measure-Object -Sum).Sum
    $totalDiff = $sumA - $sumB
    $sign = if ($totalDiff -ge 0) { "+" } else { "" }
    $diffColor = if ($totalDiff -gt 0) { "DarkYellow" } elseif ($totalDiff -lt 0) { "DarkCyan" } else { "Gray" }
    Write-Host "  Diff (A-B): ${sign}${totalDiff}ms" -ForegroundColor $diffColor
}

Write-Host ("  " + "=" * 58) -ForegroundColor Cyan

# -- Log ---------------------------------------------------------------------------

if (-not (Test-Path $logDir)) { New-Item $logDir -ItemType Directory -Force | Out-Null }

$logBinaries = [ordered]@{}
foreach ($key in $binaries.Keys) { $logBinaries[$key] = $binaries[$key] }

function Format-LogEntry($r) {
    $entry = [ordered]@{ group = $r.Group; name = $r.Name }
    foreach ($key in $binaries.Keys) {
        $entry[$key] = [ordered]@{
            status    = $r.Results[$key].Status
            elapsedMs = $r.Results[$key].ElapsedMs
        }
        if ($r.Results[$key].Detail) {
            $entry[$key]["detail"] = $r.Results[$key].Detail
        }
    }
    if ($binaries.Count -eq 2 -and $r.Results["A"] -and $r.Results["B"]) {
        $entry["diffMs"] = $r.Results["A"].ElapsedMs - $r.Results["B"].ElapsedMs
    }
    return [PSCustomObject]$entry
}

$isFailed = { param($r)
    foreach ($key in $binaries.Keys) {
        if ($r.Results[$key].Status -ne "PASS") { return $true }
    }
    return $false
}

$logPassed = @($results | Where-Object { -not (& $isFailed $_) } | ForEach-Object { Format-LogEntry $_ })
$logFailed = @($results | Where-Object { & $isFailed $_ }       | ForEach-Object { Format-LogEntry $_ })

$logSummary = [ordered]@{}
foreach ($key in $binaries.Keys) {
    $passed  = @($results | Where-Object { $_.Results[$key].Status -eq "PASS" }).Count
    $totalMs = ($results | ForEach-Object { $_.Results[$key].ElapsedMs } | Measure-Object -Sum).Sum
    $logSummary[$key] = [ordered]@{
        passed         = $passed
        failed         = $results.Count - $passed
        total          = $results.Count
        totalElapsedMs = $totalMs
    }
}

$logTopLevel = [ordered]@{
    timestamp      = (Get-Date -Format "o")
    binaries       = [PSCustomObject]$logBinaries
    timeoutSeconds = $cfg.TimeoutSeconds
    testCount      = $results.Count
    passed         = $logPassed
    failed         = $logFailed
    summary        = [PSCustomObject]$logSummary
}

if ($binaries.Count -eq 2) {
    $totalA = ($results | ForEach-Object { $_.Results["A"].ElapsedMs } | Measure-Object -Sum).Sum
    $totalB = ($results | ForEach-Object { $_.Results["B"].ElapsedMs } | Measure-Object -Sum).Sum
    $logTopLevel["totalDiffMs"] = $totalA - $totalB
}

$log = [PSCustomObject]$logTopLevel

$logFile = Join-Path $logDir "$runTimestamp.json"
$log | ConvertTo-Json -Depth 100 | Set-Content $logFile -Encoding UTF8

Write-Host "  Log: $logFile" -ForegroundColor DarkGray
Write-Host ""

if ($anyFailed) { exit 1 }
