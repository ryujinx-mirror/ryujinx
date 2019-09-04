
<h1>
    <img src="https://i.imgur.com/G6Mleco.png"> Ryujinx 
    <a href="https://ci.appveyor.com/project/gdkchan/ryujinx" target="_blank">
        <img src="https://ci.appveyor.com/api/projects/status/ssg4jwu6ve3k594s?svg=true">
    </a> 
    <a href="https://discord.gg/N2FmfVc">
        <img src="https://img.shields.io/discord/410208534861447168.svg">
    </a>
</h1>

<p align="center">
    <i>An Experimental Switch emulator written in C#</i><br />
    <br />
    <img src="https://i.imgur.com/JDLmXJ6.png">
</p>

<h5 align="center">
    A lot of games boot, but only a handful are playable, see the compatiblity list <a href="https://github.com/Ryujinx/Ryujinx-Games-List/issues" target="_blank">here</a>.
</h5>

## Usage

To run this emulator, you need the [.NET Core 2.1 (or higher) SDK](https://dotnet.microsoft.com/download/dotnet-core).  

If you use a pre-built version, you can use the graphical interface to run your games/homebrew apps.  

If you build it yourself you will need to:  
Run `dotnet run -c Release -- path\to\homebrew.nro` inside the Ryujinx project folder to run homebrew apps.  
Run `dotnet run -c Release -- path\to\game.nsp/xci` to run official games.

Every file related to Ryujinx is stored in the `RyuFs` folder. Located in `C:\Users\USERNAME\AppData\Roaming\` for Windows, `/home/USERNAME/.config` for Linux or `/Users/USERNAME/Library/Application Support/` for macOS.

## Latest build

These builds are compiled automatically for each commit on the master branch.  
**They may be unstable or might not work at all.**  
The latest automatic build for Windows, macOS, and Linux can be found on the [Official Website](https://ryujinx.org/#/Build).

## Requirements

 - **Switch Keys**  
 
   Everything on the Switch is encrypted, so if you want to run anything else, other than homebrews, you have to dump them from your console. To get more information please take a look at our [Keys Documentation](KEYS.md) *(Outdated)*
   
 - **Shared Fonts**  
 
   Some games draw text using fonts files, those are what is called Shared Fonts.  
   All you have to do is [Download](https://ryujinx.org/ryujinx_shared_fonts.zip) them and extract those files in `RyuFs\system\fonts`.
   
 - **FFmpeg Dependencies**  
 
   Ryujinx has a basic implementation of `NVDEC` (video decoder used by the Switch's GPU).  
   Many games use videos that use NVDEC, so you need to download [Zeranoe FFmpeg Builds](http://ffmpeg.zeranoe.com/builds/) for your system, and in **Shared** linking.  
   When it's done, extract the `bin` folder directly into your Ryujinx folder.
   
 - **System Titles**  
 
   Some of our System Modules implementation require [System Data Archives](https://switchbrew.org/wiki/Title_list#System_Data_Archives).  
   You can install them by mounting your nand partition using [HacDiskMount](https://switchtools.sshnuke.net/) and copy the content in `RyuFs/nand/system`.
   
 - **Executables**
 
   Ryujinx is able to run games or homebrews.  
   You need a hacked Switch to dump them ([Hack Guide](https://switch.hacks.guide/)).  
   You need to dump your own games with [NxDumpTool](https://github.com/DarkMatterCore/nxdumptool) for XCI dump or [SwitchSDTool](https://github.com/CaitSith2/SwitchSDTool) for NSP dump.  
   You can find homebrew on different related websites or on the [Switch Appstore](https://www.switchbru.com/appstore/).

## Features

 - **Audio**  
 
   Everything for audio is partially supported. We currently use a C# wrapper for [libsoundio](http://libsound.io/) and we support [OpenAL](https://openal.org/downloads/OpenAL11CoreSDK.zip) (installation needed) too as a fallback. Our current Opus implementation is pretty incomplete.

- **CPU**  

  The CPU emulator emulates an ARMv8 CPU, and only the new 64-bits ARMv8 instructions are implemented (with a few instructions still missing). It translates the ARM code to a custom IR and then it performs a few optimizations and turns that into x86 code.  
  To handle that we use our own JIT called ARMeilleure, which has the custom IR and compiles the code to x86.  
  ChocolArm is the old ARM emulator, is being replaced by ARMeilleure (It can still be enabled inside the configuration menu/file) and it works by translating the ARM code to .NET IL. The runtime JIT then compiles that to the platform CPU code. On .NET Core, the JIT is called RyuJIT (hence the project name, Ryujinx).

- **GPU**  

  The GPU emulator emulates the Switch Maxwell GPU, using the OpenGL API (4.2 minimum) through a custom build of OpenTK.
  
- **Input**  

   We currently have Keyboard, Mouse, Touch, and JoyCon input support (emulated through the keyboard) and some controllers too. You can set-up everything inside the configuration menu/file.
  
- **Configuration**  
 
   The emulator has some options, like Dump shaders, Enabled/Disabled some Logging, Remap Controllers, Choose Controller, and more. You can set-up all of them through the graphical interface or manually through the Config File: `Config.json`.  
For more information [you can go here](CONFIG.md) *(Outdated)*.

## Compatibility

You can check out the compatibility list [here](https://github.com/Ryujinx/Ryujinx-Games-List/issues).  
Don't hesitate to open a new issue if a game isn't already on there.

## Help

If you have homebrew that currently doesn't work within the emulator, you can contact us through our Discord with the compiled *.NRO / *.NSO (and source code if possible) and we'll take note of whatever is causing the app / game to not work, on the watch list and fix it at a later date.  
If you need help for setting up Ryujinx, you can go to our Discord server too.

## Contact

For contributions, help, support, and suggestions or if you just want to get in touch with the team; join our [Discord server](https://discord.gg/N2FmfVc)!  
For donation support, please take a look at our [Patreon](https://www.patreon.com/ryujinx).