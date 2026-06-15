[CmdletBinding()]
param(
    [int]$ApiPort = 5178,
    [int]$WebPort = 3000
)

$ErrorActionPreference = "Stop"

$clientDirectory = Split-Path -Parent $PSScriptRoot
$repositoryDirectory = Split-Path -Parent $clientDirectory
$backendDirectory = Join-Path $repositoryDirectory "FitLead"
$apiProject = Join-Path $backendDirectory "FitLead.Api\FitLead.Api.csproj"
$infrastructureProject = Join-Path $backendDirectory "FitLead.Infrastructure\FitLead.Infrastructure.csproj"

$containerName = "fitlead-smoke-postgres-$PID"
$databaseName = "fitlead_smoke"
$databaseUser = "postgres"
$databasePassword = "postgres"
$apiProcess = $null
$containerStarted = $false
$apiOutputPath = Join-Path ([System.IO.Path]::GetTempPath()) "$containerName-api.out.log"
$apiErrorPath = Join-Path ([System.IO.Path]::GetTempPath()) "$containerName-api.err.log"

$environmentNames = @(
    "ASPNETCORE_ENVIRONMENT",
    "ASPNETCORE_URLS",
    "ConnectionStrings__DefaultConnection",
    "DemoSeed__Enabled",
    "ClientApp__BaseUrl",
    "Cors__AllowedOrigins__0",
    "API_BASE_URL",
    "NEXT_PUBLIC_UPLOADCARE_PUBLIC_KEY",
    "PLAYWRIGHT_BASE_URL"
)

$originalEnvironment = @{}
foreach ($name in $environmentNames) {
    $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
}

function Assert-CommandAvailable {
    param([Parameter(Mandatory)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' is not available."
    }
}

function Assert-PortAvailable {
    param([Parameter(Mandatory)][int]$Port)

    $listener = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($listener) {
        throw "Port $Port is already in use. Stop the existing service or choose another port."
    }
}

function Wait-ForPostgreSql {
    param([Parameter(Mandatory)][string]$Name)

    for ($attempt = 1; $attempt -le 60; $attempt++) {
        & docker exec $Name pg_isready -U $databaseUser -d $databaseName *> $null
        if ($LASTEXITCODE -eq 0) {
            return
        }

        Start-Sleep -Seconds 1
    }

    throw "PostgreSQL smoke container did not become ready in time."
}

function Wait-ForApi {
    param(
        [Parameter(Mandatory)][string]$HealthUrl,
        [Parameter(Mandatory)][System.Diagnostics.Process]$Process
    )

    for ($attempt = 1; $attempt -le 90; $attempt++) {
        if ($Process.HasExited) {
            $errorOutput = if (Test-Path $apiErrorPath) {
                Get-Content $apiErrorPath -Raw
            } else {
                "No API error output was captured."
            }

            throw "FitLead API exited before becoming healthy.`n$errorOutput"
        }

        try {
            $response = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -eq 200) {
                return
            }
        } catch {
            Start-Sleep -Seconds 1
        }
    }

    throw "FitLead API did not become healthy at $HealthUrl in time."
}

function Restore-Environment {
    foreach ($name in $environmentNames) {
        [Environment]::SetEnvironmentVariable(
            $name,
            $originalEnvironment[$name],
            "Process")
    }
}

try {
    Assert-CommandAvailable "docker"
    Assert-CommandAvailable "dotnet"
    Assert-CommandAvailable "npm.cmd"
    Assert-CommandAvailable "npx.cmd"
    Assert-PortAvailable $ApiPort
    Assert-PortAvailable $WebPort

    & docker info *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Engine is not available. Start Docker Desktop and try again."
    }

    & dotnet ef --version *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "EF Core CLI is unavailable. Install it with 'dotnet tool install --global dotnet-ef --version 8.*'."
    }

    Write-Host "Starting isolated PostgreSQL container..."
    $containerId = & docker run `
        --detach `
        --rm `
        --name $containerName `
        --env "POSTGRES_DB=$databaseName" `
        --env "POSTGRES_USER=$databaseUser" `
        --env "POSTGRES_PASSWORD=$databasePassword" `
        --publish "127.0.0.1::5432" `
        postgres:16-alpine

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($containerId)) {
        throw "Failed to start the PostgreSQL smoke container."
    }
    $containerStarted = $true

    Wait-ForPostgreSql $containerName

    $databasePort = (& docker port $containerName "5432/tcp").Trim().Split(":")[-1]
    if (-not $databasePort) {
        throw "Could not determine the PostgreSQL smoke container port."
    }

    $connectionString =
        "Host=127.0.0.1;Port=$databasePort;Database=$databaseName;Username=$databaseUser;Password=$databasePassword"

    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$ApiPort"
    $env:ConnectionStrings__DefaultConnection = $connectionString
    $env:DemoSeed__Enabled = "true"
    $env:ClientApp__BaseUrl = "http://localhost:$WebPort"
    $env:Cors__AllowedOrigins__0 = "http://localhost:$WebPort"
    $env:API_BASE_URL = "http://127.0.0.1:$ApiPort"
    $env:NEXT_PUBLIC_UPLOADCARE_PUBLIC_KEY = "test-public-key"
    $env:PLAYWRIGHT_BASE_URL = "http://localhost:$WebPort"

    Write-Host "Applying EF Core migrations to the isolated database..."
    & dotnet ef database update `
        --project $infrastructureProject `
        --startup-project $apiProject `
        --connection $connectionString

    if ($LASTEXITCODE -ne 0) {
        throw "EF Core migration failed."
    }

    Write-Host "Starting FitLead API..."
    $apiProcess = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList @(
            "run",
            "--project", $apiProject,
            "--no-launch-profile",
            "--no-restore") `
        -WorkingDirectory $backendDirectory `
        -WindowStyle Hidden `
        -PassThru `
        -RedirectStandardOutput $apiOutputPath `
        -RedirectStandardError $apiErrorPath

    Wait-ForApi "http://127.0.0.1:$ApiPort/health" $apiProcess

    Push-Location $clientDirectory
    try {
        Write-Host "Ensuring the Playwright Chromium browser is installed..."
        & npx.cmd playwright install chromium
        if ($LASTEXITCODE -ne 0) {
            throw "Playwright Chromium installation failed."
        }

        Write-Host "Building the production frontend and running smoke tests..."
        & npm.cmd run test:smoke:web
        if ($LASTEXITCODE -ne 0) {
            throw "Frontend smoke tests failed."
        }
    } finally {
        Pop-Location
    }
} finally {
    if ($apiProcess -and -not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
        Wait-Process -Id $apiProcess.Id -Timeout 10 -ErrorAction SilentlyContinue
    }

    if ($containerStarted) {
        & docker rm --force $containerName *> $null
    }

    Restore-Environment

    if (Test-Path $apiOutputPath) {
        Remove-Item -LiteralPath $apiOutputPath -Force -ErrorAction SilentlyContinue
    }

    if (Test-Path $apiErrorPath) {
        Remove-Item -LiteralPath $apiErrorPath -Force -ErrorAction SilentlyContinue
    }
}
