![](https://ryujinx.github.io/static/img/Ryujinx_logo_128.png)
# Ryujinx [![Build status](https://ci.appveyor.com/api/projects/status/ssg4jwu6ve3k594s?svg=true)](https://ci.appveyor.com/project/gdkchan/ryujinx)

Experimental Switch emulator written in C#

Don't expect much from this. Some homebrew apps work, Puyo Puyo Tetris shows the intro logo (sometimes), and a handful of games boot / work; but that's about it for now.
Contributions are always welcome.

**Building**

To build this emulator, you will need the .NET Core 2.1 (RC1) (or higher) SDK. https://www.microsoft.com/net/download/
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
   - For more information on how to configure these buttons see [CONFIG.md](CONFIG.md)

 - Controller Input is partially supported:
   - Left Joycon:
     - Analog Stick = Left Analog Stick
	 - DPad Up = DPad Up
	 - DPad Down = DPad Down
	 - DPad Left = DPad Left
	 - DPad Right = DPad Right
	 - Minus = Select / Back / Share
	 - L = Left Shoulder Button
	 - ZL = Left Trigger
	 
   - Right Joycon:
	 - Analog Stick = Right Analog Stick
	 - A = B / Circle
	 - B = A / Cross
	 - X = Y / Triangle
	 - Y = X / Square
	 - Plus = Start / Options
	 - R = Right Shoulder Button
	 - ZR = Right Trigger
   - For more information on how to configure these buttons see [CONFIG.md](CONFIG.md)

 - Config File: `Ryujinx.conf` should be present in executable folder.
   For more information [you can go here](CONFIG.md).

 - If you are a Windows user, you can configure your keys, the logs, install OpenAL, etc... with Ryujinx-Setting.
 [Download it, right here](https://github.com/AcK77/Ryujinx-Settings)

**Help**

If you have some homebrew that currently doesn't work within the emulator, you can contact us through our Discord with the compiled NRO/NSO (and source code if possible) and then we'll make changes in order to make the requested app / game work.

**Contact**

For help, support, suggestions, or if you just want to get in touch with the team; join our Discord server!  
https://discord.gg/VkQYXAZ

For donation support, please take a look at our Patreon: https://www.patreon.com/ryujinx

**Running**

To run this emulator, you need the .NET Core 2.1 (or higher) SDK *and* the OpenAL 11 Core SDK.  
Run `dotnet run -c Release -- path\to\homebrew.nro` inside the Ryujinx solution folder to run homebrew apps.  
Run `dotnet run -c Release -- path\to\game_exefs_and_romfs_folder` to run official games (they need to be decrypted and extracted first!)

**Compatibility**

You can check out the compatibility list within the Wiki. Only a handful of games actually work.

**Latest build**

These builds are compiled automatically for each commit on the master branch. They may be unstable or might not work at all.  
The latest automatic build for Windows (64-bit) can be found on the [official website](https://ryujinx.org/#/Build).
