param(
    [string]$Project = "tests/FitLead.IntegrationTests/FitLead.IntegrationTests.csproj"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$resultsDir = Join-Path $PSScriptRoot "TestResults\Coverage"
$reportDir = Join-Path $PSScriptRoot "coverage-report"
$projectPath = if ([System.IO.Path]::IsPathRooted($Project)) {
    $Project
}
else {
    Join-Path $repoRoot $Project
}

if (Test-Path $resultsDir) {
    Remove-Item -Path $resultsDir -Recurse -Force
}

if (Test-Path $reportDir) {
    Remove-Item -Path $reportDir -Recurse -Force
}

Write-Host "Running tests with coverage..."
dotnet test $projectPath `
  --settings (Join-Path $PSScriptRoot "coverage.runsettings") `
  --collect:"XPlat Code Coverage" `
  --results-directory $resultsDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed."
}

$coverageFiles = Get-ChildItem -Path $resultsDir -Recurse -Filter "coverage.cobertura.xml" -File
if ($coverageFiles.Count -eq 0) {
    throw "coverage.cobertura.xml was not produced."
}

Write-Host "Coverage files:"
$coverageFiles | ForEach-Object { Write-Host " - $($_.FullName)" }

$reportsPattern = Join-Path $resultsDir "**\coverage.cobertura.xml"

$usedReportGenerator = $false

if (Get-Command reportgenerator -ErrorAction SilentlyContinue) {
    reportgenerator "-reports:$reportsPattern" "-targetdir:$reportDir" "-reporttypes:Html;TextSummary"
    $usedReportGenerator = $LASTEXITCODE -eq 0
}
else {
    dotnet tool run reportgenerator -- "-reports:$reportsPattern" "-targetdir:$reportDir" "-reporttypes:Html;TextSummary"
    $usedReportGenerator = $LASTEXITCODE -eq 0
}

if ($usedReportGenerator) {
    Write-Host "Coverage report generated at: $reportDir"
}
else {
    Write-Warning "ReportGenerator is not available or failed. Raw coverage XML is still available."
    Write-Host "Recommended setup (local tool manifest):"
    Write-Host "dotnet new tool-manifest"
    Write-Host "dotnet tool install dotnet-reportgenerator-globaltool"
    Write-Host "dotnet tool restore"
}
