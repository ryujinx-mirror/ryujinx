# Ryujinx
Experimental Switch emulator written in C#

Don't expect much from this. Some homebrew apps works, and Tetris shows the intro logos (sometimes) but that's about it for now.
Contributions are always welcome.

**Running**

To run this emulator, you need the .NET Core 2.0 (or higher) SDK.
Run `dotnet run -c Release -- game.nro` inside the Ryujinx solution folder.

Audio is partially supported (glitched) on Windows, you need to install the OpenAL Core SDK :
https://openal.org/downloads/OpenAL11CoreSDK.zip
