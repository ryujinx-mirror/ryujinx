#!/bin/sh

SCRIPT_DIR=$(dirname $(realpath $0))

env DOTNET_EnableAlternateStackCheck=1 "$SCRIPT_DIR/Ryujinx" "$@"
