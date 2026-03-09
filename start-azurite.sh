#!/usr/bin/env bash
# Start Azurite storage emulator for local Azure Functions development.
# Usage: ./start-azurite.sh

set -e

if ! command -v azurite &>/dev/null; then
    echo "Azurite not found. Installing..."
    npm install -g azurite
fi

echo "Starting Azurite on default ports (10000/10001/10002)..."
azurite --silent --location "${TMPDIR:-/tmp}/azurite" --debug "${TMPDIR:-/tmp}/azurite-debug.log"
