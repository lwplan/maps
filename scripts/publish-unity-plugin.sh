#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="$ROOT_DIR/UnityProject/Assets/Plugins"

echo "Publishing maps plugin to $OUTPUT_DIR"
dotnet publish "$ROOT_DIR/maps.csproj" -c Release -f netstandard2.1 -o "$OUTPUT_DIR"

echo "Done. Assemblies are available under $OUTPUT_DIR"
