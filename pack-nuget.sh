#!/usr/bin/env bash
set -euo pipefail

if [ $# -ne 1 ]; then
  echo "Usage: $0 <version>"
  exit 1
fi

VERSION="$1"
FEED_URL="https://nuget.pkg.github.com/${GITHUB_REPOSITORY_OWNER}/index.json"
API_KEY="${GH_PACKAGES_PAT:-}"

if [ -z "$API_KEY" ]; then
  echo "GH_PACKAGES_PAT environment variable is not set."
  exit 1
fi

NUGET_OUTPUT_DIR="nuget"
mkdir -p "$NUGET_OUTPUT_DIR"

echo "Using version: $VERSION"
echo "Feed: $FEED_URL"

build_pack_layer() {
  local layer_name="$1"
  local pattern="$2"

  echo "=============================="
  echo "Layer: $layer_name ($pattern)"
  echo "=============================="

  mapfile -t projects < <(find . -name "$pattern" | sort)

  if [ ${#projects[@]} -eq 0 ]; then
    echo "No projects found for pattern: $pattern"
    return 0
  fi

  for proj in "${projects[@]}"; do
    echo "Restoring $proj"
    dotnet restore "$proj"

    echo "Building $proj"
    dotnet build "$proj" -c Release

    echo "Packing $proj"
    dotnet pack "$proj" -c Release --no-build -o "$NUGET_OUTPUT_DIR" /p:Version="$VERSION"
  done

  echo "Pushing packages for layer: $layer_name"
  dotnet nuget push "$NUGET_OUTPUT_DIR"/*.nupkg \
    --api-key "$API_KEY" \
    --source "$FEED_URL" \
    --skip-duplicate
}

update_references() {
  local pattern="$1"   # e.g. "*.Domain.csproj"

  echo "Updating references for pattern: $pattern"

  # Extract suffix: "*.Domain.csproj" → ".Domain"
  local suffix=".$(echo "$pattern" | sed -E 's/^\*\.(.*)\.csproj/\1/')"
  echo "Detected suffix: $suffix"

  # Find ALL csproj files
  mapfile -t all_projects < <(find . -name "*.csproj" | sort)

  for proj in "${all_projects[@]}"; do
    echo "Scanning $proj"

    #
    # 1. Update inline Version="..."
    #
    sed -i -E \
      "s|(Include=\"[^\"]*${suffix}\"[^>]*Version=\")([^\"]+)(\")|\1${VERSION}\3|g" \
      "$proj"

    #
    # 2. Update <Version>...</Version> inside a PackageReference
    #
    sed -i -E \
      "/Include=\"[^\"]*${suffix}\"/,/<\/PackageReference>/ s|<Version>[0-9A-Za-z\.\-]+</Version>|<Version>${VERSION}</Version>|g" \
      "$proj"
  done
}



echo "=== LAYER 1: .Domain ==="
build_pack_layer "Domain" "*.Domain.csproj"

echo "=== Update references to .Domain ==="
update_references "*.Domain.csproj"

echo "=== LAYER 2: .DataAccess ==="
build_pack_layer "DataAccess" "*.DataAccess.csproj"

echo "=== Update references to .DataAccess ==="
update_references "*.DataAccess.csproj"

echo "=== LAYER 3: .Client ==="
build_pack_layer "Client" "*.Client.csproj"

echo "=== Update references to .Client ==="
update_references "*.Client.csproj"

echo "All layers processed."
