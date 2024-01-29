#!/bin/sh

SCRIPT_DIR=$(dirname "$(realpath "$0")")
RYUJINX_BIN="Ryujinx"

if [ -f "$SCRIPT_DIR/Ryujinx.Ava" ]; then
    RYUJINX_BIN="Ryujinx.Ava"
fi

if [ -f "$SCRIPT_DIR/Ryujinx.Headless.SDL2" ]; then
    RYUJINX_BIN="Ryujinx.Headless.SDL2"
fi

COMMAND="env DOTNET_EnableAlternateStackCheck=1"

if command -v gamemoderun > /dev/null 2>&1; then
    COMMAND="$COMMAND gamemoderun"
fi

$COMMAND "$SCRIPT_DIR/$RYUJINX_BIN" "$@"
