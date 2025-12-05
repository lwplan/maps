#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="$ROOT_DIR/UnityProject/Assets/Plugins"
BUILD_DIR="$ROOT_DIR/.unity-plugin-build"

echo "Publishing maps plugin to $OUTPUT_DIR"
rm -rf "$BUILD_DIR"
dotnet publish "$ROOT_DIR/maps.csproj" -c Release -f netstandard2.1 -o "$BUILD_DIR"

mkdir -p "$OUTPUT_DIR"
rm -f "$OUTPUT_DIR"/SixLabors.*
for dll in maps.dll Delaunator.dll DelaunatorSharp.dll YamlDotNet.dll System.Numerics.Vectors.dll; do
  if [[ -f "$BUILD_DIR/$dll" ]]; then
    cp "$BUILD_DIR/$dll" "$OUTPUT_DIR/"
  fi
done

echo "Done. Assemblies are available under $OUTPUT_DIR"
