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
  local description="$1"
  local target_pattern="$2"
  local depends_suffix="$3"

  echo "Updating references in $description"
  mapfile -t targets < <(find . -name "$target_pattern" | sort)

  if [ ${#targets[@]} -eq 0 ]; then
    echo "No target projects found for pattern: $target_pattern"
    return 0
  fi

  for proj in "${targets[@]}"; do
    echo "Processing $proj"

    # Update any PackageReference whose Include ends with the given suffix
    # Example: *.Domain, *.DataAccess, *.Client
    if grep -q "<PackageReference Include=\".*${depends_suffix}\"" "$proj"; then
      sed -i -E "s|( <PackageReference Include=\"[^\"]*${depends_suffix}\"[^>]*Version=\")([^\"]+)(\"[^>]*/>)|\1${VERSION}\3|g" "$proj" || true
      sed -i -E "s|(<PackageReference Include=\"[^\"]*${depends_suffix}\"[^>]*Version=\")([^\"]+)(\"[^>]*/>)|\1${VERSION}\3|g" "$proj" || true
    fi
  done
}

echo "=== LAYER 1: .Domain ==="
build_pack_layer "Domain" "*.Domain.csproj"

echo "=== Update .DataAccess references to .Domain ==="
update_references ".DataAccess projects" "*.DataAccess.csproj" ".Domain"

echo "=== LAYER 2: .DataAccess ==="
build_pack_layer "DataAccess" "*.DataAccess.csproj"

echo "=== Update .Client references to .DataAccess ==="
update_references ".Client projects" "*.Client.csproj" ".DataAccess"

echo "=== LAYER 3: .Client ==="
build_pack_layer "Client" "*.Client.csproj"

echo "=== Update .Business references to .Client ==="
update_references ".Business projects" "*.Business.csproj" ".Client"

echo "=== Update .Api references to .Client ==="
update_references ".Api projects" "*.Api.csproj" ".Client"

echo "=== LAYER 4: .Business ==="
build_pack_layer "Business" "*.Business.csproj"

echo "=== LAYER 5: .Api ==="
build_pack_layer "Api" "*.Api.csproj"

echo "All layers processed."
