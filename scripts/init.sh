#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOLUTION="$ROOT_DIR/Lofasi.slnx"
API_PROJECT="$ROOT_DIR/src/Lofasi.API/Lofasi.API.csproj"
INFRA_PROJECT="$ROOT_DIR/src/Lofasi.Infrastructure/Lofasi.Infrastructure.csproj"

cd "$ROOT_DIR"

RUN_API=false

for arg in "$@"; do
  case "$arg" in
    --run)
      RUN_API=true
      ;;
    *)
      echo "Unknown argument: $arg"
      echo "Usage: ./scripts/init.sh [--run]"
      exit 1
      ;;
  esac
done

echo "Restoring NuGet packages..."
dotnet restore "$SOLUTION"

echo "Restoring local .NET tools..."
dotnet tool restore --tool-manifest "$ROOT_DIR/.config/dotnet-tools.json"

echo "Applying EF Core migrations and creating/updating the SQLite database..."
dotnet tool run dotnet-ef database update \
  --project "$INFRA_PROJECT" \
  --startup-project "$API_PROJECT" \
  --context BankingDbContext

echo "Building solution..."
dotnet build "$SOLUTION" --no-restore

echo "Initialization completed."

if [ "$RUN_API" = true ]; then
  echo "Starting API..."
  dotnet run --project "$API_PROJECT"
else
  echo "Run the API with: dotnet run --project src/Lofasi.API"
fi
