<#
.SYNOPSIS
    Compiles all .jyro test scripts to .jyrx binary format.

.DESCRIPTION
    Scans all test directories for .jyro files and compiles each one into
    a .jyrx binary placed in tests/binaries/ with the same folder structure.
#>
[CmdletBinding()]
param(
    [string]$BinaryA,
    [string]$Config
)

$ErrorActionPreference = "Stop"
$SuiteRoot = if ($PSScriptRoot) { $PSScriptRoot } else { $PWD.Path }
if (-not $Config) { $Config = Join-Path $SuiteRoot "config.json" }

# Load binary path from config
$cfg = @{ BinaryA = "" }
if (Test-Path $Config) {
    $json = Get-Content $Config -Raw | ConvertFrom-Json
    if ($json.BinaryA) { $cfg.BinaryA = $json.BinaryA }
}
if ($BinaryA) { $cfg.BinaryA = $BinaryA }

if (-not $cfg.BinaryA) {
    Write-Host "ERROR: No binary configured." -ForegroundColor Red
    exit 1
}

$binary = $cfg.BinaryA
if (-not [System.IO.Path]::IsPathRooted($binary)) {
    $binary = Join-Path $SuiteRoot $binary
}
if (-not (Test-Path $binary)) {
    Write-Host "ERROR: Binary not found: $binary" -ForegroundColor Red
    exit 1
}
$binary = (Resolve-Path $binary).Path

$binariesDir  = Join-Path $SuiteRoot "binaries"
$plaintextDir = Join-Path $SuiteRoot "plaintext"
$compiled = 0
$failed = 0

if (-not (Test-Path $plaintextDir)) {
    Write-Host "ERROR: Plaintext directory not found: $plaintextDir" -ForegroundColor Red
    exit 1
}

Get-ChildItem $plaintextDir -Recurse -Filter "*.jyro" | ForEach-Object {
    $rel = $_.DirectoryName.Substring($plaintextDir.Length).TrimStart('\/')

    $outDir = Join-Path $binariesDir $rel
    if (-not (Test-Path $outDir)) { New-Item $outDir -ItemType Directory -Force | Out-Null }

    $outFile = Join-Path $outDir ($_.BaseName + ".jyrx")
    $result = & $binary compile -i $_.FullName -o $outFile 2>&1

    if ($LASTEXITCODE -eq 0) {
        $size = (Get-Item $outFile).Length
        Write-Host "  OK   $rel/$($_.Name)  ($size bytes)" -ForegroundColor Green
        $script:compiled++
    } else {
        Write-Host "  FAIL $rel/$($_.Name)" -ForegroundColor Red
        $result | ForEach-Object { Write-Host "       $_" -ForegroundColor DarkRed }
        $script:failed++
    }
}

Write-Host ""
Write-Host "  Compiled: $compiled  Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Yellow" })
