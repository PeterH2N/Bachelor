#!/usr/bin/env bash
set -euo pipefail

VERSION="$1"

echo "Setting version: $VERSION"

# ----------------------------------------
# Find all csproj files in the repo
# ----------------------------------------
ALL_PROJECTS=$(find . -name "*.csproj")

echo "All projects found:"
echo "$ALL_PROJECTS"

# ----------------------------------------
# Filter only packable projects
# ----------------------------------------
PACKABLE_PROJECTS=""

for PROJ in $ALL_PROJECTS; do
  if grep -q "<IsPackable>true</IsPackable>" "$PROJ"; then
    PACKABLE_PROJECTS="$PACKABLE_PROJECTS $PROJ"
  fi
done

echo "Packable projects:"
echo "$PACKABLE_PROJECTS"

# ----------------------------------------
# Update <Version> in packable csproj files
# ----------------------------------------
for PROJ in $PACKABLE_PROJECTS; do
  echo "Updating version in $PROJ"

  if grep -q "<Version>" "$PROJ"; then
    sed -i "s|<Version>.*</Version>|<Version>$VERSION</Version>|" "$PROJ"
  else
    sed -i "s|<PropertyGroup>|<PropertyGroup>\n    <Version>$VERSION</Version>|" "$PROJ"
  fi
done

# ----------------------------------------
# Pack all packable projects
# ----------------------------------------
OUTPUT_DIR="./nuget"
mkdir -p "$OUTPUT_DIR"

for PROJ in $PACKABLE_PROJECTS; do
  echo "Packing $PROJ"
  dotnet pack "$PROJ" -c Release -o "$OUTPUT_DIR" /p:Version="$VERSION"
done

echo "Done. Packages are in: $OUTPUT_DIR"
