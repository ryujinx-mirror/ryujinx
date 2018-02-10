# Ryujinx [![Build status](https://ci.appveyor.com/api/projects/status/ssg4jwu6ve3k594s?svg=true)](https://ci.appveyor.com/project/gdkchan/ryujinx)

Experimental Switch emulator written in C#

Don't expect much from this. Some homebrew apps works, and Tetris shows the intro logos (sometimes) but that's about it for now.
Contributions are always welcome.

**Building**

To build this emulator, you need the .NET Core 2.0 (or higher) SDK. https://www.microsoft.com/net/download/
In release builds, memory checks are disabled to improve performances.

Or just drag'n'drop the *.NRO or the game folder on  the executable if you have a pre-build version.

**Features**

 - Audio is partially supported (glitched) on Windows but you need to install the OpenAL Core SDK.
https://openal.org/downloads/OpenAL11CoreSDK.zip

 - Keyboard Input is partially supported:
   - Arrows.
   - Enter > "Start" & Tab > "Select"
   - Qwerty: 
     - A > "A"
     - S > "B"
     - Z > "X"
     - X > "Y"
   - Azerty:
     - Q > "A"
     - S > "B"
     - W > "X"
     - X > "Y" 

 - Config File: `Ryujinx.conf` should be present on executable folder.
   - Logging_Enable_Info (bool)
   Enable the Informations Logging.

   - Logging_Enable_Trace (bool)
   Enable the Trace Logging (Enabled in Debug recommanded).

   - Logging_Enable_Debug (bool)
   Enable the Debug Logging (Enabled in Debug recommanded).

   - Logging_Enable_Warn (bool)
   Enable the Warning Logging (Enabled in Debug recommanded).

   - Logging_Enable_Error (bool)
   Enable the Error Logging (Enabled in Debug recommanded).

   - Logging_Enable_Fatal (bool)
   Enable the Fatal Logging (Enabled in Debug recommanded).

   - Logging_Enable_LogFile (bool)
   Enable writing the logging inside a Ryujinx.log file.

**Help**

If you have some homebrews that currently doesn't work on it, you can contact us through discord with the compiled NRO/NSO (and source code if possible) and will work to make them work.

**Contact**

For help, support, suggestion, or if you just want to get in touch with the team, join our Discord served!
https://discord.gg/VkQYXAZ

**Running**

To run this emulator, you need the .NET Core 2.0 (or higher) SDK.
Run `dotnet run -c Release -- path\to\homebrew.nro` inside the Ryujinx solution folder to run homebrew apps.
Run `dotnet run -c Release -- path\to\game_exefs_and_romfs_folder` to run official games (they need to be decrypted and extracted first!).

Audio is partially supported (glitched) on Windows, you need to install the OpenAL Core SDK:
https://openal.org/downloads/OpenAL11CoreSDK.zip

**Lastest build**

Those builds are compiled automatically for each commit on the master branch. They may be unstable or not work at all.
To download the lastest automatic build for Windows (64-bits), [Click Here](https://ci.appveyor.com/api/projects/gdkchan/ryujinx/artifacts/ryujinx_lastest_unstable.zip).
