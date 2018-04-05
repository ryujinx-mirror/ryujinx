# Ryujinx [![Build status](https://ci.appveyor.com/api/projects/status/ssg4jwu6ve3k594s?svg=true)](https://ci.appveyor.com/project/gdkchan/ryujinx)

Experimental Switch emulator written in C#

Don't expect much from this. Some homebrew apps work, and Puyo Puyo Tetris shows the intro logo (sometimes) but that's about it for now.
Contributions are always welcome.

**Building**

To build this emulator, you need the .NET Core 2.0 (or higher) SDK. https://www.microsoft.com/net/download/
In release builds, memory checks are disabled to improve performances.

Or just drag'n'drop the *.NRO / *.NSO or the game folder on the executable if you have a pre-build version.

**Features**

 - Audio is partially supported (glitched) on Windows but you need to install the OpenAL Core SDK.
https://openal.org/downloads/OpenAL11CoreSDK.zip

 - Keyboard Input is partially supported:
   - Left Joycon:
	 - Stick Up = W
	 - Stick Down = S
	 - Stick Left = A
	 - Stick Right = D
	 - Stick Button = F
	 - DPad Up = Up
	 - DPad Down = Down
	 - DPad Left = Left
	 - DPad Right = Right
	 - Minus = -
	 - L = E
	 - ZL = Q

   - Right Joycon:
	 - Stick Up = I
	 - Stick Down = K
	 - Stick Left = J
	 - Stick Right = L
	 - Stick Button = H
	 - A = Z
	 - B = X
	 - X = C
	 - Y = V
	 - Plus = +
	 - R = U
	 - ZR = O

 - Config File: `Ryujinx.conf` should be present in executable folder.
   For more informations [you can go here](CONFIG.md).
 
 - If you are a Windows user, you can configure your keys, the logs, install OpenAL, etc... with Ryujinx-Setting.
 [Download it, right here](https://github.com/AcK77/Ryujinx-Settings)

**Help**

If you have some homebrew that currently don't work on the emulator, you can contact us through Discord with the compiled NRO/NSO (and source code if possible) and then we'll make changes to make the requested app / game work.

**Contact**

For help, support, suggestions, or if you just want to get in touch with the team, join our Discord server!
https://discord.gg/VkQYXAZ

**Running**

To run this emulator, you need the .NET Core 2.0 (or higher) SDK.
Run `dotnet run -c Release -- path\to\homebrew.nro` inside the Ryujinx solution folder to run homebrew apps.
Run `dotnet run -c Release -- path\to\game_exefs_and_romfs_folder` to run official games (they need to be decrypted and extracted first!).

**Latest build**

These builds are compiled automatically for each commit on the master branch. They may be unstable or not work at all.
To download the latest automatic build for Windows (64-bits), [Click Here](https://ci.appveyor.com/api/projects/gdkchan/ryujinx/artifacts/ryujinx_latest_unstable.zip?pr=false).
