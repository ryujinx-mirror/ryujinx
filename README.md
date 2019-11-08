
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
    A lot of games boot, but only some are playable. See the compatiblity list <a href="https://github.com/Ryujinx/Ryujinx-Games-List/issues" target="_blank">here</a>.
</h5>

## Usage

To run this emulator, you need the [.NET Core 3.0 (or higher) SDK](https://dotnet.microsoft.com/download/dotnet-core).

If you use a pre-built version, you can use the graphical interface to run your games and homebrew.

If you build it yourself you will need to:
Run `dotnet run -c Release -- path\to\homebrew.nro` inside the Ryujinx project folder to run homebrew apps.
Run `dotnet run -c Release -- path\to\game.nsp/xci` to run official games.

Every file related to Ryujinx is stored in the `RyuFs` folder. Located in `C:\Users\USERNAME\AppData\Roaming\` for Windows, `/home/USERNAME/.config` for Linux or `/Users/USERNAME/Library/Application Support/` for macOS. It can also be accessed by clicking `Open Ryujinx Folder` under the File menu in the GUI.

## Latest build

These builds are compiled automatically for each commit on the master branch, **and may be unstable or completely broken.**

The latest automatic build for Windows, macOS, and Linux can be found on the [Official Website](https://ryujinx.org/#/Build).

## Requirements

 - **Switch Keys**

   Everything on the Switch is encrypted, so if you want to run anything other than homebrew, you have to dump encryption keys from your console. To get more information please take a look at our [Keys Documentation](KEYS.md) *(Outdated)*.

 - **FFmpeg Dependencies**

   Ryujinx has a basic implementation of `NVDEC`, a video decoder used by the Switch's GPU. Many games include videos that use it, so you need to download [Zeranoe's FFmpeg Builds](http://ffmpeg.zeranoe.com/builds/) for **Shared** linking and your computer's operating system. When it's done, extract the contents of the `bin` folder directly into your Ryujinx folder.

 - **System Titles**

   Some of our System Module implementations, like `time`, require [System Data Archives](https://switchbrew.org/wiki/Title_list#System_Data_Archives). You can install them by mounting your nand partition using [HacDiskMount](https://switchtools.sshnuke.net/) and copying the content to `RyuFs/nand/system`.

 - **Executables**

   Ryujinx is able to run both official games and homebrew.

   Homebrew is available on many websites, such as the [Switch Appstore](https://www.switchbru.com/appstore/).

   A hacked Nintendo Switch is needed to dump games, which you can learn how to do [here](https://nh-server.github.io/switch-guide/). Once you have hacked your Nintendo Switch, you will need to dump your own games with [NxDumpTool](https://github.com/DarkMatterCore/nxdumptool/releases) to get an XCI or NSP dump.

## Features

 - **Audio**

   Everything for audio is partially supported. We currently use a C# wrapper for [libsoundio](http://libsound.io/), and we support [OpenAL](https://openal.org/downloads/OpenAL11CoreSDK.zip) (installation needed) too as a fallback. Our current Opus implementation is pretty incomplete.

- **CPU**

  The CPU emulator, ARMeilleure, emulates an ARMv8 CPU, and currently only has support for the new 64-bit ARMv8 instructions (with a few instructions still missing). It translates the ARM code to a custom IR, performs a few optimizations, and turns that into x86 code. To handle that, we use our own JIT called ARMeilleure, which uses the custom IR and compiles the code to x86.

- **GPU**

  The GPU emulator emulates the Switch's Maxwell GPU using the OpenGL API (version 4.2 minimum) through a custom build of OpenTK.

- **Input**

   We currently have support for keyboard, mouse, touch input, JoyCon input support emulated through the keyboard, and some controllers too. You can set up everything inside the configuration menu.

- **Configuration**

   The emulator has settings for dumping shaders, enabling or disabling some logging, remapping controllers, and more. You can configure all of them through the graphical interface or manually through the config file, `Config.json`.

   For more information [you can go here](CONFIG.md) *(Outdated)*.

## Compatibility

You can check out the compatibility list [here](https://github.com/Ryujinx/Ryujinx-Games-List/issues).

Don't hesitate to open a new issue if a game isn't already on there.

## Help

If you have homebrew that currently doesn't work within the emulator, you can contact us through our Discord with the .NRO/.NSO and source code, if possible. We'll take note of whatever is causing the app/game to not work, on the watch list and fix it at a later date.

If you need help with setting up Ryujinx, you can ask questions in the support channel of our Discord server.

## Contact

If you have contributions, need support, have suggestions, or just want to get in touch with the team, join our [Discord server](https://discord.gg/N2FmfVc)!

If you'd like to donate, please take a look at our [Patreon](https://www.patreon.com/ryujinx).
