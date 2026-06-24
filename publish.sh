#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

dotnet publish Gw2PveDesktop.Avalonia/Gw2PveDesktop.Avalonia.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained false \
  -o publish/linux-x64

echo "Published to publish/linux-x64/Gw2PveDesktop"
