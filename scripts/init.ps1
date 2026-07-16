param(
    [switch]$Run
)

$ErrorActionPreference = "Stop"

$RootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$Solution = Join-Path $RootDir "Lofasi.slnx"
$ApiProject = Join-Path $RootDir "src/Lofasi.API/Lofasi.API.csproj"
$InfrastructureProject = Join-Path $RootDir "src/Lofasi.Infrastructure/Lofasi.Infrastructure.csproj"
$ToolManifest = Join-Path $RootDir ".config/dotnet-tools.json"

Set-Location $RootDir

Write-Host "Restoring NuGet packages..."
dotnet restore $Solution

Write-Host "Restoring local .NET tools..."
dotnet tool restore --tool-manifest $ToolManifest

Write-Host "Applying EF Core migrations and creating/updating the SQLite database..."
dotnet tool run dotnet-ef database update `
    --project $InfrastructureProject `
    --startup-project $ApiProject `
    --context BankingDbContext

Write-Host "Building solution..."
dotnet build $Solution --no-restore

Write-Host "Initialization completed."

if ($Run) {
    Write-Host "Starting API..."
    dotnet run --project $ApiProject
}
else {
    Write-Host "Run the API with: dotnet run --project src/Lofasi.API"
}
