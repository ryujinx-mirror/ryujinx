#!/bin/sh

SCRIPT_DIR=$(dirname $(realpath $0))
RYUJINX_BIN="Ryujinx"

if [ -f "$SCRIPT_DIR/Ryujinx.Ava" ]; then
    RYUJINX_BIN="Ryujinx.Ava"
fi

env DOTNET_EnableAlternateStackCheck=1 "$SCRIPT_DIR/$RYUJINX_BIN" "$@"
