![](https://ryujinx.github.io/static/img/Ryujinx_logo_128.png)
# Ryujinx [![Build status](https://ci.appveyor.com/api/projects/status/ssg4jwu6ve3k594s?svg=true)](https://ci.appveyor.com/project/gdkchan/ryujinx)

Experimental Switch emulator written in C#

Many games boot, only a handful are playable, see the compatiblity list [here](https://github.com/Ryujinx/Ryujinx-Games-List/issues).

**Building**

To build this emulator, you will need the [.NET Core 2.1 (or higher) SDK](https://www.microsoft.com/net/download/)
or just drag'n'drop the homebrew *.NRO / *.NSO or the game *.NSP / *.XCI on the executable if you have a pre-built version.

**Features**

 - Audio is partially supported.

 - Keyboard Input is supported, see [CONFIG.md](CONFIG.md)

 - Controller Input is supported, see [CONFIG.md](CONFIG.md)

 - Config File: `Ryujinx.conf` should be present in executable folder.
   For more information [you can go here](CONFIG.md).

**Help**

If you have some homebrew that currently doesn't work within the emulator, you can contact us through our Discord with the compiled *.NRO / *.NSO (and source code if possible) and then we'll keep whatever is making app / game not work on the watch list and fix it at a later date.

**Contact**

For help, support, suggestions, or if you just want to get in touch with the team; join our [Discord server](https://discord.gg/N2FmfVc)!

For donation support, please take a look at our [Patreon](https://www.patreon.com/ryujinx).

**Running**

To run this emulator, you need the .NET Core 2.1 (or higher) SDK.  
Run `dotnet run -c Release -- path\to\homebrew.nro` inside the Ryujinx project folder to run homebrew apps.  
Run `dotnet run -c Release -- path\to\game.nsp/xci` to run official games.

**Compatibility**

You can check out the compatibility list [here](https://github.com/Ryujinx/Ryujinx-Games-List/issues).

**Latest build**

These builds are compiled automatically for each commit on the master branch. They may be unstable or might not work at all.  
The latest automatic build for Windows, Mac, and Linux can be found on the [official website](https://ryujinx.org/#/Build).
