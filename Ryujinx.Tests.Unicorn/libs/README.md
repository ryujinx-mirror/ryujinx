# Unicorn

Unicorn is a CPU simulator with bindings in many languages, including
C#/.NET.
It is used by the Ryujinx test suite for comparative testing with its built-in
CPU simulator, Armeilleure.

## Windows

On Windows, Unicorn is shipped as a pre-compiled dynamic library (`.dll`), licenced under the GPLv2.

The source code for `windows/unicorn.dll` is available at: https://github.com/MerryMage/UnicornDotNet/tree/299451c02d9c810d2feca51f5e9cb6d8b2f38960

## Linux

On Linux, you will first need to download Unicorn from https://github.com/unicorn-engine/unicorn.

Then you need to patch it to expose the FSPCR register by applying `linux/unicorn_fspcr.patch` 

Then, compile Unicorn from source with its `make.sh` script.

See https://github.com/Ryujinx/Ryujinx/pull/1433 for details.
