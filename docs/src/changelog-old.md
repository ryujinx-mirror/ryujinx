This is the changelog page for versions before 1.1.x (AppVeyor releases)

## 1.0.7181 - 2022-01-21
### Fixed:
- Fix deadlock for GPU counter report when 0 draws are done.
  - Fixes a rare bug where reporting a counter for a region containing 0 draws could deadlock the GPU, usually showing an error saying "Gpu AwaitResult: Error: Query result timed out. Took more than 5000 tries".
  - This uncommon bug affected games such as Mario Kart 8 Deluxe, Splatoon 2 and Super Mario Odyssey when backend multithreading was set to Auto/On.

## 1.0.7180 - 2022-01-21
### Fixed:
- Add host CPU memory barriers for DMB/DSB and ordered load/store.
  - Fixes softlocks in Pokémon Brilliant Diamond/Shining Pearl and Pokémon Mystery Dungeon DX.
  - May fix or reduce softlocks in Pokémon Sword/Shield, Xenoblade Chronicles 2 and Yumenikki -Dream Diary-, possibly other games with similar softlocks.

## 1.0.7179 - 2022-01-21
### Changed:
- Stop using glTransformFeedbackVaryings and use explicit layout on the shader.
  - Reduces code differences between main build and Vulkan branch. No changes in games.

## 1.0.7178 - 2022-01-20
### Added:
- Add capability for BGRA formats.
  - Reduces code differences between main build and Vulkan branch. No changes in games.

## 1.0.7177 - 2022-01-20
### Added:
- Implement FCVTNS (Scalar GP).
  - Allows NBA 2K21 to boot.

## 1.0.7176 - 2022-01-18
### Changed:
- Readme overhaul.
  - Updates and cleans up the Readme.md file.

## 1.0.7175 - 2022-01-16 
### Fixed:
- Scale scissor used for clears.
  - Fixes a regression introduced in 1.0.7168 that would cause the screen to flicker in some games with resolution scale other than native, like Luigi's Mansion 3 or Tetris 99.

## 1.0.7174 - 2022-01-16 
### Fixed:
- kernel: Fix deadlock when pinning in interrupt handler.
  - Fixes a deadlock on DoDonPachi Resurrection when starting a new game.
  - May fix similar deadlocks on other games.

## 1.0.7173 - 2022-01-16
### Fixed:
- Fix return type mismatch on 32-bit titles.
  - Fixes an assert that was caused by the return type not matching the actual return type of the function, due to the address being 32-bits. 
  - Only affects debug builds.

## 1.0.7172 - 2022-01-13
### Added:
- ssl: Implement SSL connectivity.
  - Adds support for SSL connection using .NET APIs for it.
  - May improve games that connect to non-Nintendo servers.

## 1.0.7171 - 2022-01-12 
### Changed:
- bsd: Revamp API and make socket abstract.
  - Cleans up the emulator's sockets code. No known changes to emulator functionality.

## 1.0.7170 - 2022-01-12
### Fixed:
- sfdnsres: Fix serialization issues.
  - Fixes a crash in Monster Hunter Rise when guest Internet access is enabled.

## 1.0.7169 - 2022-01-12
### Changed:
- Update to LibHac 0.15.0.
  - No changes to emulator functionality.

## 1.0.7168 - 2022-01-11
### Fixed:
- Fix render target clear when sizes mismatch.
  - Fixes Pathway not entirely clearing the screen.

## 1.0.7167 - 2022-01-11 
### Fixed:
- Fix adjacent 3d texture slices being detected as Incompatible Overlaps.
  - Fixes some regressions introduced in 1.0.7162 which caused 3D texture data to be lost for most slices.
  - Fixes colour grading in Xenoblade Chronicles 2, and probably more games.

## 1.0.7166 - 2022-01-11
### Changed:
- account: Rework LoadIdTokenCache to auto generate a random JWT token.

## 1.0.7165 - 2022-01-11
### Changed:
- sfdnsres: Block communication attempt with NPLN servers.
  - Blocks the emulator from communicating with Monster Hunter Rise servers.

## 1.0.7164 - 2022-01-10 
### Added:
- Implement IMUL, PCNT and CONT shader instructions, fix FFMA32I and HFMA32I.
  - Required by MelonDS Switch port and some homebrew applications. (MelonDS still needs more changes to work.)
  - Fixes a regression introduced in 1.0.7069.

## 1.0.7163 - 2022-01-10 
### Fixed:
- Fix sampled multisample texture size.
  - Fixes rendering in Okami HD.

## 1.0.7162 - 2022-01-09
### Fixed:
- Texture Sync, incompatible overlap handling, data flush improvements.
  - Fixes white water and teleportation animation in Breath of the Wild.
  - Fixes rainbow lighting in Splatoon 2 after loading screens.
  - Fixes many rendering/ texture streaming issues in (mostly) UE3/UE4 games, including A Hat in Time, Brothers: A Tale of Two Sons, Darksiders 3, Hellblade: Senua's Sacrifice, Life is Strange: True Colors, possibly more.
  - Fixes texture corruption in The Witcher 3.
  - Fixes Mario Kart 8 Deluxe replay thumbnails being from the previous race or black.

## 1.0.7161 - 2022-01-08 
### Fixed:
- Return error on DNS resolution when guest internet access is disabled.
  - Fixes a regression introduced in 1.0.7137 that made Crash Bandicoot 4 crash early on boot after trying to connect to some server. Note that the game will still crash if guest internet access is enabled.

## 1.0.7160 - 2022-01-08
### Fixed:
- Add support for render scale to vertex stage.
  - Fixes render scale causing offset bloom (ghost images) in Super Mario Party, Mario Party Superstars and Clubhouse Games.
  - Fixes grid-like artifacts when increasing the resolution on Hyrule Warriors: Age of Calamity.

## 1.0.7159 - 2022-01-04
### Added:
- CPU - Implement FCVTMS (Vector).
  - Allows XCOM 2 Collection to boot.

## 1.0.7158 - 2022-01-03
### Fixed:
- sfdnsres: Implement NSD resolution.
  - Fixes a missing implementation usage of NSD on IResolver when requested on GetAddrInfoRequest* and GetHostByNameRequest*.

## 1.0.7157 - 2022-01-03
### Fixed:
- Fix build id case issue for enabled cheats.
  - Fixes an issue where cheats wouldn't work if the cheat file name was lowercase.

## 1.0.7156 - 2022-01-03
### Added:
- Implement analog stick range modifier.
  - Adds range modification to the analog sticks of a given controller. Setting it above 1.00 makes it easier for controllers (especially older ones) to reach max stick values.

## 1.0.7155 - 2022-01-03
### Added:
- ffmpeg: Add extra checks and error messages.
  - In Linux, these logs will make it more evident that a game is crashing because the necessary ffmpeg packages aren't installed.

## 1.0.7154 - 2022-01-03
### Added:
- Add Cheat Manager.
  - Adds a cheat manager which makes it possible to disable and enable installed cheats while a game is running. However, it does not allow you to edit or add cheats, and any new cheats added to the cheat file while a game is running, will not work.
  - Cheat manager can be accessed by right clicking games on the list or from Actions > Manage Cheats during gameplay.

## 1.0.7153 - 2022-01-03
### Added:
- misc - Improve DNS blacklist for Nintendo servers.
  - Blacklists more DNS addresses for Nintendo’s servers.

## 1.0.7152 - 2021-12-31
### Fixed:
- Force crop when presentation cached texture size mismatches.
  - Fixes rendering being shifted up slightly in Hades.
  - Fixes NSO Nintendo 64 game selection being shifted up. (The NSO games still require more changes to run.)
  - Fixes similar issue in Super Mario Sunshine.

## 1.0.7151 - 2021-12-30 
### Fixed:
- Add support for the R4G4 texture format.
  - Fixes black HUD elements in the Nintendo 64 NSO version of Ocarina of Time. (The NSO games still require more changes to run.)

## 1.0.7150 - 2021-12-30 
### Fixed:
- Fix A1B5G5R5 texture format.
  - Fixes graphical glitches in the Nintendo 64 NSO version of Ocarina of Time. May fix similar glitches in other N64 emulated titles. (The NSO games still require more changes to run.)

## 1.0.7149 - 2021-12-30 
### Changed:
- friend: Stub IsFriendListCacheAvailable and EnsureFriendListAvailable.
  - Super Bomberman R Online will no longer crash on boot, however it will still get stuck on the loading screen due to other issues.

## 1.0.7148 - 2021-12-30
### Changed:
- am: Stub SetMediaPlaybackStateForApplication.
  - Required by the YouTube app.

## 1.0.7147 - 2021-12-30 
### Added:
- kernel: Implement thread pinning support.
  - Adds support for 8.x thread pinning changes and implements SynchronizePreemptionState syscall.
  - May fix some softlocks. 

## 1.0.7146 - 2021-12-29
### Fixed:
- Improve SocketOption handling.
  - Fixes some warnings caused by missing options.

## 1.0.7145 - 2021-12-29 
### Changed:
- hid: A little cleanup.
  - Small code cleanup. No changes expected in emulator functionality.

## 1.0.7144 - 2021-12-28
### Fixed:
- Flip scissor box when the YNegate bit is set.
  - Fixes in-game UI in Bloons TD 5.
  - Fixes menus being cut off in the YouTube app.
  - May fix similar issues in games using OpenGL on the Switch.

## 1.0.7143 - 2021-12-28
### Fixed:
- Fix GetHostByNameRequestWithOptions and GetHostByAddrRequestWithOptions.
  - Allows the Twitch app to boot further, but it still doesn't work as it requires unimplemented SSL functions.

## 1.0.7142 - 2021-12-27
### Fixed:
- Use minimum stream sample count on SDL2 audio backend.
  - Fixes "New audio stream setup with a sample count of ..." log spam followed by massive slowdown in some games, such as Final Fantasy VII, when using the SDL2 audio backend. 

## 1.0.7141 - 2021-12-27
### Fixed:
- Fix wrong title language.
  - Fixes a regression introduced in 1.0.7131 that caused game titles to display in the wrong language (for example, Korean being displayed when British English was selected).

## 1.0.7140 - 2021-12-26 
### Fixed:
- Fix DMA copy fast path line size when xCount < stride.
  - Fixes some random crashes in the Youtube app.
  - Possibly fixes some other games that do DMA copy with linear source textures. 

## 1.0.7139 - 2021-12-26 
### Fixed:
- Fix missing default value of audio volume.
  - Volume level default is now set to 100% instead of 0.

## 1.0.7138 - 2021-12-26
### Fixed:
- Fix I2M texture copies when line length is not a multiple of 4.
  - Fixes some texture corruption on games using OpenGL on the Switch, such as subtitle text in Cat Girl Without Salad. 
  - Fixes a similar issue in the YouTube app.

## 1.0.7137 - 2021-12-26 
### Fixed:
- Fix GetAddrInfoWithOptions and some sockets issues.
  - A new "Enable guest Internet access" option has been added. When enabled, applications might try to access their servers. (This does NOT include Nintendo Switch Online servers.)
  - Allows the YouTube app to run. Requires "Enable guest Internet access".
  - Might improve games with network related issues.

## 1.0.7136 - 2021-12-26
### Fixed:
- Fix bug causing an audio buffer to be enqueued more than once.
  - Fixes audio in the YouTube app. Potentially fixes games with broken audio.

## 1.0.7135 - 2021-12-23
### Changed:
- Revert "sdl2: Update to Ryujinx.SDL2-CS 2.0.19" (1.0.7132).
  - Reverted as it was causing crashes on latest Windows 10 build for some people. 

## 1.0.7134 - 2021-12-23
### Changed:
- Remove PortRemoteClosed warning.
  - Removes the warning as the result is not really an error, but a normal result that is returned under normal operation, when a service session is closed. 

## 1.0.7133 - 2021-12-23
### Changed:
- misc: Update SPB to 0.0.4-build17.
  - Update to a new SPB version targeting .NET 6.

## 1.0.7132 - 2021-12-23
### Fixed:
- sdl2: Update to Ryujinx.SDL2-CS 2.0.19.
  - Fixes G-Shark gamepad.
  - Fixes broken motion controls on Linux.

## 1.0.7131 - 2021-12-23
### Fixed:
- Update to LibHac v0.14.3.
  - Support reading NCAs with sparse partitions. Fixes NSPs with FS entry offsets outside of the total NCA length being unable to boot.
  - Allow deleting read-only files and directories in LocalFileSystem. Fixes
Ryujinx saves getting corrupted if backed up using Google Drive.
  - Disable AesXtsFileSystem. Fixes
LibHac failing to decrypt some games on launch with AES-XTS keys error, such as Alien: Isolation or Pokkén Tournament DX Demo.
  - The IFileSystem interface has been updated for system version 12.0.0. Now it uses the Fs.Path type for paths instead of a Span<byte>.
  - LibHac now uses SharedRef<T> and UniqueRef<T> types which are similar to the std::shared_ptr and std::unique_ptr types used in Horizon. These help ensure resources are properly closed. FS IPC interfaces were updated to use these types.
  - Fix loading NCAs that don't have a data partition.
  - Ruined King: A League of Legends Story now goes in game.
  - Hextech Mayhem: A League of Legends Story now goes in game.
  - Fire Emblem: Shadow Dragon and the Blade of Light now goes in game
  - Lost in Random now goes in game.
  - Last Stop now goes in game.

## 1.0.7130 - 2021-12-23
### Added:
- Add Volume Controls + Mute Toggle (F2).
  - Adds volume controls to the Settings > System page, as well as a volume indicator in the bottom bar.
  - Clicking the volume indicator in the bottom bar or pressing F2 toggles mute. 

## 1.0.7129 - 2021-12-19
### Fixed:
- Fix for texture pool not being updated when it should + buffer texture fixes.
  - Fixes black vertex explosions on Dragon Quest XI S and possibly other UE4 games.
  - Fixes vertex explosions in Minecraft: Bedrock Edition.
  - Fixes black/flickering textures in SnowRunner, Balan Wonderworld Demo, SpongeBob SquarePants: Battle for Bikini Bottom, Ender Lilies and Yoshi's Crafted World (costume menu).
  - Improves rendering slightly in GTA San Andreas (Trilogy).

## 1.0.7128 - 2021-12-19
### Fixed:
- Add support for releasing a semaphore to DmaClass.
  - Fixes freezes in games that use OpenGL on the Switch (primarily GameMaker ones), such as Undertale, Idol Days, Yokai Watch 1, and Record of Lodoss War: Deedlit in Wonder Labyrinth.

## 1.0.7127 - 2021-12-19
### Added:
- Implement CSDB instruction.
  - Allows Monster Rancher 1 & 2 DX to go in-game.

## 1.0.7126 - 2021-12-19
### Changed:
- Use more intense lossless compression.
  - Compresses project images as much as possible while retaining the original quality.

## 1.0.7125 - 2021-12-15
### Changed:
- Remove debug configuration and schema.

## 1.0.7124 - 2021-12-14
### Changed:
- Remove unused empty Ryujinx.Audio.Backends project.

## 1.0.7123 - 2021-12-12
### Changed:
- misc: Sync Config.json default debug config.

## 1.0.7122 - 2021-12-08
### Fixed:
- Fix SUATOM and other texture shader instructions with RZ dest.
  - No known changes in games.

## 1.0.7121 - 2021-12-08
### Changed:
- Remove usage of Mono.Posix.NETStandard accross all projects.
  - Saves ~1.5MB on releases and removes an external C library.

## 1.0.7120 - 2021-12-08
### Fixed:
- Move texture anisotropy check to SetInfo.
  - Improves performance in certain games (mainly UE4) when anisotropic filtering is not set to Auto, such as Shin Megami Tensei V.

## 1.0.7119 - 2021-12-08
### Added:
- Implement remaining shader double-precision instructions.
  - Implements remaining double instructions: DMNMX, DSET and DSETP.
  - Implements remaining double-precision operations on MUFU instruction: RCP64H and RSQ64H.
  - Fix immediate operands on all double-precision instruction.
  - World War Z uses the DSET/DSETP instruction, however it still crashes upon entering gameplay. No other known changes in games.

## 1.0.7118 - 2021-12-08
### Fixed:
- misc: Fix alsoft.ini being present on Linux releases.
  - Removes alsoft.ini from future Linux releases, as it's only required in Windows (see changelog for 1.0.6783).

## 1.0.7117 - 2021-12-08
### Added:
- Implement UHADD8 instruction.
  - Allows No More Heroes and No More Heroes 2: Desperate Struggle to boot.

## 1.0.7116 - 2021-12-05
### Fixed:
- Fix FLO.SH shader instruction with a input of 0.
  - No known changes in games.

## 1.0.7115 - 2021-12-04
### Fixed:
- kernel: Improve GetInfo readability and update to 13.0.0.

## 1.0.7114 - 2021-12-04
### Changed:
- misc: Migrate usage of RuntimeInformation to OperatingSystem.

## 1.0.7113 - 2021-11-30
### Fixed:
- Fix Amiibo hanging since .NET 6 changes.
  - Fixes a regression introduced in 1.0.7111 where scanning an Amiibo would cause the emulator to lock up.

## 1.0.7112 - 2021-11-29
### Fixed:
- Don't blow up everything if a DLC file is moved or renamed.
  - Fixes DLC manager crashing the emulator when DLC files have been moved or renamed, removing the need to delete a DLC.json file when this happens.

## 1.0.7111 - 2021-11-28
### Added:
- infra: Migrate to .NET 6.
  - Migrates projects and CI to .NET 6.
  - May slightly improve performance in some games.

## 1.0.7110 - 2021-11-28
### Fixed:
- kernel: Fix sleep timing accuracy.
  - Fixes softlocks and slow timer in Hyrule Warriors: Definitive Edition.
  - Greatly improves loading in Breath of the Wild 1.0.0.
  - Slightly improves performance in some games.

## 1.0.7109 - 2021-11-28
### Added:
- kernel: Add support for CFI.
  - No known changes in games.

## 1.0.7108 - 2021-11-24
### Added:
- account/ns: Implement 13.0.0+ service calls.
  - Animal Crossing: New Horizons 2.0.0 (and newer) no longer crashes on boot.
  - Dying Light no longer crashes on boot.

## 1.0.7107 - 2021-11-21
### Fixed:
- Better depth range detection.
  - Improves rendering in Bastion.

## 1.0.7106 - 2021-11-15
### Fixed:
- Nickname! - Init Amiibos with Profile's name!
  - Amiibo now use the name of the current user profile when the name of the owner is requested, instead of "No Name".

## 1.0.7105 - 2021-11-14
### Fixed:
- Fix shader integer from/to double conversion. 
  - Fixes the Invalid reinterpret cast from "F32" to "F64" exception that would happen on any shader using I2F or F2I instructions to convert between double and integer types.
  - Allows World War Z to boot into menus (the game is still not playable).

## 1.0.7104 - 2021-11-13
### Fixed:
- Limit Custom Anisotropic Filtering to only fully mipmapped textures.
  - Fixes graphical bugs caused by non-Auto Anisotropic Filtering, affecting games such as Astral Chain or Shin Megami Tensei V.
  - Allows changing Anisotropic Filtering while a game is running.

## 1.0.7103 - 2021-11-10 
### Added:
- Implement DrawTexture functionality.
  - Steel Assault now renders.
  - Final Fantasy VII now goes in-game.
  - Charge Kid rendering improved slightly.

## 1.0.7102 - 2021-11-10
### Fixed:
- Fix direct mouse access checkbox label.
  - Corrects the tooltip for direct mouse access.

## 1.0.7101 - 2021-11-08 
### Added:
- Support shader gl_Color, gl_SecondaryColor and gl_TexCoord built-ins.
  - Fixes black screens on games using OpenGL on the Switch, such as rRootage Reloaded and Dragon Quest III.

## 1.0.7100 - 2021-11-08
### Fixed:
- Fix bindless/global memory elimination with inverted predicates.
  - Fixes lighting issues on Disaster Report 4, and possibly other games.

## 1.0.7099 - 2021-11-08
### Fixed:
- Fix InvocationInfo on geometry shader and bindless default integer const.
  - Fixes a regression introduced in 1.0.7078. Worm Jazz is no longer missing graphics.
  - Fixes some shader compilation errors.
  - On AMD and Intel GPUs, fixes some UE4 games that showed a black screen due to a regression that broke geometry shaders.

## 1.0.7098 - 2021-11-04
### Fixed:
- Ensure syncpoints are released and event handles closed on channel close.
  - Fixes "Cannot allocate a new syncpoint!" error.
  - Fixes games that crashed with an "Out of handles!" exception, such as Legend of Mana.

## 1.0.7097 - 2021-11-03
### Fixed:
- Clamp number of mipmap levels to avoid API errors due to invalid textures.
  - Fixes character mods rendering black textures (such as the one for Mario Party Superstars). No known changes on commercial games.

## 1.0.7096 - 2021-11-01
### Fixed:
- hle: Make Ryujinx.HLE project entirely safe.
  - Followup of the changes in 1.0.7089.

## 1.0.7095 - 2021-11-01
### Fixed:
- When waiting on CPU, do not return a time out error from EventWait.
  - Fixes timeouts introduced in 1.0.7082.
  - Fixes crashes on Tokyo Mirage Sessions #FE Encore.

## 1.0.7094 - 2021-10-29
### Changed:
- ci: Disable macOS x64 build on Appveyor.
  - Reduces Appveyor build time significantly.
  - The platform will still be built on Github Actions.

## 1.0.7093 - 2021-10-28
### Added:
- Add support for fragment shader interlock.
  - Fixes flickering lights in Super Mario Party and Mario Party Superstars.

## 1.0.7092 - 2021-10-28
### Added:
- Add support for the brazilian portuguese language code. 
  - Brazilian Portuguese can now be selected in the "System Language" dropdown.

## 1.0.7091 - 2021-10-24
### Fixed:
- Kernel: Fix inverted condition on permission check of SetMemoryPermission syscall.

## 1.0.7090 - 2021-10-24
### Fixed:
- Preserve image types for shader bindless surface instructions (.D variants).
  - Make format unknown for surface atomic if bindless and not sized.

## 1.0.7089 - 2021-10-24
### Fixed:
- HLE: Improve safety. 
  - Makes timezone implementation safe. Improves various parts of the code that were previously unsafe.
  - Add an util function that handles reading an ASCII string in a safe way.

## 1.0.7088 - 2021-10-24
### Fixed:
- Kernel: Clear pages allocated with SetHeapSize. Previously, all new pages allocated by SetHeapSize were not cleared by the kernel, which could cause weird memory corruption.
  - Add support for custom fill heap and ipc value.

## 1.0.7087 - 2021-10-24
### Fixed:
- Fixup channel submit IOCTL syncpoint parameters.
  - Fixes the parameters that are passed to the "submit" function.
  - No changes expected on games.

## 1.0.7086 - 2021-10-24
### Fixed:
- misc: Fix IVirtualMemoryManager.Fill ignoring value. This is required for future kernel fixes.
  - No changes to emulator functionality.

## 1.0.7085 - 2021-10-23
### Added:
- Kernel: Add resource limit related syscalls.
  - Fix register mapping being wrong for SetResourceLimitLimitValue.
  - Used only by homebrew and system apps.

## 1.0.7084 - 2021-10-23
### Added:
- Kernel: Implement SetMemoryPermission syscall.
  - Fix KMemoryPermission not being an unsigned 32 bits type and
add the "DontCare" bit (used by shared memory, currently unused in
Ryujinx).
  - Used only by homebrew and system apps.

## 1.0.7083 - 2021-10-23
### Added:
- Kernel: Add missing address space check in SetMemoryAttribute syscall.

## 1.0.7082 - 2021-10-19
### Fixed:
- Fix race when EventWait is called and a wait is done on the CPU.
  - Fixes the "Invalid Event at index X" error being printed in some games.
  - Fixes a crash in Marvel Ultimate Alliance 3, New Pokémon Snap (on boot), Persona 5 Scramble when loading the first level, and some games that crashed while playing videos.

## 1.0.7081 - 2021-10-18
### Fixed:
- Fix shader 8-bit and 16-bit STS/STG.
  - Fixes broken interior lighting in The Witcher 3.

## 1.0.7080 - 2021-10-18
### Added:
- Add workaround for Nvidia driver 496.13 shader bug.
  - Fixes flipped upside-down games and a large variety of rendering errors caused by the new drivers.

## 1.0.7079 - 2021-10-18
### Added:
- Add an early 'TailMerge' pass.
  - No changes expected on games.

## 1.0.7078 - 2021-10-18
### Fixed:
- Add initial tessellation shader support.
  - Fixes sand not rendering in Luigi's Mansion 3.

## 1.0.7077 - 2021-10-17
### Fixed:
- Add missing U8/S8 types from shader I2I instruction.
  - Fixes a regression introduced in 1.0.7069 where in some cases an exception would be thrown when emitting code for a shader.

## 1.0.7076 - 2021-10-17
### Fixed:
- Extend bindless elimination to work with masked and shifted handles.
  - Combined with the change in 1.0.7073, makes Hades render almost correctly.
  - Improves rendering in The Witcher 3.

## 1.0.7075 - 2021-10-17
### Added:
- Implement SHF (funnel shift) shader instruction.
  - Improves rendering on Cotton/Guardian Saturn Tribute games, and potentially other games.

## 1.0.7074 - 2021-10-13
### Fixed:
- Fixes a regression introduced on version 1.0.7067 that caused some games using the foreground software keyboard applet and the Mii editor to crash.

## 1.0.7073 - 2021-10-12
### Fixed:
- Fixes an issue of Vulkan (from the guest) draw methods by forcing the state to be dirty when the indexed draw registers are written to.
  - Only affects games using the Vulkan API as other APIs do not use those registers.

## 1.0.7072 - 2021-10-12
### Fixed:
- Enqueue frame before signalling the frame is ready.
  - Fixes performance lowering after some loading transitions in Link's Awakening and Xenoblade DE.

## 1.0.7071 - 2021-10-12
### Changed:
- Don't force scaling on 2D copy sources since it only needs to scale if the texture already exists and was scaled by rendering or something else. 
  - This will also avoid the destination being scaled if the source wasn't. The copy can handle mismatching scales just fine.
  - This prevents scaling artifacts in GMS games, and maybe others (not Super Mario Maker 2, that has another issue).

## 1.0.7070 - 2021-10-12
### Added:
- Added Vp8 codec support.
  - Fixes videos in Diablo II, TY The Tasmanian Tiger and probably more games.

## 1.0.7069 - 2021-10-12
### Changed:
- Rewrite shader decoding stage.
  - Changes the way how shader instructions are decoded by using a separate struct per instruction variant. This may be a bit wasteful, but we beleive it's the best way to avoid some errors, and also to avoid special handling on the instruction implementation.
  - Now all the shader instructions are on the table (not all of them are actually implemented, of course), this should facilitate future work when we need to actually implement those instructions.
  - Fixes inexistent fields in one of the double instructions.
  - Fixes SAT using the FTZ bit in one of the half instruction.
  - Fixes wrong bit in TLD.B AOFFI.
    - Fixes some rendering in Jump Force and probably other games.

## 1.0.7068 - 2021-10-12
### Added:
- Implemented GetConfig of spl service.
  - Fixes startup crash in some homebrews.

## 1.0.7067 - 2021-10-12
### Changed:
- Implemented the inline software keyboard without input pop up dialog in the GUI since a way to display the text is already provided by the games.
  - Supports non-ASCII text, selection, copy and paste, overwrite mode, toggling input (on and off so keyboard players can input text correctly),  the new Calc format used by newer games.
  - Fixes some software keyboard issues in Monster Hunter games and more.

## 1.0.7066 - 2021-10-08
### Changed:
- Optimize the JIT linear scan register allocator.
  - The speed of PPTC compilation is greatly improved, in some cases taking half of the time compared to the previous version.
  - First run or runs with PPTC disabled have also been improved, now taking less time to reach the peak performance.

## 1.0.7065 - 2021-10-07
### Fixed:
- Implement the VIC X8B8G8R8 output pixel format.
  - Fixes missing videos (black screen) on Metroid Dread title screen, menus, new ability guides, area change loading screens and more.

## 1.0.7064 - 2021-10-07
### Fixed:
- Reregister flush actions when taking a buffer's modified range list.
  - Fixes a regression introduced in 1.0.7061, where buffer flush would not happen for data written before buffers were joined.

## 1.0.7063 - 2021-10-05
### Changed:
- Enable branch tightening when PPTC is enabled.
  - Slightly better JIT code generation with PPTC enabled.
  - PPTC cache size was reduced by a few KB as a result.

## 1.0.7062 - 2021-10-05
### Fixed:
- Fixes wrong name size on DisplayInfo structure.
  - Fixes a regression introduced on version 1.0.7041 that caused Dragon Ball Xenoverse 2, and maybe other games to crash on boot.

## 1.0.7061 - 2021-10-04
### Changed:
- Reduces cost for range list creation and inheritance.
  - Fixes potential performance regression introduced on version 1.0.7044.

## 1.0.7060 - 2021-10-04
### Fixed:
- Allow textures to be used without having a sampler pool set.
  - Improves rendering on Cotton Guardian Force Saturn Tribute.

## 1.0.7059 - 2021-10-04
### Fixed:
- Fixes a bug introduced on version 1.0.7052 that could cause a memory leak and eventually a crash playing H264 videos.

## 1.0.7058 - 2021-09-28
### Added:
- Added the ability to signal a write as "precise" on the tracking, which signals a special handler (if present) which can be used to avoid unnecessary flush actions, or maybe even more. For buffers, precise writes specifically do not flush, and instead punch a hole in the modified range list to indicate that the data on GPU has been replaced.
  - Fixed regressions from a previous change (in Mario + Rabbids Kingdom Battle, Rune Factory 4 and more).

## 1.0.7057 - 2021-09-28
### Fixed:
- Force copy when auto-deleting a texture with dependencies.
  - Fixes broken lighting caused by pausing in SMO's Metro Kingdom. May fix some other infrequent issues.

## 1.0.7056 - 2021-09-28
### Fixed:
- Only make render target 2D textures layered if needed.
  - Fixed The Legend of Heroes: Zero no Kiseki which is now playable.

## 1.0.7055 - 2021-09-28
### Added:
- Added a set of optimizations to the HybridAllocator which increases the throughput of LCQ and usually shaves a couple of seconds from boot time.

## 1.0.7054 - 2021-09-28
### Fixed:
- Use normal memory store path for DC ZVA as an optimized way to clear memory in homebrew applications.

## 1.0.7053 - 2021-09-28
### Added:
- Implemented/Stubbed some clkrst service calls which are needed by some homebrew.

## 1.0.7052 - 2021-09-28
### Fixed:
- Use separate contexts per channel and decode frames in DTS order.
  - Decoding multiple videos at once no longer causes image corruption.
  - As a result of the hack removal, H264 video playback should be smoother now, without duplicate frames (please note that the game may still decide to duplicate frames or not present anything if the decoding is too slow, YMMV).
  - Some minor issues like a few games flashing a green frame when the video starts have also been fixed.
  - Add missing field_pic_order_in_frame_present_flag flag to the stream PPS. Fixes decoding errors on Layton's Mystery Journey, but the video is still not rendered properly due to VIC Issues.
  - Add new Trim method on NVDEC's SurfaceCache to allow cached frames to be freed. It is called every time a context is destroyed. Cache access now uses a lock to make it thread safe, as Trim may be called from outside the NVDEC thread.
    - Fixed some videos in Hatsune Miku: Project DIVA Mega Mix and No More Heroes 3.

## 1.0.7051 - 2021-09-28
### Fixed:
- Fixed PTC count table relocation patching introduced in a previous change where by 2 different count table entry addresses were used for LCQ functions.

## 1.0.7050 - 2021-09-28
### Added:
- Stubbed some irs service calls which are needed to get some games playable or bootable.
  - Night Vision and Spy Alarm are now bootable (but still unplayable due to the lack of the IR data).

## 1.0.7049 - 2021-09-28
### Fixed:
- Fixed an issue where scales might not be properly updated on games that uses compute.
  - Fixed resolution scale issues on Ni no Kuni 2.

## 1.0.7048 - 2021-09-28
### Fixed:
- Updated the game compatibility infos in README file.

## 1.0.7047 - 2021-09-19
### Added:
- Adds a method to PhysicalMemory that attempts to write all cached resources directly, so that memory tracking can be avoided. The goal of this is both to avoid flushing buffer data, and to avoid raising the sequence number when data is written, which causes buffer and texture handles to be re-checked.
  - Improves performance on Xenoblade 2 and DE, which were flushing buffer data on the GPU thread when trying to write compute data.
  - May improve performance in other games that write SSBOs from compute, and update data in the same/nearby pages often.

## 1.0.7046 - 2021-09-19
### Added:
- Implements an augmented interval tree based off of the existing TreeDictionary and uses it for the MultiRangeList. This greatly speeds up texture overlap checks, as they can't use the non-overlapping fast path that buffers and tracking handles can use. Like the tree dictionary, it is based on a red-black tree and is self balancing.
  - Speed up texture/view creation and notifying textures that they have been unmapped, which might reduce stuttering in unreal engine games.

## 1.0.7045 - 2021-09-19
### Added:
- Uses new subgroup extension (that AFAIK were introduced for SPIR-V) if the old ARB_shader_ballot extension is not supported. This is required for Vulkan. Additionally, this benefits Intel on OpenGL as they don't support the ARB_shader_ballot extension on the proprietary driver for some reason, but does support the new extensions.
  - Astral Chain and probably some other games works on Intel OpenGL now.

## 1.0.7044 - 2021-09-19
### Added:
- Changes the RangeList to cache the Address and EndAddress within the list itself rather than accessing them from the object's properties.
  - Greatly improves performance in Super Mario Odyssey (~1.25x), Xenoblade (required some WIP code for now) and most other GPU limited games.
  - Improvement will generally depend on how many buffers the game binds and how many draws it does.

## 1.0.7043 - 2021-09-19
### Fixed:
- Set texture/image bindings in place rather than allocating and passing an array.

## 1.0.7042 - 2021-09-19
### Fixed:
- Amadeus: Fixes regression from 1.7040 on ListAudioDeviceName.

## 1.0.7041 - 2021-09-19
### Fixed:
- Unified values and accurated implementation of resolutions in vi service.
  - am calls GetDefaultDisplayResolution / GetDefaultDisplayResolutionChangeEvent have more informations on what the service does.
  - vi:u/vi:m/vi:s GetDisplayService are now accurate.
    - IApplicationDisplay GetRelayService, GetSystemDisplayService, GetManagerDisplayService, GetIndirectDisplayTransactionService, ListDisplays, OpenDisplay, OpenDefaultDisplay, CloseDisplay, GetDisplayResolution are now properly implemented.
  - Some other calls are cleaned or have extra checks accordingly to RE.
- Additionnaly, IFriendService have some wrong aligned things, and `pm:info` service placeholder was missing. 

## 1.0.7040 - 2021-09-19
### Added:
- Amadeus: Implements all the changes made with REV10 (audio service) on 13.0.0.

## 1.0.7039 - 2021-09-18
### Fixed:
- Pause/Resume entry in Action menu now is properly disabled after switching to fullscreen mode.
- An issue with the Pause changes was fixed that caused Ryujinx to not be properly close.

## 1.0.7038 - 2021-09-15
### Fixed:
- Fixes a regression introduced in 1.0.7034, when you try to extract data from games dump.

## 1.0.7037 - 2021-09-14
### Fixed:
- FPS monitor now shows instantaneous FPS rather than any form of weighted average.
- Frametime has been added as a metric with a 2 decimal place precision factor.
- All performance metrics now update every 750ms rather than 1000ms.

## 1.0.7036 - 2021-09-14
### Fixed:
- Add Linux Unicorn (for Tests) patch and description.

## 1.0.7035 - 2021-09-14
### Fixed:
- HOS project cleanup.

## 1.0.7034 - 2021-09-14
### Fixed:
- Replace FileChooserDialog with FileChooserNative.
  - Fixes a crash when you add a drive as folder.

## 1.0.7033 - 2021-09-13
### Fixed:
- Refactor PtcInfo.
  - Reduces the coupling of PtcInfo and the backend by moving relocation tracking to the backend. RelocEntrys remains as RelocEntrys through out the pipeline until it actually needs to be written to the PTC streams. Keeping this representation makes inspecting and manipulating relocations after compilations less painful.

## 1.0.7032 - 2021-09-11
### Fixed:
- Account for negative strides on DMA copy.
  - Math.Abs is used on the stride to calculate the size, to ensure it is positive.
  - If stride is negative, the base offset is adjusted to the real start offset of the copy.
- Changed the flush call on InlineToMemory to use the GPU memory manager rather than the physical one, to account for non-contiguous memory.
  - Idol Days no longer crashes when trying to open the log or load/save the game, although it does have other issues there that don't seem to be caused by this change.

## 1.0.7031 - 2021-09-11
### Added:
- Implements GetVaRegions in nv service.
  - Fixes a crash on Quake which can progress further, now it crashes due to Sockets issues.

## 1.0.7030 - 2021-09-11
### Fixed:
- Fixes ICommonStateGetter GetDefaultDisplayResolution returned resolution while using Docked Mode.
  - Fixes Tsukihime -A piece of blue glass moon- rendering and probably some other games.

## 1.0.7029 - 2021-09-11
### Added:
- Added "Pause Emulation" option which could be found at menu "Actions > Pause Emulation" or using F5 hotkey.

## 1.0.7028 - 2021-09-11
### Fixed:
- Lift textures in the AutoDeleteCache for all modifications.
  - Fixes lighting breaking when switching levels in Tony Hawk Pro Skater 1+2 and potentially some more UE4 games.

## 1.0.7027 - 2021-09-11
### Fixed:
- Fixes single quote key incorrectly mapped in our GTK3 code.

## 1.0.7026 - 2021-09-11
### Fixed:
- Fixes time played staying at 0 second when "Stop Emulation" is pressed.

## 1.0.7025 - 2021-09-11
### Fixed:
- Remove error dialog when files encountered weren't of a valid type.

## 1.0.7024 - 2021-09-02
### Fixed:
- Fixes shaders using the TXQ instruction failing to compile if more than 2 dimensions are being read.
  - Fixes some (but not all) lighting issues on Tony Hawk's Pro Skater 1 + 2, and other UE4 games.

## 1.0.7023 - 2021-08-31
### Added:
- Implements support for the shader image atomic instructions.
  - Fixes object interaction not animating on Yoshi Crafted World in some levels.
  - Fixes missing lighting on several UE4 games, such as Bravely Default 2, Tony Hawk's Pro Skater 1 + 2, No More Heroes 3, Densha de Go!! and more.
  - Fixes missing facial animations on several UE4 games, such as Bravely Default 2 and Trials of Mana.

## 1.0.7022 - 2021-08-30
### Fixed:
- Fixes shader shuffle up instruction, when the source thread ID is out of range.
  - Fixes vertex explosions on Marvel Ultimate Alliance 3.

## 1.0.7021 - 2021-08-29
### Fixed:
- Improves handling of multi-draw with indirect count on HLE macros, for cases where the start draw is non-zero.
  - No visible changes expected, as no game is known to hit this case so far.

## 1.0.7020 - 2021-08-29
### Fixed:
- Fixes a bug that would cause textures that do not overlap, but is assumed to overlap because gaps on the memory region where the data is located is not taken into account, to be removed from the cache, causing data loss.
  - Fixes "white screen" issue on several Unreal Engine 4 games, including Yoshi Crafted World, Disaster Report 4, No More Heroes 3, JUMP FORCE Deluxe Edition and more.
  - Some instances of glowing objects were also fixed.
  - Note that this does not fix all texture issues with those games, but they are greatly improved now.

## 1.0.7019 - 2021-08-29
### Fixed:
- Fixes a case where threads would not wait until the GPU written data is flushed if more than one thread was trying to access it at the same time.
  - Fixes a regression introduced on version 1.0.7016 that caused Catherine Full Body to crash randomly with guest memory corruption.

## 1.0.7018 - 2021-08-27
### Changed:
- Avoid redundant texture scale updates.
  - Minor performance improvements on some games, most notably Xenoblade Chronicles: Definitive Edition.

## 1.0.7017 - 2021-08-26
### Added:
- Added support for attribute indexing, required by Donkey Kong Country Tropical Freeze, and a few other games.
  - This improves the rendering on some Donkey Kong Country Tropical Freeze levels.

## 1.0.7016 - 2021-08-26
### Added:
- The Graphic Abstraction Layer is now multithreaded.
  - Allows more finer controls over vendor drivers.
    - Allows to disable NVIDIA threaded optimization and remove stuttering related to it.
  - Allows multithreaded shader compilation at runtime and reduce stuttering when multiple shaders can be build at the same time.
  - This can be enabled or disabled via an option in the settings called "Backend Threading". The default option is "Auto", which always means on right now, but could change for some vendors or backends.
  - Consult the [pull request](https://github.com/ryujinx-mirror/ryujinx/pull/2501) for more details.

## 1.0.7015 - 2021-08-26
### Added:
- Implement MSR instruction for A32.
  - Pocket Rumble is now playable.

## 1.0.7014 - 2021-08-26
### Added:
- Added support for HLE macros allowing to improve macro efficiency.
  - For now, the only HLE macro function implemented is MultiDrawElementsIndirectCount, used by Monster Hunter Rise.

## 1.0.7013 - 2021-08-26
### Fixed:
- Remove missing unicorn files included in the Tests project.
  - Fixes build warning.

## 1.0.7012 - 2021-08-26
### Fixed:
- Updates to the 0.13.3 bugfix release of LibHac and removes the workaround introduced in PR #2576.
  - Fixes regression: SD card saves will now be placed in /Nintendo/save instead of /save/Nintendo.

## 1.0.7011 - 2021-08-26
### Changed:
- Added fallbacks for all Audio backends.
  - SDL2 is now the default audio backend, then OpenAL, then SoundIO. 

## 1.0.7010 - 2021-08-26
### Fixed:
- Swap BGR565 components by changing the format.
  - Fixes regression on some homebrew using a BGR565 framebuffer that stopped rendering.

## 1.0.7009 - 2021-08-26
### Fixed:
- Update to Ryujinx.SDL2-CS 2.0.17 (Fix runtime issues on Gentoo)

## 1.0.7008 - 2021-08-20
### Fixed:
- Adds a workaround for a Libhac issue where an exception is thrown when trying to delete an inexistent folder recursively.

## 1.0.7007 - 2021-08-20
### Fixed:
- Fixes spelling on some UI dialogs.

## 1.0.7006 - 2021-08-20
### Fixed:
- Allows swapping R and B components on BGR565 and BGRA5551 texture formats.
  - Fixes swapped red/blue issue on Pokkén Tournament DX.
  - Fixes swapped red/blue issue on Super Smash Bros Ultimate, on the stages with large monitor screens, such as Pokémon Stadium and the Boxing Ring.

## 1.0.7005 - 2021-08-20
### Fixed:
- Changes disabled vertex attribute value to (0, 0, 0, 1).
  - Fixes a regression in Super Mario Odyssey that caused some plants in the Wooded Kingdom to render black.

## 1.0.7004 - 2021-08-20
### Changed:
- Removes overlapping textures from the texture cache, if the overlapping texture is not view compatible with the existing one, and its data has been modified.
  - Greatly reduces memory usage in Xenoblade Chronicles: Definitive Edition and Xenoblade Chronicles 2, in areas with high usage (like Gormott).
  - Fixes crashes and performance issues caused by the system running out of memory.

## 1.0.7003 - 2021-08-20
### Fixed:
- Fixes errors from reading the SD card when encryption keys have changed.
  - This caused an error when launching Super Smash Bros. Ultimate with an old SD card save directory.

## 1.0.7002 - 2021-08-20
### Fixed:
- Fixes wrong base level calculation when copying data to slices of 3D textures.
  - Fixes 3D texture data not being properly updated in some rare cases.
  - The bug was observed on Super Mario Odyssey. Other games using 3D textures might be affected too.

## 1.0.7001 - 2021-08-20
### Fixed:
- Fixes an assert that was happening on debug builds since version 1.0.7000, due to an optimization creating a constant operand with the wrong type.
  - Should not have any visible effect on games as the bug did not cause wrong code generation.

## 1.0.7000 - 2021-08-17
### Changed:
- Optimizes JIT memory allocations by using an arena allocator, along with other optimizations.
  - Greatly improves PPTC compilation speeds, reducing the total duration by around 20%-60% depending on the game.
  - Lowers memory usage when rebuilding PPTC.
  - Reduces stutters and time taken to reach peak performance with PPTC disabled and on the first run (before PPTC is built).
  - Fixes a bug that could cause incorrect code to be generated in some rare cases, where a shift value was overwritten with an incorrect value.

## 1.0.6999 - 2021-08-17
### Fixed:
- Fixes a crash that could happen when mounting save data if the keys or SD seed changed.

## 1.0.6998 - 2021-08-17
### Fixed:
- Allows transform feedback data to be flushed when accessed from CPU.
  - Fixes some vertex explosions on SNK Heroines: Tag Team Frenzy.

## 1.0.6997 - 2021-08-12
### Changed:
- Updates the LibHac dependency to version 0.13.1. This improves the accuracy of the emulator file system implementation, and solve some file system and save related issues on games. See below for a more detailed list of changes.
  - Refactor `FsSrv` to match the official refactoring done in FS.
  - Change how the `Horizon` and `HorizonClient` classes are handled. Each client created represents a different process with its own process ID and client state.
  - Add FS access control to handle permissions for FS service method calls.
  - Add FS program registry to keep track of the program ID, location and permissions of each process.
  - Add FS program index map info manager to track the program IDs and indexes of multi-application programs.
  - Add all FS IPC interfaces.
  - Rewrite `Fs.Fsa` code to be more accurate.
  - Rewrite a lot of `FsSrv` code to be more accurate.
  - Extend directory save data to store `SaveDataExtraData`
  - Extend directory save data to lock the save directory to allow only one accessor at a time.
  - Improve waiting and retrying when encountering access issues in `LocalFileSystem` and `DirectorySaveDataFileSystem`.
  - More `IFileSystemProxy` methods should work now.
  - Probably a bunch more stuff.

## 1.0.6996 - 2021-08-12
### Changed
- Improves error message when the prod.keys file is present, but the keys are outdated.
  - Now the "Ryujinx was unable to parse the provided firmware. This is usually caused by outdated keys." message is presented if the keys are outdated when installing a new firmware.

## 1.0.6995 - 2021-08-12
### Fixed
- Fixes a regression that could cause the same compute shader to be compiled over and over again when shader cache was enabled, causing a large performance drop in some games.
  - Fixes performance regression in Hyrule Warriors: Ages of Calamity, and probably other games that uses compute shaders.

## 1.0.6994 - 2021-08-11
### Fixed
- Reverts changes from version 1.0.6987.
  - Fixes regression on The Legend of Zelda: Breath of the Wild (flickering trees) and Monster Hunter Stories 2: Wings of Ruin (invisible huts), probably more.

## 1.0.6993 - 2021-08-11
### Fixed
- Initialize render scales to 1 on the OpenGL backend.
  - Fixes a regression introduced on 1.0.6989 where some games would have a solid colour instead of the correct texture due to the scale value being 0.

## 1.0.6992 - 2021-08-11
### Fixed
- Unify GpuAccessorBase and TextureDescriptorCapableGpuAccessor.
  - Fixes regression introduced in 1.0.6990 that was causing shader TDR.

## 1.0.6991 - 2021-08-11
### Fixed
- Workaround for cubemap view data upload bug on Intel.
  - Fixes eyes rendering of Hatsune Miku.

## 1.0.6990 - 2021-08-11
### Fixed
- Fixes another bug on Intel where gl_FrontFacing returns incorrect values (it seems to be flipped?), the solution here is casting the bool to float and then bitcasting to integer and checking if its non-zero.
  - Fixes rendering of mario mustache on Super Mario Odyssey on Intel (affects both Vulkan and OpenGL).
- Added 2 new functions to the IGpuAccessor interface, used to check if the host has the vector indexing bug (present on AMD) and the front facing bug (present on Intel).
- LDC now uses vector indexing of the form data[offset >> 2][offset & 3] on NVIDIA and Intel, which produces better codegen (and makes shaders easier to read).
- Functions that just returns a host capability on GpuAcessor and CachedGpuAccessor were moved to a common base class to avoid duplication and bugs. In fact, one of them (the one to check the texture shadow LOD capability) was missing from the CachedGpuAccessor, which likely means those shaders were broken on shader cache rebuild on the vendors/drivers that does not support the extension (as the default interface implementation returns true). Now this is fixed.

## 1.0.6989 - 2021-08-11
### Changed
- Use "Undesired" scale mode for certain textures rather than blacklisting.
  - Games that reuse depth stencil between a lot of unrelated draws to textures of different sizes will no longer blacklist from scaling forever. (Bayonetta 2)
  - Textures that are detected as being atlas-like or dof-like will not be locked out of scaling forever - if a depth stencil target is bound when drawing, they can gain res scale. This might result in more 3D UI elements seeing resolution scale. (Xenoblade DE menus, though they're still heavily antialiased)
  - Some other games that randomly blacklist to 1x, or don't scale at all may now work.
  - The approach for blur/dof texture detection has been updated to take into account width alignment, as it can greatly alter the aspect ratio. This fixes small textures failing to detect, which would have cascaded across mipmap texture views and forced textures to scale.
  - Render target scale is now only sent to the backend when its value has changed, rather than each time the scales are evaluated.
  - Scissor and Viewport are only recalculated when the value changes too.

## 1.0.6988 - 2021-08-11
### Fixed
- Make sure attributes used on subsequent shader stages are initialized.
  - Fixes Shadows not working on Zelda Link's awakening (Intel and AMD).
  - Fixes Hatsune Miku eyes being black (Intel and AMD).
  - Fixes Shader failing to compile on Yo-kai Watch 4 (all vendors, no visible effect though?).
  - Probably more...

## 1.0.6987 - 2021-08-11
### Changed
- Improving the speed of vertex buffer updates in some specific cases, where the vertex buffer size is very large. This calculates the vertex buffer size from the max index on the index buffer when the game provided size is too large (above a given threshold), and when the index buffer is small enough.
  - This improves the speed of Super Mario Galaxy, however it appears that this game had a performance regression after range tracking, so the speed seems mostly back to what it was before that. The menu transition however is faster than what it was even before range tracking.

## 1.0.6986 - 2021-08-11
### Fixed
- Do not dirty memory tracking region handles if they are partially unmapped.
  - Fixes crash when loading a save on Dragon Quest XI S.

## 1.0.6985 - 2021-08-11
### Changed
- Replace BGRA and scale uniforms with a uniform block (required for Vulkan).

## 1.0.6984 - 2021-08-11
### Changed
- Improves ServiceNotImplementedException by removing the need to pass in whether the command is a Tipc command or a Hipc command to the exception constructor.

## 1.0.6983 - 2021-08-11
### Added
- Use a new approach for shader BRX targets:
  - Fixes several graphical glitches on Hatsune Miku Project DIVA MEGA 39's.
  - Fixes Bowwow being a white glowing ball on Zelda's Link Awakening.
  - Fixes reflection bug on Cadence of Hyrule.
  - And more...

## 1.0.6982 - 2021-08-04
### Added
- Implement (non-HD) vibrations support:
  - Implement vibration calls of hid service accurately to reverse engineering.
  - You can enable/disable rumble in the controller settings window.
  - HD rumble values are converted to non-HD vibration values, you can choose the multipliers for rumble in the controller settings window.
- Tested working controller are DualSense (PS5), Xbox One, Xbox 360, ProController but working on probably more controllers. Users reports rumble doesn't work on real JoyCons, this will be fixed later if confirmed.

## 1.0.6981 - 2021-08-04
### Fixed:
- Fixes a regression introduced in #2411:
  - Alt key no longer focuses the menu bar. That functionality wasn't really relevant to the original intent of #2411, and the main purpose of the PR remains usable. Focus on Alt is something will might fix later.
  - This reworks the entire flow of how the "Show UI" hotkey works. Note that it is specifically a "Show UI" hotkey, and hiding the UI still has to be done via the Actions menu, as we don't want users to trigger it by accident:
  - The key is now configurable.
  - The key now defaults to F4. Several annoyances was found with Alt, as it's part of the language-switching combination and also getting in the way of taking a (regular, non-Ryujinx) screenshot. Something that's not commonly used made more sense. Of course, due to the previous point above, users are welcome to switch it back to Alt if they are so inclined.
  - Fixing a problem with the implementation of the KeyboardHotkeyState enum which would cause it to break were we to add any more hotkeys.

## 1.0.6980 - 2021-08-04
### Fixes:
- XInput devices provides axis data with square ranges (ie +-32767). Moving the sticks to diagonal ends means that it will have the max range values for both axes. This causes fast motion of player characters in games that do not clamp their input.
  - This fixes it by clamping the input to a circle. Fixes The Legend of Zelda: Skyward Sword HD when Link sprinting without using up stamina when the controller sticks are at diagonals.

## 1.0.6979 - 2021-08-04
### Fixes:
- Makes more copies respect non-contiguous GPU virtual memory regions, instead of assuming it is contiguous. After this, the main thing missing will be support for non-contiguous buffers, which will be more complicated, and will be done on a separate PR.
- This also fixes a bug that noticed on I2M, where the copy would be incorrect for block linear textures, if destination X was not a multiple of 16. This was fixed by aligning the start of the vector copy, which should start and end of a multiple of 16. We are not aware of any games that uses it with a X value that is not a multiple of 16 though (it is usually 0), so maybe nothing was affected by this bug.

## 1.0.6978 - 2021-08-04
### Added:
- Improving reporting for cheats and updating the cheat instructions to match Atmosphere's implementation. Those are:
  - A warning is now issued when the cheat tries to write to code region instead of requiring log analysis in debug mode.
  - The cheat names are now printed in the log for better error detection and support.
  - Implemented new addressing modes Alias and Aslr.
  - Implemented else instruction for conditionals.
- Also, with "PPTC with ExeFS Patching" merged, it is now safe to enable cheats to write to code regions (with the aforementioned warning). This is a preparation for the upcoming full support, but does not mean that all cheats will work (some of them may start working).

## 1.0.6977 - 2021-08-04
### Fixed:
- Fixes some audio related issues that caused the emulator to hang or crash at exit, by making audio backend disposal thread safe.
  - Fixes deadlock (causing the emulator to not respond) at exit with the OpenAL backend.
  - Fixes crash at exit with the SDL2 backend due to a double free.
  - Fixes a crash at exit (only when the emulator window was closed) caused by a NRE on window disposal.

## 1.0.6976 - 2021-07-24
### Added:
- Implement an option to hide status and menu bar
  - The option can be found in the Actions menu. You can press alt to make them appears again.

## 1.0.6975 - 2021-07-24
### Fixed
- Fixes a bug where the right JoyCon is not retrieved from Cemuhook and that the right JoyCon motion shared memory is cleared after update.
  - This should allow right JoyCon to work in The Legend of Zelda: Skyward Sword HD.

## 1.0.6974 - 2021-07-19
### Changed:
- Further optimizes texture and buffer flush, by reducing the amount of data copies required.
  - Texture layout conversion methods can now write to guest memory directly, in some cases.
  - Methods to get data from GPU now returns the span of the range if a persistent buffer is being used, instead of allocating and doing a copy.
  - Improves the performance on The Legend of Zelda: Skyward Sword, Pokémon Sword/Shield, and likely other games that does texture flushes every frame.

## 1.0.6973 - 2021-07-18
### Changed:
- Fixes a performance regression introduced on version 1.0.6970, by switching to the old flush method on affected drivers.
  - Only Linux AMD drivers and the Intel Windows and Linux drivers are affected, NVIDIA is not affected at all.
  - For the affected drivers, The Legend of Zelda: Skyward Sword HD and the other affected games are as fast as they were before.

## 1.0.6972 - 2021-07-18
### Changed:
- Improves drastically the code generation of the audren's DSP.
  - This may reduce CPU usage on the audio processing side and result in a better CPU time allocation.

## 1.0.6971 - 2021-07-18
### Changed:
- Improves shader tools command line by adding support for target language and API.
  - No direct user changes.

## 1.0.6970 - 2021-07-16
### Changed:
- Improves performance of buffer and texture flushes by using a different approach.
  - Greatly improves performance of The Legend of Zelda: Skyward Sword HD.
  - Pokémon Sword/Shield is also improved, however it is still slower than the LDN 2.3 build.
  - Xenoblade Chronicles 2 is sligthly improved.
  - Any other game that does a lot of flushes per frame should be faster aswell.

## 1.0.6969 - 2021-07-14
### Fixed:
- Fixes a bug where data on textures copied using the DMA engine could be lost.
  - Fixes texture corruption (usually visible as black portraits or garbled sprites) on Legend of Mana. Other games might be affected too.

## 1.0.6968 - 2021-07-14
### Fixed:
- Allows draws and compute dispatch without a texture pool and sampler pool being set.
  - Fixes regression on Tales of Vesperia, where the game would crash at boot.
  - An error will be logged if the game attempts to draw with texture or image access without having the pools set.

## 1.0.6967 - 2021-07-14
### Fixed:
- Fixes a bug where the transfer memory handle was not being closed on nvservices, resulting in a handle leak and failure to create a transfer memory at the same region on the guest.

## 1.0.6966 - 2021-07-13
### Changed:
- Revert LibHac update
  - There have been multiple reports of save being destroyed. The issue is being investigated and the update will be rolled again once fixed.

## 1.0.6965 - 2021-07-13
### Fixed:
- Fixes build error of the headless project caused by changes on version 1.0.6964.

## 1.0.6964 - 2021-07-13
### Changed:
- Updates the LibHac dependency to version 0.13.1. This improves the accuracy of the emulator file system implementation, and solve some file system and save related issues on games. See below for a more detailed list of changes.
  - Refactor `FsSrv` to match the official refactoring done in FS.
  - Change how the `Horizon` and `HorizonClient` classes are handled. Each client created represents a different process with its own process ID and client state.
  - Add FS access control to handle permissions for FS service method calls.
  - Add FS program registry to keep track of the program ID, location and permissions of each process.
  - Add FS program index map info manager to track the program IDs and indexes of multi-application programs.
  - Add all FS IPC interfaces.
  - Rewrite `Fs.Fsa` code to be more accurate.
  - Rewrite a lot of `FsSrv` code to be more accurate.
  - Extend directory save data to store `SaveDataExtraData`
  - Extend directory save data to lock the save directory to allow only one accessor at a time.
  - Improve waiting and retrying when encountering access issues in `LocalFileSystem` and `DirectorySaveDataFileSystem`.
  - More `IFileSystemProxy` methods should work now.
  - Probably a bunch more stuff.

## 1.0.6963 - 2021-07-12
### Changed:
- Optimizes the GPU Inline-to-Memory engine transfer operations.
  - Might reduce stutters and improve performance a little on games using the OpenGL API on the Switch.

## 1.0.6962 - 2021-07-12
### Fixed:
- Fixes a regression introduced on version 1.0.6957 that caused shaders using rectangle textures to fail to build in some cases.
  - The Touryst now renders properly once again.

## 1.0.6961 - 2021-07-11
### Changed:
- Refactor GPU 3D engine to more closely match the hardware, in addition to fixing bugs.
  - Now all the 3D engine state is per-channel, rather than shared. This completes the work started with the initial GPU channel support.
  - New state modification tracking and host state update method, cleaner and more efficient than the old one.
  - Optimized `DeviceState` register read and write functions.
  - Proper channel state initialization using the same values as the official OS, instead of guessed values.
  - Fixes a bug where the host state was not being updated on changes to the `YNegate` register.
    - Fixes upside down rendering on Cat Girl Without Salad: Amuse Bouche, Dragon Quest Builders, 20XX, Asterix & Obelix XXL2, BLADE ARCUS Rebellion from Shining, and many more.
    - Fixes black screen on Game Tengoku CruisinMix Special (happens when you start a new game). The title can now be considered playable with minor transparency issues.
  - Implement missing `PrimitiveTypeOverrideEnable` register (thanks to ByLaws for testing).
    - Fixes Turok 2 menus rendering.

## 1.0.6960 - 2021-07-11
### Fixed:
- Fixes a bug on the kernel virtual memory allocation function, where an address outside of the requested range could be returned.
  - Fixes crashes on Disaster Report 4, Garfield Kart Furious Racing, and likely more games that crashes/aborts early in the boot process.

## 1.0.6959 - 2021-07-10
### Added:
- Implements CreateApplicationAndRequestToStart of am service.
  - Allow games to restart by themselves if needed. You can now change the Super Smash Bros. Ultimate language in game (Note: Restart doesn't work with OpenAL audio backend due to another issue).

## 1.0.6958 - 2021-07-10
### Fixed:
- Fixes some misc bugs in writable region write-back.
  - No changes expected on games.

## 1.0.6957 - 2021-07-08
### Fixed:
- Fixes resolution scaling when `textureSize` is used with a scaled texture on a compute or pixel shader.
  - Fixes glitches caused by resolution scaling on Monster Hunter Stories 2: Wings of Ruin (visible in battles and a few other places), Monster Hunter Rise (character selection screen on the demo, and a few other places on the full game), and likely more games.
  - Note: Monster Hunter Rise is still not properly scaled in-game, using a resolution mod is still recommended for the best results.

## 1.0.6956 - 2021-07-07
### Fixed:
- Fixes a regression introduced on version 1.0.6894 (POWER update) that caused some games to crash with a invalid memory region exception.
  - Wonder Boy: The Dragon's Trap, FINAL FANTASY X/X-2 HD Remaster and more are once again playable.

## 1.0.6955 - 2021-07-07
### Changed:
- Refactored GPU 2D, DMA, I2M and Compute engines emulation code to more closely match real hardware, with state structures now being auto-generated from official headers.
  - No changes expected on games.

## 1.0.6954 - 2021-07-06
### Added:
- A new front-end has been added to Ryujinx: Ryujinx.Headless.SDL2.
  - This is a minimalist variant of the emulator without a GUI that can be started and configured via command line.
  - **This is available on a separated release zip.** (see **ryujinx-headless-sdl2-{version}** variants on Appveyor)
  - **This doesn't have any updater.**
  - For more information on the options available run it with ``--help``.

## 1.0.6953 - 2021-07-06
### Added:
- Allows the target language and API to be passed to the translator. Will be used by the Vulkan and SPIR-V backend in the future.

## 1.0.6952 - 2021-07-06
### Added:
- Adds a portable screenshot folder to save images when portable mode is enabled. Also adds a few error checks.

## 1.0.6951 - 2021-07-06
### Fixed:
- Fixes an issue in GetShrinkedGamepadName which crash the controller window if the controller name is equal to the limited length.

## 1.0.6950 - 2021-07-06
### Fixed:
- Fixes nifm service where IsDynamicDnsEnabled isn't supported on Linux.

## 1.0.6949 - 2021-07-06
### Fixed:
- Fixes some inconsistencies introduced in aoc service (1.0.6942).
  - Fixes booting issues of Fire Emblem Three House, Super Robot Wars, Diablo 3 and more games.

## 1.0.6948 - 2021-07-06
### Fixed:
- Start a game with -f argument doesn't tick "Start Games in Fullscreen Mode" anymore.

## 1.0.6947 - 2021-07-06
### Added:
- Implement 12.0.0 hwopus service calls InitializeEx and GetWorkBufferSizeEx.
  - Games built with 12.0.0+ SDK and using hwopus service are now bootable/playable.

## 1.0.6946 - 2021-07-03
### Fixed:
- Honour copy dependencies when switching render target.
  - Fixes UI not being rendered on the Mii Editor (only AMD and Intel Windows drivers are affected).
  - Fixes dialog boxes and menus not rendering on New Pokémon Snap (only AMD and Intel Windows drivers are affected).

## 1.0.6945 - 2021-06-29
### Fixed:
- Fixes a wrong check introduced in 1.0.6942.
  - Pokémon Sword / Shield (and probably other games) can now use DLCs again.

## 1.0.6944 - 2021-06-29
### Fixed:
- Fixes IPC sessions not being disposed at emulation end.
  - This fix possible leaks (memory, opened files, ect) at emulation end.

## 1.0.6943 - 2021-06-29
### Added:
- Initial support for separate GPU address spaces was added.
  - Super Smash Bros Ultimate story mode is now playable.

## 1.0.6942 - 2021-06-29
### Changed:
- Cleanup AOC service and implement some stub calls in AM service.
  - Tony Hawk’s Pro Skater 1+2 is now bootable.

## 1.0.6941 - 2021-06-28
### Added:
- Support for taking screenshot was added.
  - You can take screenshot by pressing F8.
  - Screenshots are saved in your pictures folder in the "Ryujinx" subdirectory.
  - This support resolution scaling. (example: if you configured a 8K resolution your screenshot will be 8K)

## 1.0.6940 - 2021-06-28
### Added:
- Support for running the Mii Editor applet was added.
  - Requires installed firmware 4.0.0+.
  - Mii Editor applet must be launched outside of a game.

## 1.0.6939 - 2021-06-25
### Changed:
- Implements custom (greater than 1) line width support, along with line smoothing support.
  - Fixes incorrect line thickness on the grid that shows on Mario Golf: Super Rush.

## 1.0.6938 - 2021-06-25
### Fixed:
- Fixes unwritten shader output values, which was previously using a value of 0 instead of 1.
  - Fixes missing/invisible geometry on Monster Hunter Stories 2: Wings of Ruin (Trial Version).

## 1.0.6937 - 2021-06-24
### Fixed:
- Fixes some shader compilation failures on shaders using texture depth compare with a LOD parameter and 2D or Cube array textures. Those shaders requires an extension, which is now enabled. On GPUs where the extension is not supported, the LOD parameter is removed.
  - Fixes lighting issues on Mario Golf Super Rush.

## 1.0.6936 - 2021-06-24
### Fixed:
- Stubs GetAlbumFileList0AafeAruidDeprecated and GetAlbumFileList3AaeAruid in caps service.
  - World of Light in Super Smash Bros. Ultimate is now playable.

## 1.0.6935 - 2021-06-23
### Changed:
- Improves game searching when you start typing something while browsing the game list. You can now searching by TitleID and keyword (For example, allows to type "odyssey" and find Super Mario Odyssey).

## 1.0.6934 - 2021-06-23 
### Changed:
- Implements direct mouse support, similar to the direct keyboard support. This enables games and homebrew that support USB mouse to have that option enabled.
  - Touchscreen is disabled when direct mouse is enabled. Also, the real cursor is hidden while inside the client area (GTK seems to reset the mouse cursor state often, so you will see the mouse pop in once in a short while).

## 1.0.6933 - 2021-06-23
### Changed:
- Implementing supports for multiple GPU channels, enabling games to have separate GPU state. This is usually used when videos are played, as some games creates a separate channel for that. Before this change, a single channel was used for everything, which resulted in crashes as the state ended being corrupted.
  - Fixes rendering of Doukoku Shoshite..., Skullgirls 2nd Encore, COTTOn Reboot!, Dragonball Xenoverse 2 and more.

## 1.0.6932 - 2021-06-23
### Changed:
- This greatly speeds up games that constantly resize buffers, and removes stuttering on games that resize large buffers occasionally.
  - Large improvement on Super Mario 3D All-Stars. Slowdown now is just due to the creation of the OGL buffer resources. (#1663 needed for best performance)
  - Improvement to Hyrule Warriors: AoC, and UE4 games. These games still stutter in general due to texture creation/loading.
  - Small improvement to other games, potential 1-frame stutters avoided.
  - ForceSynchronizeMemory, which was added with POWER, is no longer needed. Some tests have been added for the MultiRegionHandle.

## 1.0.6931 - 2021-06-23
### Changed:
- Implements GetDeviceNickName and SetDeviceNickName in settings service.
  - Animal Crossing Island Transfer Tool is now bootable.

## 1.0.6930 - 2021-06-23
### Changed:
- Implements CreateUserInterface/CreateSystemInterface and stubs Initialize/IsNfcEnabled in nfc service.
- Implements CreateDebugInterface, CreateSystemInterface and GetRegisterInfo2 in nfp service.
  - Fixes a wrong size in RegisterInfo struct.

## 1.0.6929 - 2021-06-23
### Changed:
- Enables the single file publish feature at the project level to embed the managed dependencies on the executable, and remove some unused dependencies/files.

## 1.0.6928 - 2021-06-23
### Fixed:
- Fixes wrong TouchPoint Attribute in the hid shared memory and wrong main window focus value.
  - Fixes Fullscreen hotkey which needed minimize the window one time before.
  - Fixes Touch input in Mini Metro, Umineko no Naku Koro ni Saku and probably more games.

## 1.0.6927 - 2021-06-23
### Fixed:
- The component order of the instruction is the inverse of the vector returned by textureQueryLod.
  - Fixes blurry graphics on Mario + Rabbids Kingdom Battle, caused by the shader sampling from the wrong texture level (due to the incorrect LOD value).

## 1.0.6926 - 2021-06-23
### Changed:
- Implement VORN (register) Arm32 instruction.
  - MushihimeSama is now bootable.

## 1.0.6925 - 2021-06-23
### Fixed:
- Fixes an issue preventing some outputs from being passed to the pixel shader when geometry shader passthrough is enabled. 
  - Fixes missing graphics on Game Builder Garage, Nintendo Labo Toy-Con 04: VR Kit and possibly other games. (Note: Does not fix missing apple bug on Game Builder Garage.)

## 1.0.6924 - 2021-06-23
### Changed:
- Implement GetPlayerLedPattern, SetNpadJoyHoldType, GetNpadJoyHoldType, IsVibrationDeviceMounted (Needed by Super Mario Odyssey) in hid service.
- Implement GetLastActiveNpad, GetNpadSystemExtStyle, GetAppletFooterUiType and stub ApplyNpadSystemCommonPolicy in hid:sys service.

## 1.0.6923 - 2021-06-23
### Changed:
- Unbind input keys in the controller window by middle-clicking while an input is selected.

## 1.0.6922 - 2021-06-22
### Fixed
- Implement SaveSystemReport and SaveSystemReportWithUser in prepo services.

## 1.0.6921 - 2021-06-22
### Fixed
- Implement GetSharedFontInOrderOfPriorityForSystem in pl service.

## 1.0.6920 - 2021-06-22
### Fixed
- Fixes multiple inconsistencies in mii services.

## 1.0.6919 - 2021-06-23
### Changed:
- Kernel: Implement SVC MapTransferMemory and UnmapTransferMemory.

## 1.0.6918 - 2021-06-22
### Fixed
- Remove size checks for IPC buffer type 0x21/0x22.
  - Fixes a bsd service issue with homebrews and some games included Knockout City.

## 1.0.6917 - 2021-06-21
### Fixed
- Fix a bug that caused one of the account service functions to return an invalid user id if more than one user profile was configured on the emulator.
- Fixes a softlock on Shantae Half-Genie Hero title screen if more than one user profile was present. Might fix similar user profile related issues on other games.

## 1.0.6916 - 2021-06-21
### Changed
- Implement all known AppletMessage values. The new values are not used right now, but might be in the future.
- No expected changes.

## 1.0.6915 - 2021-06-20
### Fixed
- When the (separate) sampler bindless handle comes from the offset 0 of the constant buffer, it was being ignored which caused the wrong sampler to be used. This correct the bug by adding 1 to the offset on the shader translator, and then subtracting 1 on the GPU emulator to ensure that it is never 0 if a separate bindless sampler is used.
  - Fixes rendering issues in Final Fantasy XII The Zodiac Age.

## 1.0.6914 - 2021-06-19
### Changed
- Stub hid service call: IsFirmwareUpdateAvailableForSixAxisSensor needed by Game Builder Garage (Ignore missing services isn't required anymore).
- Stub irs service call: CheckFirmwareVersion needed by Nintendo Labo Toy-Con 03: Vehicle Kit.

## 1.0.6913 - 2021-06-16
### Changed
- Loading the default controller profile once again sets the controller type to Pro Controller. When Miria released, the function was changed to load Joycon Pair; this update reverts that change.

## 1.0.6912 - 2021-06-14
### Changed
- Shader decoding is now ended when reaching a block that starts with an infinite loop.
  - Fixes Dark Devotion hanging on boot at a plain white screen. The game now renders graphics, though they are unfortunately upside down.

## 1.0.6911 - 2021-06-14
### Changed
- Moved touchscreen updates to the input project. Code refactoring only; no changes in emulator behavior.

## 1.0.6910 - 2021-06-10
### Changed
- Increased the height of the settings window in the main GUI. Now all options are able to be viewed & modified without any scrolling.

## 1.0.6909 - 2021-06-09
### Fixed
- Fixed a sampler leak on exit by making TextureBindingsManager disposable and calling the dispose method in the TextureManager.

## 1.0.6908 - 2021-06-09
### Fixed
- The GPU subchannel state is no longer cleared on BindChannel.
  - Fixes a long-standing regression introduced by the addition of GPU syncpoints (PR 980) that was causing a crash on New Super Lucky's Tale.

## 1.0.6907 - 2021-06-09
### Changed
- Added support for bindless textures with separate constant buffers for texture and sampler.
  - Fixes Final Fantasy XII black screen on boot. The game is now playable though not 100% perfect.

## 1.0.6906 - 2021-06-02
### Changed
- Updated README.MD with latest motion/audio/SDL2 information as well as current game compatibility statistics.

## 1.0.6905 - 2021-06-02
### Fixed
- Fixed a shader bug with mixed PBK and SSY addresses on the stack.
  - Fixes pink sky on Dead or Alive Xtreme 3.

## 1.0.6904 - 2021-06-02
### Fixed
- Resolved a bug involving texture blit off-by-one errors that caused the destination textures to have a black border.
  - Fixes overly bright lighting on Fast RMX.
  - Fixes incorrect Mii character face rendering on Mario Kart 8 Deluxe.
  - Fixes a blue or transparent bar appearing at the top of the screen in Kirby Star Allies with resolution scaling enabled.

## 1.0.6903 - 2021-06-02
### Changed
Oops! Looks like quite a few of you are still using Dinput controllers. 
- Reverted the specific change that fixed the rare emulator hang on launch issue for now while we look into alternative fixes.
  - Fixes Dinput controllers not being recognized by the emulator.

## 1.0.6902 - 2021-06-02
### Changed
- Updated SDL2-CS.
  - Fixes motion controls not working in Linux.
  - Fixes a rare situation where the emulator could hang on launch for an undetermined period of time.

## 1.0.6901 - 2021-06-02
### Changed
- Now uses quads on OpenGL hosts when supported.
  - Improves OpenGL performance in Xenoblade Chronicles DE/2, Fast RMX, and potentially other games.

## 1.0.6900 - 2021-06-01
### Changed
- Changed the clear alpha channel to be applied on the bound framebuffer instead of clearing the alpha channel by its handle.
  - Fixes the infamous "black screen" bug occurring on AMD GPUs since driver 21.4.1.

## 1.0.6899 - 2021-05-31
### Fixed
- Fixed a bug in the shader translator where it would try to normalize image load/store colors.
  - Resolves a crash while loading a race in MotoGP 21. This title now reaches full gameplay but still suffers from graphical corruption.

## 1.0.6898 - 2021-05-30
### Fixed
- Fixed a logging regression where an incorrect function name would be included in the trace if the function had its name stripped.

## 1.0.6897 - 2021-05-30
### Fixed
- Fixed an inverted low/high mask value on GetThreadCoreMask32 syscall.
  - Allows Game Tengoku CruisinMix Special to boot further.

## 1.0.6896 - 2021-05-29
### Changed
- Implemented a multi-level function table.
  - Greatly improves performance in ACA NEOGEO, SEGA AGES, Arcade Archives, and some NES/SNES Online games. FPS increases of up to 65% were observed during testing. Note that this will really only impact those whose systems were unable to reach the game's intended FPS before, as disabling vsync in these games will unfavorably alter the game speed if FPS rises past what was intended by the game developer.
  - Improves overall JIT performance to a small degree, with minor reductions in boot time and increases in game frame rates (~5% on average where improvements are observed).
  - May improve performance in games that previously had inexplicably low frame rates.

## 1.0.6895 - 2021-05-25
### Changed
- Implemented all changes made in audren REV9 on 12.0.0.

## 1.0.6894 - 2021-05-24
### Changed
- POWER Update: implemented a new host mapped memory manager for major performance improvements across the board. Fastest option is now set by default.
  - Increases FPS by significant margins in most games/scenarios (except where emulated GPU-limited).
  - Reduces PPTC compilation times by roughly half.
  - Reduces boot time of most games by 30-50%.
  - Reduces size of JIT cache.
    - Resolves "JIT Cache Exhausted" crashes.
  - Resolves "AcquireSemaphore" crashes on all Ryzen CPUs known to exhibit the issue.
  - Resolves slow character movement speed in the monastery in Fire Emblem: Three Houses.
  - Noticeably improves/shortens "spool-up" period when first playing a game (note: does not negate the need to compile shaders).

## 1.0.6893 - 2021-05-24
### Changed
- Improved accuracy of reciprocal step ARM CPU instructions.
  - Fixes a bug where character clothing would be incorrectly warped in Rune Factory 5.

## 1.0.6892 - 2021-05-24
### Fixed
- Fixed incorrect default value for constant vertex attributes.
  - Improves rendering in some SD GUNDAM G GENERATION CROSS RAYS games.
- Fixed dumping of shaders containing bindless texture references.
  - Fixes a regression introduced in 1.0.6802 (PR 2145). This is a developer feature, no expected changes to emulator behavior.

## 1.0.6891 - 2021-05-24
### Changed
- Improved texture selection by comparing the aligned size of the largest mip level when considering sampler resize.
  - Fixes graphical errors and crashes in various Unity games that use render-to-texture.
  - Fixes an internal CLR error in Rune Factory 5.
  - Potentially improves infrequent graphical errors and crashes in Mario Kart 8 Deluxe.

## 1.0.6890 - 2021-05-21
### Changed
- Changed to a new approach for out of bounds blit.
  - Fixes invalid memory region crashes on Rune Factory 5 and Shantae.

## 1.0.6889 - 2021-05-21
### Added
- Implemented another Depth32F texture variant.
  - Along with the previous merge and the one following, greatly improves rendering on Yo-kai Watch 1.

## 1.0.6888 - 2021-05-21
### Fixed
- Fixed non-independent blend state entries being missing from the state table.
  - Fixes broken blend on some OpenGL games such as Code of Princess EX.

## 1.0.6887 - 2021-05-20
### Changed
- Extended information printed when the guest crashes or breaks execution. This may be useful for troubleshooting purposes.

## 1.0.6886 - 2021-05-20
### Added
- Implemented a new keyboard backend to the Ryujinx.Input.SDL2 project. This is currently unused.

## 1.0.6885 - 2021-05-20
### Fixed
- Fixed a regression that had broken resolution scaling in the Hyrule Warriors/Fire Emblem Warriors series of games.

## 1.0.6884 - 2021-05-20
### Fixed
- Fixed a regression introduced in 1.0.6878 (PR 2290) where buffer and texture information from one vertex shader was not propagated to the other after they were merged.

## 1.0.6883 - 2021-05-20
### Fixed
- Fixed a regression introduced in 1.0.6878 (PR 2290) where the constant buffer array size would be incorrect on games that use non-constant constant buffer slots.

## 1.0.6882 - 2021-05-20
### Added
- Adds a dynamic query in Linux to locate the installation of the ffmpeg root path.
  - Fixes nvdec-related crashes on Linux distros that do not use /libs as the ffmpeg root path.
  - Advises the user to install ffmpeg if it is not found.

## 1.0.6881 - 2021-05-20
### Changed
- Assigned _backgroundContext before starting its working thread.
  - Fixes a random chance for an embedded game to crash on launch.

## 1.0.6880 - 2021-05-20
### Changed
- Minor optimization to CPU emulation code.

## 1.0.6879 - 2021-05-20
### Added
- Added VIC/ORR Vd.T, #imm fast paths.

## 1.0.6878 - 2021-05-19
### Changed
- Refactored shader resource description creation and moved it out of the GLSL backend to the translator. No expected changes to emulator behavior.

## 1.0.6877 - 2021-05-19
### Changed
- Shader recompilation/rebuilding triggered at boot-time (such as in the case of a driver upgrade of shader code change) is now multi-threaded.
  - Reduces boot-time shader recompilation time by up to 75%.

## 1.0.6876 - 2021-05-19
### Fixed
- Fixed an issue with bindless textures when CbufSlot is not equal to the current TextureBufferIndex.
  - Fixes random chance of screen color flickering in Super Mario Party.

## 1.0.6875 - 2021-05-16
### Changed
- LocalVariable is now allowed to be assigned more than once.
  - Allows flow controls such as loops and if-elses with LocalVariables participating in phi nodes.

## 1.0.6874 - 2021-05-16
### Changed
- Moved the Windows Intel/AMD view format workaround out of the backend and replaced it with copy dependencies.
  - Significantly improves FPS on Intel iGPUs and AMD GPUs in Windows. Please test share your results!

## 1.0.6873 - 2021-05-16
### Changed
- Decoupled the Ryujinx configuration instance from all Ryujinx subprojects and moved directly to the Ryujinx project. No expected changes in emulator behavior.

## 1.0.6872 - 2021-05-14
No changes.

## 1.0.6871 - 2021-05-13
### Changed
- Miscellaneous x86 code generation optimizations.
  - Has potential to increase performance, but there are no known examples of improved games currently.

## 1.0.6870 - 2021-05-13
### Changed
- Implemented support for PPTC to stay enabled during runtime even if exeFS mods are activated.

## 1.0.6869 - 2021-05-11
### Fixed
- Fixed a race condition in SM initialization that could occur in rare situations when booting a game, causing it to crash.

## 1.0.6868 - 2021-05-11
### Fixed
- Fixed a specific core migration bug on the scheduler that could occur during context switching.
  - There are no currently known triggers for this bug; as such, there are no expected changes to emulator behavior.

## 1.0.6867 - 2021-05-08
### Fixed
- Fixed a minor bug in GTK3 keyboard mapping.

## 1.0.6866 - 2021-05-07
### Fixed
- Fixed a regression introduced in 1.0.6860 (PR 2260) that caused embedded games (in game collections such as Psikyo Shooting Stars Bravo) to display a single color in the entire window instead of the intended graphics.

## 1.0.6865 - 2021-05-07
### Fixed
- Fixed the default value for GraphicsConfig.MaxAnisotropy. No expected changes in emulator operation.

## 1.0.6864 - 2021-05-05
### Changed
- Corrected a couple of issues regarding SM instances and TIPC.
  - SM was previously instanced once and reused on all sessions. This could cause inconsistency on the service initialization. 
  - TIPC replies now match what is generated on original hardware.

## 1.0.6863 - 2021-05-05
### Added
- Implemented SDL2 audio backend.
  - Resolves most audout-related audio quality issues that occur with either OpenAL or SoundIO.
  - Considered safe to be used as the primary audio backend, but is not yet set as default; setting SDL2 as the default audio backend will happen in the future.
Please test this new audio backend and report your results! Options > Settings > System tab > Audio Backend dropdown.

## 1.0.6862 - 2021-05-05
### Changed
- Redirected ffmpeg-related log output to mitigate unnecessary console/log activity.

## 1.0.6861 - 2021-05-05
### Changed
- Cleaned up the nsd service and implemented/stubbed some associated service calls.
  - GetSettingName is removed because of a bad previous implementation (doesn't seem to be used by any games).
  - SetChangeEnvironmentIdentifierDisabled, WriteSaveDataToFsForTest, DeleteSaveDataOfFsForTest, and IsChangeEnvironmentIdentifierDisabled are stubbed.
  - GetApplicationServerEnvironmentType is implemented.

## 1.0.6860 - 2021-05-03
### Added
- Implemented a base for a possible future Vulkan integration with the current GTK3 UI.

## 1.0.6859 - 2021-05-03
### Fixed
- Fixed an issue that caused Linux to be unable to use FFmpeg since 1.0.6857.

## 1.0.6858 - 2021-05-02
### Changed
- Cleaned Discord Precense
  - Removed hardcoded title list and special icons for them.
  - Added a buttton to https://ryujinx.org/
  - Updated to latest version.

## 1.0.6857 - 2021-05-02
### Changed
- Updated to FFmpeg 4.4.0 

## 1.0.6856 - 2021-05-02
### Changed
- Rewrited HID shared memory management
  - This have no impacts on inputs.

## 1.0.6855 - 2021-05-01
### Fixed
- Fixed a bug that caused buffer memory modified shaders to not be flushed in certain cases.
  - Fixes a bug in New Pokémon Snap causing photos to not be identified.
  - Fixes a regression in Monster Hunter Rise where insects were invisible.

## 1.0.6854 - 2021-04-30
### Changed
- Changed logger to include certain values that were previously only written in the console.

## 1.0.6853 - 2021-04-30
### Changed
- Increased the Amiibo internal scan delay from 50ms to 125ms.
  - Fixes Amiibo scanning in Hyrule Warriors: Age of Calamity and Fire Emblem: Three Houses.

## 1.0.6852 - 2021-04-26
### Fixed
- Fixed a bug in GetClockSnapshot that was causing the steady clock timepoint to fail to be written.
  - Resolves a soft-lock occurring in early splash screens on Shantae. The game is now playable.

## 1.0.6851 - 2021-04-26
### Fixed
- Fixed unsolicited input (buttons pressing themselves/analog directions without user input) in all games known to exhibit the issue, including Mega Man 11, Crash Bandicoot 4, Black Legend, Balan Wonderworld, ADVERSE, Effie, and many other UE4 games.

## 1.0.6850 - 2021-04-24
### Fixed
- Fixed a typo in NpadIdType (changed uint to int).
  - Resolved a regression causing a crash on launch in Bayonetta 2 and possibly other games with similar HID calls.

## 1.0.6849 - 2021-04-24
### Fixed
- Fixed IsRegionInUse check on NV memory allocator. This was a non-fatal error calling the AllocSpace ioctl on a few games.

## 1.0.6848 - 2021-04-24
### Fixed
- Cleaned up various incorrect types in code from 2018 across the Ryujinx.HLE project.

## 1.0.6847 - 2021-04-23
### Added
- Implemented custom user profile emulation.
  - Simply click on Options > Manage User Profiles.
  - Profiles cannot be edited/managed while a game is running.
  - The "RyuPlayer" profile is the default user which cannot be deleted, as it contains all of the original save files for your games. However, the name and picture can be changed.
  - WARNING: Save data is specific to each user profile so if you delete a profile, the save data under that profile is also deleted!!
 
(For a more in-depth explanation of how the feature works, see the original PR description here: https://github.com/ryujinx-mirror/ryujinx/pull/2227)

## 1.0.6846 - 2021-04-22
### Changed
- Cleaned up the mm:u services, replacing previously unknown values with those that are now known.

## 1.0.6845 - 2021-04-20
### Changed
- Keyboard input is no longer queried if a controller is not set/selected.
  - Fixes a possible crash if raw keyboard input a.k.a. Direct Keyboard Access is enabled.

## 1.0.6844 - 2021-04-20
### Changed
- Changed clip distances to only be enabled if they are actually written to on the vertex shader.
  - Fixes flashing triangles, seemingly occurring only on Intel iGPUs, on games that use custom clip distances (most first party games).
See before & after screenshots below of Animal Crossing: New Horizons on an Intel iGPU:
Before: https://i.imgur.com/KegGaAw.png
After: https://i.imgur.com/mwZP4w5.png
Try out your favorite first party games on Intel iGPUs and please report to us the new results!

## 1.0.6843 - 2021-04-18
### Changed
- Miscellaneous CPU emulation optimization.

## 1.0.6842 - 2021-04-18
### Fixed
- Added a missing parentheses around low pass z computation, affecting low pass base gain on delay effect in mono.
  - Fixes incorrect audio volume changes when going indoors in FEZ.

## 1.0.6841 - 2021-04-18
### Changed
- Improved shader global memory to storage optimization pass.
  - Reduced code size.
    - May improve compile times as a result.
  - Improved code execution speed (probably only matters for integrated/mobile GPUs).
  - Reduced the number of bindings used (mitigates out of bindings compilation error on drivers with a low limit, like Intel iGPUs that only support 16 max).
  - Better buffer management (no need to sync/write track unused buffers).

## 1.0.6840 - 2021-04-17
### Changed
- Divdided up Intel HwCapabilities identification into IntelWindows and IntelUnix as the respective drivers present significant differences, and certain specific processing or workarounds should only be applied to one but not the other.

## 1.0.6839 - 2021-04-17
### Changed
- Modified audren to handle out of bounds read on empty delay lines.
  - Fixes a crash on launch occurring on FEZ. The game now boots properly.

## 1.0.6838 - 2021-04-17
### Fixed
- Fixed a bug in the Intel view copy workaround to take the texture target from the storage rather than the view, when using the storage handle for the copy.

## 1.0.6837 - 2021-04-17
### Fixed
- Fixed a controller applet related softlock occurring in Mario Kart 8 Deluxe while navigating multiplayer menus.

## 1.0.6836 - 2021-04-15
### Changed
- Subtracted haptic and sensor initialization from the SDL2 implementation as they are not needed.
  - Fixes an issue that could occur in certain Windows 10 environments with group policy set in a way that prevented the emulator from launching.

## 1.0.6835 - 2021-04-14
### Added
- Stubbed IApplicationFunctions ExtendSaveData
  - Fixes a missing service crash in Minecraft (cartridge/retail version).

## 1.0.6834 - 2021-04-14
### Changed
- Fixed a regression introduced on 1.0.6826 (PR 2186) regarding the IManage Resolve/ResolveEx calls.
  - Fixes crashes in The Savior's Gang, 1971 Project Helios, Chess Ultra, and NBA 2K Playgrounds 2.
- Stubbed ISslContext GetConnectionCount.
  - Resolves missing service crashes in Monopoly, Legendary Fishing, and Quiplash 2.

## 1.0.6833 - 2021-04-14
### Changed
- Miria has arrived! As a preparation for the upcoming new UI and for Vulkan, this update removes OpenTK 3 dependency entirely.
- Switched to OpenTK 4 for OpenAL implementation and mark OpenAL implementation as obsolete.
- Switched to OpenTK 4 for the Ryujinx.Graphics.OpenGL project.
- The emulator now uses SPB for context creation and manipulations instead of OpenTK 3.
  - Currently only WGL and GLX backend are supported. (no regressions compared to old implementation)
- Switched to SDL2 for controller input.
  - Controllers can now be hot-plugged without any constraints. No need to click any "refresh" buttons.
  - Motion controls now work on supported controllers without the need of external tools. (Legacy Cemuhook integration is still present if desired.)
  - DS4, DS5, Pro Controller and Single Joycons are natively supported. (Joycon pairs are not yet natively supported and outside the scope of this PR; Cemuhook integration can be used for joycon pairs in the meantime.)
  - Rumble API is exposed **while still not in use for now** (outside the scope of this PR).
- Switched to GTK3 for keyboard input.
- Individual keyboard support was removed (meaning there is no list of keyboards to choose from anymore; only "All Keyboards" is shown as an option).

## 1.0.6832 - 2021-04-13
### Changed
- Applied lossless image compression to various image resources within the GUI, saving a whopping 69KB.

## 1.0.6831 - 2021-04-13
### Added
- Added initial support for 12.x IPC system.
  - Also adds support for the new SM commands ids available on TIPC.

## 1.0.6830 - 2021-04-13
### Changed
- Added several new items to logging, for troubleshooting and log analysis purposes.
- Now logs hotkey and UI menu item changes while a game is running.

## 1.0.6829 - 2021-04-12
### Changed
- Miscellaneous PPTC optimizations to handle games with large exeFS.
  - PPTC can now handle any size of JitCache;
  - Allows to avoid memory peaks due to the resizing of the internal MemoryStream array (and its possible waste), its allocation in the LOH region of the heap; and allows to reduce the size of the .cache file if many translation invalidations are involved (LowCq -> HighCq over several runs).
  - Added an outer header structure and added the hashes for both this new structure and the existing "inner" header structure;
  - Hashes are now handled on a per-section basis and no longer for the entire uncompressed stream.
  - In games with a large exeFS, this "resolves System.IO.IOException: Stream was too long." and "Insufficient memory to continue the execution of the program." error messages. Does *not* resolve "JIT Cache exhausted" error. This will be resolved in a future PR.

## 1.0.6828 - 2021-04-12
### Added
- Added an AccountManager class (based on AccountUtils class).
  - Adds a "default profile values" which were the old hardcoded ones.
  - The image profile is moved to the Account service folder.
  - The hardcoded UserId for the savedata is now using the AccountManager last opened one.
  - The DeviceId in Mii service is changed to the right value (checked by REd sys:set call).
  - Miscellaneous cleanup.
  - Lays some groundwork to be able to move forward with implementing Custom User Profiles.

## 1.0.6827 - 2021-04-12
### Fixed
- Fixed sub-image copies on Intel iGPUs in OpenGL.
  - Resolves some glaring graphical bugs in games, on recent Intel iGPUs (those that support Open GL4.5 or higher), including Mario Kart 8 Deluxe and Captain Toad: Treasure Tracker.
  - These changes only apply to Intel iGPUs.

## 1.0.6826 - 2021-04-12
### Added
- Implemented GetCurrentNetworkProfile and stubbed several ssl services.
  - nifm IGeneralService GetCurrentNetworkProfile is implemented (fixes #643) and now blocked games boot further.
  - nifm GetLocalInterface() helper and DnsSetting struct have been fixed to return the right values.
  - ssl ISslService have been cleaned up.
  - ssl ISslContext have been cleaned up, CreateConnection and ImportClientPki have been stubbed (fixes #1289).
  - ssl ISslConnection is partially stubbed, which make some games playable, see the attached screenshots.
  - nsd IManager Resolve and ResolveEx have been fixed using the right buffer type.
  - Fixes missing service crashes for over 20 tested games. These can now proceed further in the boot process, and many are now playable.

## 1.0.6825 - 2021-04-12
### Fixed
- Fixed a bug in SurfaceFlinger when closing a layer, and refactored selection of the current layer being rendered.
  - Star Wars: Republic Command and Shadow Gangs now proceed further in the boot process.

## 1.0.6824 - 2021-04-08
### Changed
- Auto-updating has been once again enabled in portable mode configurations.
  - NOTE: if you are currently using portable mode, please download this update manually from the link above or from https://ryujinx.org/download

## 1.0.6823 - 2021-04-07
### Fixed
- Fixed PermissionLevel names within the FriendService.

## 1.0.6822 - 2021-04-07
### Fixed
- Fixed a bug with the CRC32 intrinsic where the JIT was not forcing a copy when a constant value is used as an input.
  - Resolves an issue where the emulator could crash while rebuilding PPTC during a boot of Monster Hunter Rise.

## 1.0.6821 - 2021-04-07
### Added
- Added an option to manually trigger Github actions.

## 1.0.6820 - 2021-04-07
### Changed
- Added a temporary fix for Windows nuget issues/fix git hashes for PRs.

## 1.0.6819 - 2021-04-04
### Added
- Exposes an option on the UI to increase the amount of the emulated Switch memory from 4GB to 6GB.
  - Since there are no known retail Switch consoles with 6GB of RAM, this option has been placed under the Hacks section next to the "Ignore Missing Services" option.

## 1.0.6818 - 2021-04-03
### Changed
- Debug builds posted by github-actions on pull requests are now hidden.

## 1.0.6817 - 2021-04-02
### Changed
- No longer force flush commands on sync creation for NVIDIA GPUs as they don't seem to need it.
  - Fixes a minor performance regression in Monster Hunter Rise and possibly other games.

## 1.0.6816 - 2021-04-02
### Changed
- Miscellaneous code optimizations to improve StoreToContext emission.
  - Reduces register pressure, code size and compile time a little bit.

## 1.0.6815 - 2021-04-02
### Changed
- Updated README.MD to reflect new OpenGL requirements, game compatibility statistics, and mods/cheats support.

## 1.0.6814 - 2021-04-02
### Changed
- Miscellaneous code optimizations to reduce allocation during SSA construction.

## 1.0.6813 - 2021-04-02
### Changed
- Changed the Pro Controller image used in the controller setup UI for improved button clarity.

## 1.0.6812 - 2021-04-02
### Changed
- Changed behavior of render targets in use to prevent them from falling out of the auto delete cache. This bug was discovered while developing Vulkan (would cause a crash if a freed texture was used).
- Cleaned up some ordering of related code for better readability.

## 1.0.6811 - 2021-04-02
### Fixed
- Updated the Tamper Machine module with the following fixes:
  - Fixed a crash that could occur while applying runtime mods/cheats to an unpacked game.
  - Fixed an inconsistency in sleep timings and adjusted the duration to more closely match Atmosphere's.

## 1.0.6810 - 2021-04-02
### Added
- Implemented shader HelperThreadNV/gl_HelperInvocation.
  - Fixes flickering textures on Monster Hunter Rise.
  - OpenGL requirements for the emulator have been raised to OpenGL 4.5 minimum (previously 4.4).

## 1.0.6809 - 2021-03-29
### Changed
- Command flushes are now forced after creating a syncpoint.
  - Alleviates "sync object timeout" errors on AMD and Intel GPUs.

## 1.0.6808 - 2021-03-29
### Changed
- Modified the DNS blacklist to be case-insensitive.

## 1.0.6807 - 2021-03-29
### Changed
- Optimized PrintRoSectionInfo code.
  - Reduces game load time by a small amount. May not be noticeable in many cases.

## 1.0.6806 - 2021-03-27
### Added
- Stub ILibraryAppletAccessor RequestExit in AM service. 
  - Fix a softlock in Monster Hunter Rise when you press "Private Policy" at the beginning.
  - Fix a softlock in Monster Hunter Generations Ultimate after a multiplayer session.

## 1.0.6805 - 2021-03-27
### Fixed
- Fix ZN flags set for shader instructions using RZ.CC destination.
- Fixed a bug on pixel shaders with calls where it would set the output pixel values before every return.

## 1.0.6804 - 2021-03-27
### Changed
- Added credit to AmiiboAPI in the emulator's About window as well as in the README.MD file.
- Fixed an incorrect warning code in an am service. 

## 1.0.6803 - 2021-03-27
### Added
- Implemented the TamperMachine module, which fully supports runtime mods & all Atmosphere style cheats (except game pause/resume).
  - The standalone Cheat Engine software is no longer needed.
  - Cheats use the same folder as mods.
  - See https://github.com/ryujinx-mirror/ryujinx/pull/1928 for usage details and links to cheat repos.

## 1.0.6802 - 2021-03-26
### Changed
- Moved the bindless check stopgap from translation to decode phase, and disables cache-related processing early.
  - Fixes Monster Hunter Rise 1.1.1's issue with Ryujinx's shader cache. You may now enable it while using this version of the game.

## 1.0.6801 - 2021-03-25
### Added
- Implemented SaveScreenShot calls:
  - caps:u IAlbumApplicationService (32) SetShimLibraryVersion
  - caps:c IAlbumControlService (33) SetShimLibraryVersion
  - caps:su IScreenShotApplicationService (32) SetShimLibraryVersion
  - caps:su IScreenShotApplicationService (203/205/210) SaveScreenShotEx0/SaveScreenShotEx1/SaveScreenShotEx2
- Enabled in-game screenshot functionality in Animal Crossing: New Horizons, and Monster Hunter Rise, using ImageSharp to save the raw screenshot data to a JPG file.
  - There is currently a bug in Monster Hunter Rise that requires spamming the 'A' button in order for the screenshot function to work the first time. This is being worked on and will hopefully be fixed soon!
- To retrieve screenshots:
    1. Click on File > Open Ryujinx Folder.
    2. Drill down to the sdcard -> Nintendo -> Album folder.
    3. Screenshots will be saved in dated subfolders e.g. %AppData%\Ryujinx\sdcard\Nintendo\Album\2021\03\26\2021032601020300-0123456789ABCDEF0123456789ABCDEF.jpg

## 1.0.6800 - 2021-03-25
### Added
- Implemented CPU instructions: Sqdmulh_Ve & Sqrdmulh_Ve Inst.s with tests.
  - Fixes missing opcode crash on Monster Hunter Rise.

## 1.0.6799 - 2021-03-25
### Added
- Implemented SetRequestExitToLibraryAppletAtExecuteNextProgramEnabled and added a placeholder for the ectx services.
  - Fixes a missing service crash on CARRION, The Witch and The 66 Mushrooms, Pixel Game Maker Series Werewolf Princess Kaguya, Monster Hunter Rise v1.1.1, and any other/future game created with Nintendo SDK 11.x.

## 1.0.6798 - 2021-03-24
### Changed
- Cleaned up sfdnsres service implementation and added several related services:
  - Implemented GetAddrInfoRequest. Partially implemented GetHostByNameRequestWithOptions, GetHostByAddrRequestWithOptions and GetAddrInfoRequestWithOptions.
    - Fixes missing service crashes on over 100 tested games. Many games that could not boot before are now playable; some cannot go in-game if designed to rely on a connection to the internet (which is blocked).
  - Added a DNS blacklist to prevent games from reaching Nintendo online services.
- Reduces code differences between master and the LDN build.

## 1.0.6797 - 2021-03-22
### Changed
- Optimized the operation of & code for the GUI progress reporting (PPTC & Shader Cache loading bars).
  - Fixes an issue that could cause a string of GTK-related code warnings to pop-up in the console upon opening the emulator.

## 1.0.6795/1.0.6796 - 2021-03-20
### Changed
- Adjusted some CI build configurations.

## 1.0.6794 - 2021-03-19
### Changed
- The shader cache now detects and avoids caching shaders that use bindless textures.
  - Prevents corruption of the shader cache that could cause a crash on launch in games that have bindless textures (mostly Unreal Engine 4 games).
  - This is a temporary stopgap solution to resolve shader cache issues until proper bindless support is implemented.

## 1.0.6793 - 2021-03-18
### Fixed
- Improved linear texture compatibility rules.
  - Resolves an issue where small or width-aligned (rather than byte aligned) textures would fail to create a view of existing data. Creates a copy dependency, as size change may be risky. View layout compatibility is now determined by the stride shifted by the level, rather than a stride caculated from the level's width. Linear textures are considered copy compatible when they have matching stride.
    - Fixes missing lens flare in Mario Kart 8 Deluxe and Splatoon 2.
    - Fixes an issue causing Cross Code to boot to a black screen.

## 1.0.6792 - 2021-03-18
### Changed
- Removed the IIpcService.cs interface references as it is no longer needed or used.

## 1.0.6791 - 2021-03-18
### Added
- Added more items to standard logs for troubleshooting purposes.
  - Now logs Docked/Handheld state/toggling, selected audio backend, and Vsync status/toggling.

## 1.0.6790 - 2021-03-18
### Added
- Implemented ApplicationErrorArg to the Error Applet.
  - This was already implemented in the LDN build and as such has already undergone sufficient testing.
  - Reduces code differences between the LDN build and master.

## 1.0.6789 - 2021-03-18
### Changed
- Reworded some pop-up dialog boxes for better clarity of the messages to the end user. No emulator functionality changes.

## 1.0.6788 - 2021-03-18
### Added
- Added Amiibo scan emulation of all Amiibos.
  - Uses a self-hosted fork of the AmiiboAPI RESTful API. No need to scan any bin files!
  - Amiibo can be trained in Super Smash Bros.
To scan an Amiibo:
1. Go to the Amiibo-specific scanning location or in-game menu option in a supported game.
2. Click on the Ryujinx Actions menu header > Scan an Amiibo. That's it!

## 1.0.6787 - 2021-03-16
### Changed
- Improved the grammar in a code comment. No emulator functionality changes.

## 1.0.6786 - 2021-03-15
### Changed
- Improved portable mode operation:
  - Implemented an auto-enabling of portable mode if the user places a subfolder named "portable" beneath the Ryujinx program folder. If detected, Ryujinx will automatically use it as the default location for all system files (keys/PPTC/shaders etc.).
    - Using this method, the Ryujinx program folder is now fully able to be moved around at will without needing to use any special shortcuts or command line parameters, and will still retain portable functionality. Thanks to a previous enhancement to the auto-update function, the "portable" subfolder will be preserved during auto-update processes.

## 1.0.6785 - 2021-03-14
### Fixed
- Fixed a typo in a debug assert message within the OpenAL implementation details.

## 1.0.6784 - 2021-03-14
### Changed
- The active audio device is now reported as a TV, rather than internal speakers.
  - Fixes noticeably poor audio quality in Pokémon Sword and Shield, and potentially other games.
  - May also enable 5.1 audio output on games where it was not functioning properly before.

## 1.0.6783 - 2021-03-13
### Changed
- Implemented an override to the openal-soft audio output mode to fix a potential loss in audio quality while using the OpenAL backend if Windows audio output was in headphone mode.

## 1.0.6782 - 2021-03-13
### Fixed
- Fixed a Github actions warning. No emulator code changes.

## 1.0.6781 - 2021-03-10
### Fixed
- ILibraryAppletAccessor handles are now closed on disposal.
  - Fixes an issue where if a game was calling the controller applet multiple times it could result in an "out of handles" crash.

## 1.0.6780 - 2021-03-09
### Fixed
- Fixed lineSize for LinearStrided to Linear conversion.
  - Fixes a possible crash when width is greater than stride, which can happen due to alignment when copying textures with the copy buffer method. The only currently known game where this had occurred is Bravely Default II.

## 1.0.6779 - 2021-03-09
### Changed
- Emulator now allows bindless handles to be found for image/texture instructions with predicates, when the assignment of the texture handle is within the same predicate.
  - Fixes character shadows and black, soulless eyes on Bravely Default II.
  - Fixes broken rendering in the Billiards and Shooting Gallery mini-games in 51 Worldwide Games.
  - Probably fixes other broken rendering in games with bindless textures.
  - Resolves conflicts between bindless textures and shader cache that previously required shader cache to be purged or disabled prior to each boot of certain affected games.
    - For affected games (such as Super Mario Party, 51 Worldwide Games, and many newer UE4 games) you may purge the shader cache once. Note: not all UE4 games are yet shader cache compatible.

## 1.0.6778 - 2021-03-08
### Fixed
- Reworked Buffer Textures to create their buffers in the TextureManager, then bind them with the BufferManager later.
  - Fixes an issue where a buffer texture's buffer could be invalidated after it is bound, but before use.
- Fixed width unpacking for large buffer textures. The width is now 32-bit rather than 16.
- Forced buffer textures to be rebound whenever any buffer is created, as using the handle id wasn't reliable, and the cost of binding isn't too high.
- The sum of these fixes vertex explosions and flickering animations in UE4 games. May affect games that use ImageStore, as now those textures are flushed on read/write.
  - Resolves all known lighting issues in Fire Emblem: Three Houses.

## 1.0.6777 - 2021-03-07
### Changed
- Modified the component mask to be flipped if the target is BGRA.
  - On its own, should not result in any visible changes. Requires better bindless texture handling in order for visual improvements to be realized (such as fixing the shadows in Braveley Default II battles).

## 1.0.6776 - 2021-03-06
### Changed
- Removed unused physical region memory tracking.
  - Accelerates memory unmap/remap of virtual addresses that were once used for GPU resources, as well as reduce the cost of creating new tracking handles.
    - Reduces load times and non-shader related stutter in games that perform many unmap/remap operations such as Unreal Engine 4 games, among others.
  - Tracking regions/handles are now notified about an unmap operation before they are actually unmapped.
Example of non-shader related stutter being improved by this update:
Before
https://cdn.discordapp.com/attachments/728011297857339422/817785701834817566/bd2_master.mp4

After
https://cdn.discordapp.com/attachments/728011297857339422/817785806307590144/bd2_pr.mp4


## 1.0.6775 - 2021-03-06
### Changed
- Improved handling for unmapped GPU resources.
  - Fixed a memory tracking bug that would set protection on empty PTEs (caused some buffer tracking crashes).
  - When a texture's memory is (partially) unmapped, all pool references are forcibly removed and the texture must be rediscovered to draw with it. This will also force the texture discovery to always compare the texture's range for a match.
  - RegionHandles now know if they are unmapped, and automatically unset their dirty flag when unmapped.
  - Partial texture sync now loads only the region of textures that has been modified. Unmapped memory tracking handles cause dirty flags for a texture group handle to be ignored.
  - Significantly improves emulator stability in newer UE4 games such as Bravely Default II.
  - Fixes texture swaps in Bravely Default II. Note: this game still suffers from some graphical issues such as random vertex explosions, missing facial animations, and missing shadows.
  - Improves emulator stability in Super Smash Bros. Ultimate.
  - Fixes a regression causing a crash on launch in Contra: Rogue Corps. The game is once again playable.

## 1.0.6774 - 2021-03-04
### Fixed
- Fixed SetStandardSteadyClockInternalOffset permission check.

## 1.0.6773 - 2021-03-02
### Changed
- Added loading bars at the bottom of the main window for pre-boot host shader cache building and PPTC translation operations to give users a visual indicator of boot progress. 
  - Removed associated console entries for both of these activities. Rebuilding shaders after an invalidating event will show both in the console and in the loading bar.

## 1.0.6772 - 2021-03-02
### Added
- Added fast paths in the audio renderer for AArch64 in all current fast paths.

## 1.0.6771 - 2021-03-02
### Changed
- Improved efficiency of "Hide Cursor on Idle" operation to minimize overhead.

## 1.0.6770 - 2021-03-02
### Changed
GPU:
- Fixed various issues with view compatibility, synchronization and creation.
- Greatly improved texture view creation, keeping it within a strict storage-view hierarchy. (no view-views)
  - Fixes a memory leak in Hyrule Warriors: Age of Calamity. Its VRAM usage is now rather low. :)
- Reworked texture memory tracking to use a structure called a "Texture Group".
  - There is one texture group for each storage texture, and views of that storage cover a subset of its "Texture Group Handles".
    - A handle can represent one or more sub-image, which can be a mip level, array layer, cubemap face or 3d texture slice.
    - This allows memory tracking to be shared between views, which is good as they also share internal data.
  - This fixes issues where cpu texture invalidation would be missed or repeat sync when using multiple views of the same texture.
  - Affected games:
    - Fixed swapping textures in Xenoblade Chronicles: Definitive Edition, Xenoblade 2, MH:GU, potentially other games.
- Introduces "Copy Dependencies" between handles of a texture group - indicating that their contents must be copied to another texture after modification.
  - Allows copies between two textures that are view compatible, but have different storages.
  - Replaces two old copy-on-create methods that were used for rendering to 3D and compressed textures.
    - Also removes the workaround for keeping the copied data in these textures, which was to disable memory tracking for them entirely.
    - This fixes colour ramps in a large number of first party games such as Super Mario Odyssey, where they would use the first rendered version of the 3D texture and look incorrect on future levels/rooms.
  - Affected games:
    - Fixes nearly all issues with cubemaps/lighting in Mario Kart 8 Deluxe.
    - Fixes issues in Splatoon 2 where lighting would be unusually dark on the first load of the plaza, or on random stages.
    - Fixes issues in Splatoon 2 where the time remaining or menu button text would be missing or incorrect.
    - Fixes a lot of issues with textures in A Hat In Time (Unreal Engine 3)
    - Fix issues with textures in Yoshi's Crafted World, and generally other Unreal Engine 4 games
      - Bravely Default 2 requires its own set of gpu memory management fixes, coming some point soon.
    - Fixes issues with DOF in Animal Crossing: New Horizons (as well as Copy Dependencies)
    - Pikmin 3's intro video is now visible. Any possible issues with the Koppad have been thwarted.
    - Probably a lot more.
  - Should greatly decrease chance of GPU memory becoming corrupted by a texture flush.
- Reworked size calculation for mipmapped 3D textures.
  - Fixes fog, waves, water caustics, mario dirt overlay and much more in Super Mario Odyssey (also needs copy dependencies)

For a screenshot collection, see https://github.com/ryujinx-mirror/ryujinx/pull/2001#issuecomment-782771370

## 1.0.6769 - 2021-03-01
### Fixed
- Fixed a regression in SignalMemoryTracking, introduced in 1.0.6763 (PR 2044), that could cause a black screen on the Monster Hunter Rise Demo.

## 1.0.6768 - 2021-02-28
### Changed
- Revised SystemInfo:
  - Extract CPU name from CPUID when supported.
  - Linux: Robust parsing of procfs files
  - Windows: Prefer native calls to WMI
  - Remove unnecessary virtual specifiers
- Reduces application startup time by roughly 1 second.

## 1.0.6767 - 2021-02-28
### Fixed
- Modified the SoundIO session implementation to be lock-free.
  - Fixes a rarely occurring random crash related to SoundIO introduced in 1.0.6732 (PR 2007).
  - Mildly improves performance while using SoundIO.

## 1.0.6764 - 2021-02-28
### Changed
- Switched ci to use Github artifacts for PRs. No emulator code changes.

## 1.0.6763 - 2021-02-28
### Fixed
- Fixed virtual address overflow near ulong limit.
  - Creates an overflow-safe way of counting pages for affected functions and replaces the address-based loop in IsRangeMapped with a page-based loop.

## 1.0.6762 - 2021-02-28
### Changed
- Updated wording in the auto-updater.

## 1.0.6761 - 2021-02-28
### Changed
- Improved heuristic for showing the software keyboard.
  - Fixes an inline keyboard soft-lock that occurred in God Eater 3 when changing the codename of your character.

## 1.0.6732 - 2021-02-25
### Changed
- Haydn: Part 1 (based on reverse engineering of audio 11.0.0)
  - Complete reimplementation of audout and audin.
    - Audin only has a dummy backend at this time.
  - Dramatically reduces overall CPU usage in both audio backends (50% CPU usage reduction on average with SoundIO backend - your mileage may vary). May improve FPS in situations where the CPU was previously maxed out.
  - Audio Renderer now initializes its backend on demand instead of keeping two up all the time.
  - All audio backend implementations are now in their own project.
  - Ryujinx.Audio.Renderer was renamed to Ryujinx.Audio and as such appropriately refactored.
  - Resolves a missing audin service crash in FUZE4.
  - Resolves an audout-related crash on launch in OniNaki.

## 1.0.6714 - 2021-02-24
### Fixed
- Modified the auto-updater to preserve user execute permissions in Unix/Linux environments.

## 1.0.6690 - 2021-02-23
### Changed
- Modified the auto-updater behavior to only purge Ryujinx files when installing a new update.
  - Allows "portable mode" relative paths beneath the Ryujinx executable or other subfolders to be safely used as data directories. Make sure to wait until after this update is installed before changing your portable mode paths!

## 1.0.6687 - 2021-02-23
### Fixed
- Fixed the unwanted propagation of a relocatable constant in a specific case.
  - Resolves a particular PPTC-related crash presenting a "failed to encode constant" error.

## 1.0.6682 - 2021-02-22
### Changed
- Updated README.MD to include the latest game compatibility list counts.

## 1.0.6680 - 2021-02-22
### Added
- The auto-updater now uses multiple download threads, noticeably reducing download time in most cases.

## 1.0.6674 - 2021-02-22
### Added
- Implemented VCNT instruction.
  - Fixes a missing opcode crash on Valkyria Chronicles, which now goes in-game.

## 1.0.6670 - 2021-02-21
### Changed
- PPTC & Pool Enhancements:
  - Fixed memory instability / excessive memory spikes in some "heavy" games after loading/saving .cache files (fixes issue reported by users);
  - Fixed limitation of loading/saving large .cache files (fixes issue reported by users);
  - Reduced memory usage when loading/saving .cache files;
  - Ptc.Load & Ptc.Save now use XXHash128, which is 10 times faster than MD5;
  - Fixed redundant saving / log spamming of .info files (fixes issue reported by users);
  - Added a simple PtcFormatter library for deserialization/serialization, which does not require reflection, in use at PtcJumpTable and PtcProfiler; improves maintainability and simplicity / readability of affected code.
  - Improved handling of Pools (Slim) for PPTC, allowing to halve the number of pools in use and thus reducing memory usage and slightly increasing the translation speed (you save ~1 second every 1k translations (for a CPU with 4+4 cores); so for 30k translations you save ~30 seconds (for the same CPU));
  - BitMap pools are now limited during use and disposed after use;
  - Pools Limiter is now configurable.

## 1.0.6667 - 2021-02-21
### Changed
- Converted Copy operations into Fill operations instead of adding one in HybridAllocator.
  - Reduces code size and register pressure.

## 1.0.6662 - 2021-02-21
### Added
- Implemented SetLcdBacklighOffEnabled service call.
  - Fixes a missing service crash that could occur in Super Smash Bros. Ultimate vault menus.

## 1.0.6659 - 2021-02-20
### Changed
- Miscellaneous input handling refactoring.
  - Resolves myriad input mapping issues including phantom button presses while mapping DirectInput devices.

## 1.0.6635 - 2021-02-19
### Changed
- Windows now recognizes Ryujinx as a DPI-aware application. Also fixes DPI scaling in other operating systems.
  - Fixes menus and controller configuration screens being too cramped or cutting off parts of the window.

## 1.0.6634 - 2021-02-19
### Changed
- Ryujinx now allows modding of AddOnContent (DLC) RomFS.

## 1.0.6631 - 2021-02-19
### Changed
- Modified the "Ignore Missing Services" option to take effect immediately, even during emulation.

## 1.0.6630 - 2021-02-19
### Fixed
- Fixed another issue introduced in the IPC refactoring changes involving returned buffer sizes. These are now provided explicitly in GetClientId calls.
  - Resolves a crash in Horizon Chase Turbo, Doom, and potentially other games with similar calls.

## 1.0.6604 - 2021-02-17
### Fixed
- Fixed an issue introduced in the IPC refactoring changes involving returned buffer sizes. These are now provided explicitly in GetFirmwareVersion calls.
  - Resolves some crashes occurring in homebrew.

## 1.0.6587 - 2021-02-16
### Fixed
- Fixed a performance regression introduced in 1.0.6582 (PR 1987) involving memory tracking. The read/write flags had been inadvertently inverted.

## 1.0.6582 - 2021-02-16
### Changed
- Addresses are now properly validated when the PTE is loaded from the page table.
  - Any invalid CPU memory addresses will now print an InvalidMemoryRegionException instead of just showing AccessViolationException on the console.
- Address validation has been moved inside the EmitPtPointerLoad function, rather than doing the check before calling it.

## 1.0.6575 - 2021-02-15
### Added
- Added an option to hide the mouse cursor on inactivity. No more pesky mouse cursor while you're playing!
  - This option can be enabled in Options > Settings > General tab > "Hide Cursor On Idle".

## 1.0.6551 - 2021-02-11
### Added
- Implemented GetSystemSessionId and added associated prepo permission levels.
  - Fixes a missing service crash on launch on Super Mario 3D World + Bowser's Fury.

## 1.0.6548 - 2021-02-11
### Changed
- Corrected joy-con image aspect ratio.

## 1.0.6546 - 2021-02-10
### Changed
- Enabled multithreaded decoding of VP9 NVDEC videos.
  - IImproves performance on several first party intro videos, providing noticeably smoother video & audio, included in the following titles: Super Smash Bros. Ultimate, Pokémon Let's Go Eevee/Pikachu, The Legend of Zelda: Link's Awakening, Fire Emblem: Three Houses, Tokyo Mirage Session #FE Encore, Animal Crossing: New Horizons, and Mr. Driller DrillLand.

## 1.0.6545 - 2021-02-10
### Changed
- Updated joy-con images in GUI for better visibility.

## 1.0.6544 - 2021-02-10
### Changed
- Improved inline keyboard compatibility.
  - Implements a new keyboard request used by Monster Hunter Rise Demo.
  - Fixes a softlock or crash on Monster Hunter Generations Ultimate in certain situations when using the keyboard.
  - Fixes a crash in Dark Souls Remastered if entering a player name for a second time that is shorter than the first.

## 1.0.6540 - 2021-02-10
### Changed
- Edited the global.json file to allow the use of dotnet sdk 5.0.xxx rather than locking it to 5.0.100.

## 1.0.6532 - 2021-02-09
### Changed
- The emulator now automatically loads a default configuration if an invalid configuration is detected.

## 1.0.6529 - 2021-02-09
### Added
- Implemented ISystemSettingsServer.IsUserSystemClockAutomaticCorrectionEnabled service call.
  - Enables functionality in some homebrew applications.

## 1.0.6521 - 2021-02-07
### Fixed
- Fixed non-contiguous IPC memory copies.
  - Fixes a specific crash on Bravely Default II Demo, Balan Wonderworld Demo, and possibly other games introduced in 1.0.5899 (PR #1458)

## 1.0.6520 - 2021-02-07
### Changed
- Optimised JIT code generation to reduce register utilisation.
  - This change may provide a minor improvement in code compilation time and quality.

## 1.0.6519 - 2021-02-07
### Changed
- Simplified code generation when using multiple vertex shader programs.
  - This is a code style change, and should not have any impact on emulator performance.

## 1.0.6518 - 2021-02-07
### Changed
- Fixed the updater application icon on Linux.

## 1.0.6517 - 2021-02-07
### Changed
- Disabled partial JIT invalidation on un-map.
  - Fixes a significant performance regression in some games introduced by 1.0.6096 (PR #1518).

## 1.0.6516 - 2021-02-07
### Added
- Added support for the ETC2 (RGB) texture format.
  - Fixes corrupt textures in Vegas Party.

## 1.0.6471 - 2021-02-01
### Changed
- Disabled flushing of multisample textures.
  - Fixes a specific crash on Super Bomberman R, fault - milestone one, Leisure Suit Larry, and possibly other untested games.

## 1.0.6469 - 2021-01-31
### Changed
- Miscellaneous refactoring of shader call code. No expected changes in emulator behavior.

## 1.0.6455 - 2021-01-29
### Added
- Added support for geometry shader passthrough.
  - Improves rendering Marvel Ultimate Alliance 3. Note: the game is still not playable due to other issues.

## 1.0.6453 - 2021-01-29
### Changed
- Updated the text label for the PPTC toggle in the emulator settings window to improve user experience.

## 1.0.6452 - 2021-01-29
### Added
- Added a texture/sampler descriptor cache for faster pool invalidation.
  - Potentially improves performance in games that have stutter caused by texture pool invalidation.
  - Fixed a regression in Mario Kart 8 Deluxe introduced in 1.0.6337 (PR 1905) that would evict a course's cube-map array and shadow map from the pool while playing.

## 1.0.6441 - 2021-01-27
### Added
- Added support for multiple destination operands on the IR rather than just one.
  - As shader changes occurred, shader caches have been gracefully invalidated; the next time a game is launched the cache will automatically rebuild at boot time.

## 1.0.6440 - 2021-01-27
### Fixed
- Lowered precision of estimate instruction results to match ARM behavior.
  - Fixes a logic bug in Catherine: Full Body preventing progression of gameplay.
  - Fixes a logic bug freezing most controller input in Slayaway Camp: Butcher's Cut, Friday the 13th: Killer Puzzle, and Out Of The Box.

## 1.0.6432 - 2021-01-26
### Added
- Support shader F32 to bool re-interpretation

## 1.0.6429 - 2021-01-26
### Fixed
- Fixed a regression introduced in 1.0.6420 (PR 1948) that would generate invalid code for atomic SSBO operations.
  - Fixes regression on Persona 5 Scramble where it could slow to a crawl and/or crash before or at the title screen.
  - Fixes regression on Monster Hunter Rise Demo where some objects were rendered completely black.

## 1.0.6426 - 2021-01-26
### Changed
- Made some simple changes on the OpenGL backend to significantly reduce the number of redundant calls.
  - Has potential to offer a minor improvement to performance depending on the game.

## 1.0.6425 - 2021-01-26
### Fixed
- Changed the implementation of conditional rendering to actually compare the values on memory even if they don't come from queries (instead of just returning false).
  - Improves rendering Marvel Ultimate Alliance 3. Note: the game is still not playable due to other issues.

## 1.0.6424 - 2021-01-26
### Changed
- Ryujinx will now re-check for keys before verifying a firmware install. The user will no longer have to close & reopen the application in order for keys to be recognized on initial setup/clean install.

## 1.0.6423 - 2021-01-26
### Fixed
- Fixed a regression that broke compute shader code dumping.
  - This is a developer-only function and does not affect emulator operation during gameplay.

## 1.0.6421 - 2021-01-26
### Added
- Add support for shader atomic min/max operations.
  - Fixes missing graphics on Disgaea 6 Demo.

## 1.0.6419 - 2021-01-26
### Added
- Implemented a prfm instruction variant.
  - Fixes a missing opcode crash in Edna & Harvey: Harvey's New Eyes and Deponia.

## 1.0.6418 - 2021-01-26
### Changed
- Increase the controller input window size to prevent the controller input window from scrolling.

## 1.0.6407 - 2021-01-24
### Changed
- Prevent the display from sleeping whilst a game is running on Windows.

## 1.0.6406 - 2021-01-24
### Added
- Added vector fast paths for VCLZ ARM instructions.

## 1.0.6405 - 2021-01-24
### Changed
- Modified storage buffer to allow out of bounds access by binding the entire buffer rather than just the range that the game says its using.
  - Fixes overly bright lighting in Yo-kai Watch 4/++ and other Yo-kai games.

## 1.0.6404 - 2021-01-24
### Changed
- Updated the controller images in the input mapping UI for better visibility across multiple themes.

## 1.0.6403 - 2021-01-24
### Changed
- Implemented a workaround for Github Actions windows-latest restore failures. No emulator code changes.

## 1.0.6392 - 2021-01-23
### Changed
- Changed the behavior of exact texture matches to now compare the physical regions if the virtual address is not the same, and allows a match if the virtual addresses are different yet mapped to the same physical region.
  - Fixes a regression on Rune Factory 4 Special and New Super Mario Brothers U Deluxe introduced in 1.0.6337 (PR 1905).

## 1.0.6391 - 2021-01-23
### Fixed
- Fixed TZName parsing in TZIF footer.
  - Fixes incorrect timezone offset seen in games such as Animal Crossing: New Horizons.

## 1.0.6389 - 2021-01-22
### Fixed
- Fixed an inverted read only flag in transfer memory creation.
  - Fixes an issue with the inline swkbd implementation.

## 1.0.6373 - 2021-01-20
### Fixed
- Fixed an issue preventing the SL + SR buttons from being mapped properly.

## 1.0.6369 - 2021-01-19
### Fixed
- Fixed a regression introduced in 1.0.6351 (PR 1929) that caused swkbd prompts to softlock or crash the respective game when called.

## 1.0.6367 - 2021-01-19
### Added
- Implemented Fmaxnmp & Fminnmp Scalar instructions.
  - Fixes a missing opcode crash in Mortal Kombat 11. The game is still not playable due to rendering issues.

## 1.0.6355 - 2021-01-18
### Changed
- Enabled parallel ASTC decoding by default.
  - Mitigates some ASTC texture load related stutter on games with ASTC textures.

## 1.0.6354 - 2021-01-18
### Fixed
- Fixed a regression introduced in 1.0.6327 (PR 1911) that caused Kirby Star Allies to crash on boot, as the game was trying to use an invalid min LOD value, leading to an out of range exception.

## 1.0.6353 - 2021-01-18
### Fixed
- Fixed a crash on exit of the emulator and a crash on stopping emulation, both occurring on Linux.
  - Resolves an issue preventing embedded games from launching on Linux (as launching embedded games first stops emulation, which was crashing the emulator).

## 1.0.6352 - 2021-01-18
### Changed
- Reduced temporary copy/fill buffer size from 1GB to 16MB.
  - Fixes a possible out of memory exception if the user is already low on RAM during game load/startup. Does not have any significant impact other than that.

## 1.0.6351 - 2021-01-18
### Added
- Implemented ILibraryAppletCreator::CreateHandleStorage call, and 
  - Fixes the am IStorage Write ReadOnly check.
  - Fixes a regression introduced in 1.0.6291 (PR 1868) that caused Monster Hunter Generations Ultimate to crash on launch in certain scenarios.

## 1.0.6348 - 2021-01-18
### Changed
- Changed joystick inputs to be treated as in a circular zone instead of a square zone.
  - Fixes diagonal analog joystick input not working when mapped to keyboard controls.

## 1.0.6346 - 2021-01-18
### Fixed
- Fixed the missing Ryujinx icon in Linux that has been missing since the GUI refactor.

## 1.0.6341 - 2021-01-17
### Changed
- Implemented lazy flush-on-read for Buffers (SSBO/Copy). This allows SSBO data to be written back to guest memory when it's needed.
  - Fixes flickering in Link's Awakening.
  - This also allows data written into an SSBO to be used as part of a draw command, or by other operations within the GPU. This is required for MH: Rise Demo.
- As part of lazy flushing, when a page is dirtied by CPU, it is assumed that the most recently written SSBOs are not the target of the CPU write, and are not updated.
  - Fixes particle effects that were broken or constantly restarted their animation in many first party games. See the PR for visual examples.
- Greatly speed up buffer copies via two new paths (direct cpu copy, or lazy buffer flush) chosen depending on whether the buffer has been written to before.
  - Mostly affects performance in Unity games and Pokémon (which has other issues).
  - A combination of all changes has fixed vertex explosions in Unity games.
  - Hyrule Warriors vertex explosions are somewhat reduced (but can still happen)
- Fixed a bug with Memory Tracking where write tracking (dirty flags) could be lost after read tracking (flush) was triggered.
  - This fixes a potential crash in Mario Kart 8, where texture data could be flushed at an inappropriate time.

## 1.0.6337 - 2021-01-17
###  Added
- Added support for GPU textures mapped at non-contiguous CPU regions as some games require them, such as the Monster Hunter Rise Demo and some UE4 games.
  - Improves rendering on Monster Hunter Rise Demo and many Unreal Engine 4 games.

## 1.0.6327 - 2021-01-15
### Fixed
- Fixed mipmap base level being ignored for sampled textures and images. This change allows the correct mipmap level to be accessed from the shader.
  - Improves shadow rendering on Monster Hunter Rise Demo.
  - Fixes the black faces/other corruption in Catherine: Full Body. The game now renders correctly.

## 1.0.6324 - 2021-01-14
### Added
- Added a menu option to toggle the prompt displayed when closing the emulator while a game is playing. This can be found in Options > Settings and is called 'Show "Confirm Exit" Dialog'. This option is enabled by default.

## 1.0.6318 - 2021-01-13
### Fixed
- Fixed a bug in the LOP3 shader causing a condition to be read from the wrong bits.
  - As shader changes occurred, shader caches have been gracefully invalidated; the next time a game is launched the cache will automatically rebuild at boot time.

## 1.0.6311 - 2021-01-13
### Changed
- Changed the SurfaceFlinger android-fence callback to be called immediately when the fence is invalid.
  - Fixes an exception when launching homebrew.

## 1.0.6308 - 2021-01-12
### Added
- Implement shader CC mode for ISCADD, X mode for ISETP and fix STL/STS/STG with RZ
  - This adds support for a number of additional shader instruction encodings.
  - This resolves a number of issues with Monster Hunter: Rise Demo, and potentially other games.

## 1.0.6307 - 2021-01-12
### Added
- Implement clear buffer
  - Fixes an issue where certain GPU buffers were not being cleared when using specific NVN features.
  - This resolves a number of issues with Monster Hunter: Rise Demo, and potentially other games.

## 1.0.6301 - 2021-01-12
### Added
- Added a simple pools limiter.
  - Reduces emulator memory usage. The amount of memory usage savings are dependent on the game but test results showed an average of 50% reduction.
  - Avoids memory peaks on launch; spikes nearing or equaling the amount of your RAM should no longer occur.
  - Further reduces game load times, with the amount of time saved varying game to game. Average time reduction of 10-15%.
  
## 1.0.6291 - 2021-01-11
### Added
- Added support for inline software keyboard.
  - Enables Monster Hunter Generations Ultimate and Gnosia to go in-game without a save file, fixes a crash in menus on Dark Souls Remastered, and allows Fate EXTELLA/LINK to enter a custom player name.

## 1.0.6288 - 2021-01-11
### Changed
- Disabled the "Simulate Wake-up" function from being able to be executed while a game is not running.
  - Resolves an error message & potential crash in the emulator if this function is used while not running a game.

## 1.0.6286 - 2021-01-10
### Changed
- Only attempt to parse/load "Common" ticket type.
  - Fixes an issue loading NSPs dumped using nxdumptool with a specific option enabled. 

## 1.0.6283 - 2021-01-10
### Added
- Stubbed IsFreeCommunicationAvailable service call.
  - Fixes a missing service crash in Monster Hunter Rise Demo. The game now boots without having to enable 'Ignore Missing Services'.

## 1.0.6281 - 2021-01-10
### Fixed
- Fixed an issue with compute reserved constant buffers not being updated when, in fact, they should have been.
  - Fixes a bug on Monster Hunter Rise Demo where the screen would randomly turn black with white elements due to compute reading values from the wrong SSBO.
  - Fixes vertex explosions on Super Smash Brothers Ultimate.

## 1.0.6276 - 2021-01-10
### Fixed
- Fixed a recent code typo that broke the auto-updater.\
- DOWNLOAD THIS UPDATE MANUALLY!

## 1.0.6269 - 2021-01-09
### Fixed
- Fixed remap ioctl when the handle value is 0.
  - Fixes an invalid address crash on Monster Hunter Rise Demo.

## 1.0.6260 - 2021-01-08
### Added
- Stubbed PresetLibraryAppletGpuTimeSliceZero service call.
  - Resolves missing service crash on boot on Monster Hunter Rise Demo. Note that the game is still not playable at this time.

## 1.0.6259 - 2021-01-08
### Changed
- Added support for conditional on BRK and SYNC shader instructions.
  - Improves rendering on Monster Hunter Rise Demo, but there are still other bugs preventing most objects from being drawn.
  - As shader changes occurred, shader caches have been gracefully invalidated; the next time a game is launched the cache will automatically rebuild at boot time.

## 1.0.6250 - 2021-01-08
### Changed
- Refactored GUI code. This is part 1 of a multi-part GUI code cleanup.
  - Subscribers to the $10 or $20 monthly Patreon tiers are now listed in the emulator's Help > About section. Thank you for your support!

## 1.0.6248 - 2021-01-07
### Changed
- Updated README.MD to reflect recent emulator changes.

## 1.0.6245 - 2021-01-07
### Fixed
- Updated the missing sample timestamp in DebugPad.
  - Fixes a regression introduced in the HID Sharedmem Rework (PR 1003) that caused several games not to boot. These games now boot again.

## 1.0.6235 - 2021-01-04
### Added
GPU:
- Implemented some missing texture formats.
  - Improves rendering in Psikyo Shooting Stars Alpha and Bravo, Sky Gamblers - Afterburner, and possibly other games that use these texture formats.

## 1.0.6229 - 2021-01-04
### Added
- Implemented Pmull_V instructions with Clmul fast path for the "1/2D -> 1Q" variant & Sse fast path and slow path for both the "8/16B -> 8H" and "1/2D -> 1Q" variants; with Test.

## 1.0.6215 - 2021-01-03
### Fixed
- Fixed a regression introduced by 1.0.6176 (PR 1766) affecting unpacked games.

## 1.0.6213 - 2021-01-02
### Changed
- Refactored & cleaned up account services implementation.

## 1.0.6212 - 2021-01-02
### Fixed
- Fixes a regression introduced in 1.0.6122 (PR 1741) that caused issues with homebrew apps/games not always using fences.

## 1.0.6209 - 2021-01-02
### Added
- Implemented apm:p service call. This seems to be used only by homebrew at this time.

## 1.0.6200 - 2021-01-01
### Changed
- Cleaned up/removed long <-> ulong casts from Nvservices code.
  - Code cleanup only. No emulator functionality change.

## 1.0.6196 - 2021-01-01
### Changed
- Updated KAddressArbiter implementation to 11.x kernel.
  - Update Wait/SignalProcessWideKey implementation to match changes made on official 11.0 kernel.
    - They now store a bool at the key address indicating if the number of threads waiting the condition variable is > 0. Games targeting firmware 11.0 or newer needs this to work properly (no games are know to target it so far).
  - Update SignalAndModifyIfEqual implementation to match changes made on official 7.0 kernel.
    - The way how the value is modified changed. Games targeting firmware 7.0 or newer might need this to work properly.
  - Fix a bug where SignalToAddress would use the old priority even if the thread priority was updated after it started waiting (I'm not sure if this was a bug that was present on 6.x kernel or just an oversight).
  - Simplified the code by sharing the function used to wake matching threads and remove them from the list. The `InsertSortedByPriority` function was also removed since it sorts by priority when looking for threads now.
  - Based on mesosphere implementation. 

## 1.0.6194 - 2021-01-01
### Changed
- Profiled Persistent Translation Cache (PPTC) is now enabled by default.

## 1.0.6190 - 2021-01-01
### Changed
- Update copyright headers for 2021.
  - Happy new Year!

## 1.0.6182 - 2020-12-30
### Changed
- Redistribute updated OpenAL binary.
  - WWe now include a copy of OpenAL out-of-the box with Ryujinx. This removes the need to manually download and install OpenAL. We've also moved from using the fork provided by openal.org to the fork provided by openal-soft.org which is still maintained, and has had 1000's of improvements over the old version. This should improve OpenAL audio issues slightly in some games that suffered previously.

## 1.0.6176 - 2020-12-29
### Added
- Implemented the ability to add new executables when running a game. This allows more possibilities for game modding.

## 1.0.6144 - 2020-12-26
### Changed
- Auto-updater restart after update function now only requires a single click on the "Yes" button.

## 1.0.6141 - 2020-12-24
### Fixed
- Miscellaneous error message verbiage fix. No change in emulator functionality.

## 1.0.6134 / 1.0.6136 - 2020-12-24
### Changed
- Free up memory allocated by Pools (via GC) during any translations at boot time (due to PPTC) and when closing a title.

## 1.0.6127 - 2020-12-17
### Changed
- Fixed Vnmls_S instruction.
- Improved Vfma_S, Vfms_S & Vfnma_S, Vfnms_S instructions performance.
- Added Vfms_V instruction.

## 1.0.6125 - 2020-12-17
### Changed
- Miscellaneous PPTC optimizations to increase resilience and reduce overhead. Note for users: existing .info (profiling) files will be invalidated.
  - Reduction of the size of both the .info and the .cache files.
  - Faster deserialization/serialization (implies reduced loading/saving times).
  - Saving of JitCache memory.
  - Improved Logs.

## 1.0.6122 - 2020-12-17
### Changed
- Interrupt GPU command processing when a frame becomes ready, presenting it immediately.
  - Greatly improves frame pacing in games that do not wait for a present before starting to draw the next frame (and run below full speed)
  - Examples: Xenoblade Chronicles: Definitive Edition, Xenoblade 2
- Vsync event and surface flinger consumption now happens at exact timings (eg. 16.6667ms) rather than rounding to the nearest millisecond (16/17ms)
  - Greatly improves games that rely on the vsync event for timing, such as Link's Awakening, or unity games that aren't tied to frame count.
  - Should result in less framerate fluctuation.
  - Improves frame consistency in windowed mode, though it can still get out of sync and double frames. Exclusive fullscreen mode should be perfect.
- Overhauled Vsync off mode. Rather than simulating a vsync every 1ms, it will now simulate a vsync every time a frame is produced. This results in higher max framerates.
  - Alongside, the artificial 60fps limit has been removed. If you disable your vsync using driver config, or have a high refresh rate screen, you can now go over 60fps.
  - With a 60hz display and driver vsync enabled, you can disable guest vsync to pass through the host vsync signal. This may result in smoother windowed mode gameplay.
- FIFO% now takes a running total of processing time and total time, then calculates the % immediately before reporting it. This is more accurate.
  - Before, it would take % values for each "frame", then average all of them. This was inaccurate when our "frame" boundary failed, or frames had uneven cost.

## 1.0.6119 - 2020-12-16
### Added
- Implemented IsLargeResourceAvailable service call.
  - Fixes missing service crashes in Immortals Fenyx Rising (still crashes due to other issues) and Death Tales (now playable).

## 1.0.6107 - 2020-12-16
### Changed
- Changed the order of termination processes to terminate the application first instead of the services first.
  - Fixes needless service crashes & hangs occurring on emulator shutdown.

## 1.0.6105 - 2020-12-16
### Added
- Implemented VRINTX.F32/VRINTX.F64 instructions.
  - Resolves missing opcode crashes on Spirit Hunter: NG, Fairy Fencer F: Advent Dark Force, Cabela's: The Hunt - Championship Edition, STURMWIND EX, Megadimension Neptunia VII, Baldur's Gate and Baldur's Gate II: Enhanced Editions, Planescape: Torment and Icewind Dale: Enhanced Editions, Little Inferno, Prinny Can I really Be the Hero, Prinny 2 Dawn of Operation Panties Dood, Human Resource Machine, 7 Billion Humans, and TY the Tasmanian Tiger. Most of these titles now go in-game.

## 1.0.6096 - 2020-12-16
### Changed
- Modified JIT cache to be cleared on exit.
- Miscellaneous CPU JIT cache operational cleanup & bugfixes.

## 1.0.6086 - 2020-12-15
### Added
- Implemented aspect ratio options that can be selected either through the Options > Settings > Graphics menu or by clicking through the different options on the bottom status bar. Note: non-16:9 will give games a stretched or squeezed appearance unless also using mods that change the expected resolution of the game.

## 1.0.6082 - 2020-12-15
### Added
- Implemented a wake-up (resume) message/event. This can be triggered while in-game by navigating to Options > Simulate Wake-up Message.
  - Allows the game The World Ends with You -Final Remix- to proceed past a point in the game where it prompts the end user to put the Switch to sleep and then wake it back up.

## 1.0.6069 - 2020-12-14
### Added
- Implemented VFMA (Vector) instructions.
  - Fixes missing opcode crashes on TY the Tasmanian Tiger, Sky Gamblers: Storm Raiders, No more Heroes, No More Heroes 2 Desperate Struggle, Sphinx and the Cursed Mummy, and Valkyria Chronicles.

## 1.0.6062 - 2020-12-14
### Fixed
- Fixed pre-allocator shift instruction copies. No known impact in games.

## 1.0.6052 - 2020-12-13
### Fixed
- Fixed a regression introduced in 1.0.5090 (PR 1413) regarding the STREX instruction implementation that caused Sonic Forces to fail to boot.

## 1.0.6051 - 2020-12-13
### Changed
- Improved shader cache resilience by supporting read-only mode if the cache.zip archive is already opened (i.e. if the game is launched in a second instance of the emulator).

## 1.0.6050 - 2020-12-13
### Fixed
- Corrected the type of NSO executable sizes.
  - Fixes the following games which used to hang forever on boot: Hatsune Miku: Project DIVA Megamix (non-JP version), Super Chariot, Death Mark, Darkest Dungeon, Dairoku: Ayakashimori, CHAOS CODE -NEW SIGN OF CATASTROPHE-, Doukoku Soshite..., Air Missions: HIND, and possibly other (untested) similarly coded games in the switch library. These games now boot and most go in-game.

## 1.0.6047 - 2020-12-12
### Fixed
- Corrected the GetNintendoAccountUserResourceCacheForApplication stub.
  - Fixes a regression introduced in 1.0.6045 (PR 1808).

## 1.0.6045 - 2020-12-11
### Fixed
- Updated fixed-size output buffer functions to write the correct sizes on the pointer buffer descriptors.
  - Fixes a regression introduced in 1.0.5899 (PR 1458).

## 1.0.6043 - 2020-12-11
### Fixed
- Fixed an issue where GL queries could end up failing to return a value after a previous 0-draw query.
  - Resolves a potential multi-second hang/freeze in Luigi's Mansion 3 and possibly other similarly coded games.

## 1.0.6027 - 2020-12-10
### Changed
- Cleaned up the GPU memory allocator code.

## 1.0.6020 - 2020-12-10
### Changed
- Modified allocator code to check if the aligned address and its requirements exceed the maximum allowed address, with a solution if the results are true.
  - Fixed a regression introduced in 1.0.6004 (PR 1722) that caused Asterix & Obelix XXL: Romastered to crash on boot.

## 1.0.6006 - 2020-12-09
### Changed
- Fixed the ngct services placeholder which was incorrect.
- Stubbed IService Match and Filter calls, as well as IServiceWithManagementApi Match and Filter calls.
  - Resolves missing service crashes in Horace, which is now playable.

## 1.0.6005 - 2020-12-09
### Changed
- Modified ThreadPool process handling to keep it active so that there is never a reason to terminate the GateThread.
  - Raises FPS from 57-58 to 60 in games where the game should have been pegged at 60FPS already.

## 1.0.6004 - 2020-12-09
### Changed
- Modified GPU memory allocation by marking memory as available in an implementation of a Red-Black-Tree affectionately christened a TreeDictionary.
  - Improves memory allocation and reduces time for certain GPU memory related operations.

## 1.0.6003 - 2020-12-09
### Changed
- Rewrote scheduler context switch code.
  - Now uses a lock to prevent the thread from being scheduled in two cores at once, like the official OS does.
  - Fixed thread CurrentCore value being updated ahead of time. Now it is only updated after the thread is switched to the new core.
  - Fixed bugs on some yield functions that could cause the scheduler to select the wrong thread.
  - Fixed bugs that would cause KThread and KSession reference counts to be decremented below 0. For KThread, it could cause a exception since the thread was removed from the KProcess thread list twice (only observed in single core mode before the changes).
  - Two asserts were added to ensure the reference count is not decremented below 0, or incremented after the object has been "destroyed".
  - Fixed race on unmap client buffer function (IPC related, not scheduler related, but its a small, simple change as was affecting the tests here as well, so I decided to just fix it).

Other improvements:
- Core idle time is now properly reported to the guest instead of just returning 0 (I am not sure if games can actually read that).
- GetCurrentThread and GetCurrentProcess functions were simplified, now it uses a thread local variable instead of looping through all core contexts and try finding the current thread.
- Context switch speed was improved. This is in part thanks to the use of ManualResetEventSlim, that greatly improves the speed on context switches in quick succession, and in part to the simplification of the function.
- The dummy thread and process were removed. Now GetCurrentThread and GetCurrentProcess just returns null if called from a non-guest or non-service thread.
  - Prevents bugs caused by external threads messing with the scheduler.
  - A new function was added that allows creating a guest process/thread from a external thread, this is used to start service faux processes.
- Allow service threads to run in parallel again.
  - They can be still be blocked by kernel in some cases, like when waiting for IPC messages for example, however they are no longer scheduled to cores, which means that they are free to run in parallel.
  - The YieldUntilCompletion function was removed as it is no longer needed.
  - This matches the behavior before ipc-part2, and fixes issues caused by this specific ipc-part2 change, like the audio crackling on SMO etc.

Other changes:
- Single core mode was removed. Now there is only multi core mode that is on by default.
  - The GUI checkbox was removed.
  - It is worth noting that the single core mode was not working anyway due to the bug mentioned before.

## 1.0.5995 - 2020-12-08
### Added
- Implemented IApplicationFunctions GetHealthWarningDisappearedSystemEvent call.
  - Fixes a missing service issue in Mario Kart 8 Deluxe Chinese version.

## 1.0.5979 - 2020-12-07
### Changed
- Fixed a regression introduced in IPC2 (PR 1458) where data written by a service would not mark regions as dirty, and reads from the service would not trigger flushes from textures.
  - Fixes vertex explosions after one race in Mario Kart 8 Deluxe.

## 1.0.5975 - 2020-12-07
### Added
- Implemented VFNMA CPU instruction.
  - Fixes missing opcode crash on launch in Valkyria Chronicles. Note: this game still does not boot due to other issues.

## 1.0.5960 - 2020-12-07
### Added
- Added support for guest Fz (Fpcr) mode through host Ftz and Daz (Mxcsr) modes (fast paths).
  - Fixes a bug in Luigi's Mansion 3 where a player would get stuck trying to go to floor B2 (or load a save game that places you on floor B2).

## 1.0.5959 - 2020-12-07
### Added
- Added the Ryujinx build version to the log file names generated during runtime.

## 1.0.5942 - 2020-12-03
### Changed
- Texture target is now extracted from Info for quick access
  - Reduces costs when committing texture bindings.

## 1.0.5941 - 2020-12-03
### Added
- Implemented VFNMS.F32/64 instructions.
  - Resolves missing opcode crash on Goat Simulator. Note: this game still crashes due to other issues.

## 1.0.5940 - 2020-12-03
### Changed
- Added a draw count to mitigate unnecessary use of glFlush operations. If no draws have occurred between when a query begins and ends, a glFlush will no longer be added to the queue.
  - Performance improvements can be observed in Super Mario Odyssey's Metro Kingdom or when resolution scaling is active, and other games that had similar bottlenecks previously.

## 1.0.5939 - 2020-12-03
### Changed
- Changed uses of QueryModified to now pass a cached delegate.
- Changed buffer tracking to use simple tracking rather than smart, as in practice smart tracking does not seem to be faster.
  - Combined, these changes nearly halve buffer tracking time in Super Mario Odyssey Metro Kingdom's initial view. Future PRs will help unlock more tangible performance boosts.

## 1.0.5937 - 2020-12-03
### Changed
- This PR is a follow-up to 1.0.5915 and ensures the proper closing of copy handles that are passed by the guest and should be closed.

## 1.0.5915 - 2020-12-02
### Fixed
- Modified the audio WorkBuffer transfer memory handle to close properly.
  - Fixes a regression introduced in 1.0.5899 (PR 1458) that caused a crash or hang on Rune Factory 4 Special and potentially other similarly coded games.

## 1.0.5913 - 2020-12-02
### Changed
- Created a single guest process per IPC message processor, to alleviate issues introduced in 1.0.5899 (PR 1458).
  - Fixes out of memory crashes on Pokémon Sword/Shield and other games encountering this same error.
  - Fixes bad audio on some games after 1.0.5899.

## 1.0.5900 - 2020-12-01
### Fixed
- Fixed a typo in TapFrame logic.
  - Resolves a crash on boot in Pang Adventures.

## 1.0.5899 - 2020-12-01
### Changed
- This is part 2 of an intended 4 part IPC (Inter-Process Communication) refactor. Part 2: HLE services now use the ReplyAndReceive syscall to receive IPC requests, rather than having special handling on the kernel that would call the service function directly after a call to SendSyncRequest by the game.
  - This represents a more accurate approach to these processes, as it attempts to more closely mimic how the Switch OS behaves.

## 1.0.5897 - 2020-12-01
### Changed
- Added a forced early Z test register.
  - Resolves broken interior volumes on Xenoblade Chronicles: Definitive Edition and Xenoblade Chronicles 2.

## 1.0.5895 - 2020-12-01
### Added
- Added a button option to "Remove All" in the DLC and Title Update management windows.

## 1.0.5888 - 2020-12-01
### Added
- Added the ability for a user to launch games in fullscreen mode. 
  - This can be enabled in the options menu or by using a commandline option of "--fullscreen".

## 1.0.5885 - 2020-12-01
### Fixed
- Added missing guest GPU accessor to the hashes computation for shader cache.
  - Existing shader caches will be preserved and migrated to a new version with this update.

## 1.0.5883 - 2020-12-01
### Fixed
- Fixed a warning of the SystemInfo class constructor for macOS. Cleaned up the info / Info at the end of specific OS class names.

## 1.0.5880 - 2020-12-01
### Fixed
- Corrected a parsing issue of the MaxAnistropy field that could cause a crash on different system locales.

## 1.0.5835 - 2020-11-27
### Added
- Added a preference system to selection of matching textures, where it will pick the "most perfect" match available in the texture cache.
  - Fixes white screen issue that appears immediately after loading in The Legend of Zelda: Breath of the Wild.

## 1.0.5834 - 2020-11-27
### Changed
- Disabled resolution scaling on textures smaller than 8px on either width or height. It's very unlikely that textures this small (or thin) contain data that the end user really wants to scale.
  - Fixes overexposed in-game HDR in several Nintendo first-party titles when resolution scaling is enabled.

## 1.0.5833 - 2020-11-27
### Changed
- Added a check when loading the game list that ensures the datetime is valid in current culture.
  - Resolves a rare crash that could occur when sorting the game list by last played datetime if the host PC datetime had been modified.

## 1.0.5831 - 2020-11-27
### Changed
- Cleaned up unnecessary warnings and motion client code.

## 1.0.5816 - 2020-11-24
### Added
- Implemented IsRestrictionEnabled call of the pctl service.
  - Fixed an issue where the game Momotaro Dentetsu: Showa, Heisei, Reiwa mo Teiban! would not boot after a save had been created.

## 1.0.5815 - 2020-11-24
### Fixed
- Fixed IApplicationFunctions::GetSaveDataSize call of the am service.
  - Resolves a crash on boot on The Language of Love (which now goes in-game) and potentially other games that had been suffering from a similar issue.

## 1.0.5796 - 2020-11-21
### Changed
- Cleaned up IApplicationFunctions.
- Stubbed TryPopFromFriendInvitationStorageChannel.
  - Resolves missing service crashes in Mini Motor Racing X and Fight Crab, which are now playable.

## 1.0.5795 - 2020-11-21
### Fixed
- Fixed reverb 3d mono having the wrong delay line offset (was using -1 instead of 1). Added this value to be used by all channel variants.
  - Resolves a crash on boot in Resident Evil 6. This title now goes in-game.

## 1.0.5794 - 2020-11-21
### Changed
- Removed an incorrect debug assert within the shader cache code.

## 1.0.5793 - 2020-11-21
### Fixed
- Fixed an underflow in the setup of delay time within the delay effect.
  - Fixes a regression introduced in Amadeus causing a crash on boot on Shovel Knight: Treasure Trove. The game is once again playable.

## 1.0.5792 - 2020-11-21
### Fixed
- Fixed an incorrect id for call ListAudioInsAuto in IAudioInManager.

## 1.0.5783 - 2020-11-20
### Changed
- Implemented the following audout service calls: GetAudioOutBufferCount, GetAudioOutPlayedSampleCount, FlushAudioOutBuffers.
  - Resolves crashes therein, and enables the following titles to go in-game: Devil May Cry 2, Devil May Cry 3 Special Edition, Atelier Shallie: Alchemists of the Dusk Sea DX, and others that need these calls.
- Fixed SetAudioOutVolume and GetAudioOutVolume.

## 1.0.5782 - 2020-11-20
### Changed
- Allowed copy destinations to have a different scale from the source. This has the following effects on resolution scaling:
  - Now works in The Legend of Zelda: Link's Awakening (with some shadow artifacts).
  - No longer blacklists/stops working in some cutscenes in Super Mario Odyssey.

## 1.0.5778 - 2020-11-20
### Changed
- Compressed <-> uncompressed copies are now performed using pixel buffer objects.
  - Fixes most button text and large UI text in Splatoon 2. Remaining text issues in this game will be resolved in a future PR.
  - Fixes cubemap mip levels below 4x4 not being copied in Mario Kart 8 Deluxe. No visual impact at this time.
  - May improve Unreal Engine 3/4 games that contain black small mipmap levels. Does not fix them entirely.

## 1.0.5775 - 2020-11-19
### Added
- Added PTC decompression error check and treats it as a loading failure, allowing a fallback to the backup file instead of crashing.

## 1.0.5773 - 2020-11-19
### Added
- Added olsc:u service and stubbed some associated calls.
  - Fixes boot-time crash for Animal Crossing: New Horizons update version 1.6.0.

## 1.0.5763 - 2020-11-18
### Added
- Added click-to-toggle functionality for Vsync on/off and Docked/Handheld modes in the status bar (bottom of the Ryujinx window). Simply click once on the respective item in the status bar to toggle.

## 1.0.5761 - 2020-11-18
### Added
- Added the ability to use a keybinding to toggle between Docked and Handheld modes at any time. To toggle between Docked and Handheld, press the F9 key.

## 1.0.5756 - 2020-11-18
### Changed
- Modified buffer textures to restrict layout conversions from being performed.
  - Fixes possible memory corruption in games that use buffer textures e.g. Unreal Engine 4.

## 1.0.5755 - 2020-11-18
### Changed
- Added FP16/FP32 fast paths for Fcvt_S, Fcvtl_V & Fcvtn_V instructions.
- Modified HardwareCapabilities to use CpuId.

## 1.0.5754 - 2020-11-18
### Changed
- Updated & cleaned up code to take advantage of .NET 5 functions/bugfixes. No expected changes in any games.

## 1.0.5753 - 2020-11-18
### Fixed
- Fixed possible parsing errors of information on NSOs at load time.

## 1.0.5742 - 2020-11-17
### Changed
- Removed some redundant code related to OpenGL depth testing. Should not affect any games.

## 1.0.5740 - 2020-11-17
### Fixed
- Fixed a regression introduced in 1.0.5624 (PR 1670) that affected homebrew using nouveau, and possibly OpenGL games, where the full texture data was not copied.

## 1.0.5739 - 2020-11-17
### Changed
- Updated README.MD for clarity and a working .NET 5 build command.

## 1.0.5738 - 2020-11-17
### Fixed
- Fixed a regression introduced in 1.0.5674 (PR 1701 - Shader Cache implementation) that caused the Linux client to crash on startup.

## 1.0.5737 - 2020-11-17
### Fixed
- Fixed a possible crash at startup when cleaning up missing shaders.

## 1.0.5736 - 2020-11-17
### Fixed
- Resolved an issue causing the virtual address of texture descriptors not be cleaned up when caching, instead cleaning texture format and swizzle.
  - Should fix high duplication and possible texture corruption for certain games introduced with the advent of disk shader cache. 
**NOTE: This will invalidate all cache layers as this is a critical bugfix on the cache saving system.**

## 1.0.5727 - 2020-11-16
### Fixed
- Fixed a regression introduced in 1.0.5617 (PR 1647) that negatively affected performance in The Legend of Zelda: Link's Awakening.

## 1.0.5726 - 2020-11-15
### Fixed
- Fixed a regression introduced in 1.0.5718 (PR 1688) that enabled VR by default. This caused graphical issues in games such as Super Smash Bros. Ultimate when VR mode is not used.

## 1.0.5718 - 2020-11-15
### Added
- Implemented VR rendering in games supported by the Toy-Con VR Goggles.

## 1.0.5709 - 2020-11-15
### Added
- Added a missing error message for ApplicationNotFound.

## 1.0.5708 - 2020-11-15
### Changed
- Changed a menu option to be camel case where it had not been previously.

## 1.0.5707 - 2020-11-15
### Changed
- Stopped the GUI from prompting the user to disable debug features in debug builds.

## 1.0.5705 - 2020-11-15
### Changed
- Migrated project and CI to .NET 5. This update brings across the board improvements to JIT performance.
  - Minor FPS improvements in some games.
  - Significant performance increase in some NVDEC videos, especially on client CPUs that do not have hyperthreading. Up to 130% increase in performance was observed in Super Smash Bros. Ultimate intro video during testing.

## 1.0.5683 - 2020-11-12
### Fixed
- Fixed patch sets toggling for mods.

## 1.0.5682 - 2020-11-12
### Added
- Added an 'Apply' button in the Options menu. This allows setting & saving options without having to close the menu.

## 1.0.5678 - 2020-11-12
### Changed
- Modified size hints for render targets to use "Screen Scissor" instead of viewport 0's width.
  - Fixes a regression in Hyrule Warriors: Age of Calamity Demo cubemaps.

## 1.0.5674 - 2020-11-12
### Added
- Implemented a disk shader cache.
  - Progressively reduces stutter & FPS drops on subsequent play of a game.
  - Optimized shader compile process to be more efficient.

## 1.0.5667 - 2020-11-11
### Fixed
- Fixed a bug that caused Super Mario Odyssey to behave as if it was in handheld mode, even when Docked mode was enabled.

## 1.0.5659 - 2020-11-10
### Fixed
- Do not report unmapped pages as modified to the GPU.
  - Fixes a regression caused by range tracking (PR #1272) that could cause a few games (like Higurashi no Naku Koro ni Hou) to crash.

## 1.0.5655 - 2020-11-09
### Fixed
- Fix a bug that could cause data loss when copying on rendering to a texture.
  - Fixes color bleeding on Luigi's Mansion 3 caused by dynamic resolution change, improves reflections on Mario Kart 8 Deluxe.

## 1.0.5653 - 2020-11-09
### Changed
- Implement ATOM shader instruction and fix offset decoding on the ATOMS shader instruction.
  - Might improve rendering on games using this instruction, although no improvement was observed on games know to use them.

## 1.0.5650 - 2020-11-09
### Changed
- Simplify bindless textures handling.
  - Might cause minor changes to games using bindless textures, such as Super Mario Party.

## 1.0.5644 - 2020-11-08
### Changed
- Un-stubbed & implemented the following IPC calls:
  - nn::apm::IManager (fully implemented)
  - nn::apm::ISession (fully implemented)
  - nn::apm::ISystemManager (partially implemented)
- Fixed some incorrect calls in nn::appletAE::ICommonStateGetter

## 1.0.5643 - 2020-11-08
### Changed
- Modified shader code to use explicit binding points rather than calling the OpenGL 'glGet*Location' functions.
  - Should mildly increase binding performance.

## 1.0.5639 - 2020-11-07
### Fixed
- Update rasterizer discard before texture clears.
  - Fixes shadow trail bug on Xenoblade Chronicles 2.

## 1.0.5636 - 2020-11-06
### Added
- Added a check to ensure GPU commands are in the queue before continuing.
  - Fixes FIFO % being stuck at 50% on some games.

## 1.0.5634 - 2020-11-06
### Changed
- Updated README.md

## 1.0.5629 - 2020-11-06
### Changed
- Enabled expanded logging of NSO files: Module name, Libraries and SDK Version are now logged when they are available. Also cleaned up NsoExecutable class a bit.

## 1.0.5628 - 2020-11-06
### Fixed
- Fixed a regression introduced in 1.0.5584 (PR 1643) that caused the next boot of a just auto-updated emulator to try to launch itself as a game.

## 1.0.5626 - 2020-11-06
### Added
- Added support for single precision constants for double precision operations. No expected changes in any games.

## 1.0.5625 - 2020-11-06
### Changed
- Modified buffer textures to have their data inherited by using the same buffer storage (instead of matching aligned sizes).
  - Fixes a random crash in Hyrule Warriors: Age of Calamity Demo and may improve other games that use buffer textures.

## 1.0.5624 - 2020-11-06
### Fixed
- Corrected the bytes per pixel value being passed. This fixes memory corruption that occurs by too much data being copied in some UE4 games.
  - BRAVELY DEFAULT II Demo and Remothered: Broken Porcelain get further in the boot process, both able to reach the title screen now, before crashing. Other UE4 games may also benefit.

## 1.0.5617 - 2020-11-05
### Fixed
- Increased depth-stencil accuracy by modifying zeta formats to only be valid for depth-stencil render targets. No expected changes in any games.

## 1.0.5611 - 2020-11-05
### Fixed
- Miscellaneous CPU emulation code fix.

## 1.0.5604 - 2020-11-02
### Added
- Added seamless cubemap flag in sampler parameters.
  - Removes hard edges from cubemaps, and specifically improves low resolution cubemaps (used for glossy surfaces). Improvements are visible in Mario Kart 8 Deluxe.

## 1.0.5603 - 2020-11-02
### Changed
- Support resolution scaling on images. Correctly blacklist for SUST. Move logic out of the backend.
  - Fixes normal-map decals in Xenoblade Chronicles: Definitive Edition.
  - Should cause Paper Mario and Dead or Alive to force to 1x/native resolution instead of just breaking.
  - May fix other strange issues related to resolution scaling.

## 1.0.5597 - 2020-11-01
### Fixed
- Fixed compressed to non-compressed texture copy size. No expected changes in any games.

## 1.0.5596 - 2020-11-01
### Changed
- Removed unused texture and sampler pool invalidation code. No expected changes in any games.

## 1.0.5584 - 2020-10-29
### Changed
- Command line arguments are now preserved when the auto-updater restarts Ryujinx. This prevents unexpected/unwanted behavior that could delete files in a custom specified user folder.

## 1.0.5583 - 2020-10-29
### Changed
- Miscellaneous resolution scaling change: now scales the texture size before sending it to the OpenGL backend.
  - Resolution scaling now works on games that present BGRA textures such as Dragon Marked for Death among others.

## 1.0.5580 - 2020-10-28
### Added
- Added scaling for Texture2DArray when using TexelFetch.
  - Fixes resolution scaling in Hyrule Warriors: Age of Calamity Demo.

## 1.0.5579 - 2020-10-28
### Changed
- Implemented a stop-gap solution to avoid sampler conflicts on bindless samplers with the same name. This should suffice until proper bindless texture support is implemented.
  - Improves rendering in Super Mario Party.

## 1.0.5577 - 2020-10-28
### Changed
- Improved motion controls.
  - Reduces input lag.
  - Alt slots now work for paired joycons.
  - Fixes issue with input settings not saving unless default profile is loaded.

## 1.0.5573 - 2020-10-25
### Fixed
- Updated vertex buffer handle null checks to prevent draws with invalid states.

## 1.0.5569 - 2020-10-25
### Fixed
- Miscellaneous transform feedback fixes.
  - Fixes T-pose in SNK Heroines: Tag Team and black grass in Xenoblade Chronicles: Definitive Edition.

## 1.0.5568 - 2020-10-25
### Changed
- Modified ASTC texture data update handling to skip updates if the data did not change.
  - Improves performance on SNK Heroines and potentially other games that use ASTC textures similarly.

## 1.0.5567 - 2020-10-25
### Added
- Implemented CAL and RET shader instructions.
  - Fixes a regression introduced in 1.0.5514 (PR 1609) that could cause a crash during battle in Fire Emblem: Three Houses.

## 1.0.5553 - 2020-10-21
### Changed
- Removed Reflection.Emit's dependency on CPU and Shader projects.
  - These projects are now more readily compatible with any potential future AOT implementations.

## 1.0.5552 - 2020-10-21
### Changed
- Added a missing null check on image binding.
  - Resolves a regression introduced in 1.0.5549 (PR 1625) that could result in a crash.

## 1.0.5549 - 2020-10-20
### Fixed
- Fixed image binding format.
  - Resolves broken fire & ice effects in Kirby Star Allies as well as incorrect dark/black ground in Xenoblade Chronicles Definitive Edition.

## 1.0.5548 - 2020-10-20
### Fixed
- Changed buffer textures to ensure storage is set when binding an image.
  - Fixes Pascal (NVIDIA GTX 10-series) GPUs suffering a driver crash when booting Unreal Engine 4 games. Note: there are remaining issues with UE4 games to be addressed in the future.

## 1.0.5545 - 2020-10-17
### Fixed
- Miscellaneous geometry shader fix. No known changes in games.

## 1.0.5543 - 2020-10-16
### Changed
- Improved texture/buffer modification detection and flushing methods.
  - Fixes missing thumbnail pictures in many games as well as the passport photo on Animal Crossing: New Horizons.
  - Fixes broken logic in Snipperclips/Plus, Pooplers, Shanky: The Vegan's Nightmare and potentially other games that use similar logic.

## 1.0.5525 - 2020-10-13
### Fixed
- Fixed LOP3 (cbuf) shader instruction encoding.
  - Resolves rendering issues on DOOM 64. The game is now playable.

## 1.0.5523 - 2020-10-13
### Changed
- Replaced "Host FPS" counter with GPU Command Queue Load reading "FIFO %".

## 1.0.5522 - 2020-10-13
### Added
- Implemented several 32-bit CPU instructions.
  - Super Mario Galaxy is now bootable inside Super Mario 3D All-Stars.
  - Fixes missing opcode crashes on Dies irae Amantes amentes and Sky Gamblers: Storm Raiders. Note: neither of these games are able to go in-game still, but Sky Gamblers: Storm Raiders now boots, playing music briefly at a black screen.

## 1.0.5517 - 2020-10-13
### Fixed
- Fixed incorrect OpenGL BlendFunc enumeration values
  - The value of some OpenGL BlendFunc values were incorrect.
  - Fixes broken shadows in Super Mario Galaxy, and potentially other games with similar behaviour.

## 1.0.5516 - 2020-10-13
### Fixed
- Fixed output component register ordering on pixel shaders
  - In some instances, the incorrect register was being copied to an output component.
  - Improves rendering in Super Mario Galaxy, and potentially other games with similar behaviour.

## 1.0.5515 - 2020-10-12
### Fixed
- Fixed a error when dual source blend was used.
  - Fixes rendering inside the observatory on Super Mario Galaxy.

## 1.0.5514 - 2020-10-12
### Fixed
- Implemented LEA.HI shader instruction.
  - Fixes corrupted colors on some characters on Super Mario Galaxy (example: Rosalina), and other graphical issues. Might also fix graphical issues on other games.

## 1.0.5513 - 2020-10-12
### Fixed
- Implemented support for constant buffer slot indexing, with the LDC shader instruction.
  - Fixes some 3D models not rendering on Super Mario 3D All-Stars.

## 1.0.5499 - 2020-10-11
### Fixed
- Fixed NVDEC FFMPEG related crash that could occur when frame sizes change.
  - Resolves random crashes in Super Mario 3D All-Stars main menu/launcher. May also fix similar crashes in other games.

## 1.0.5496 - 2020-10-10
### Changed
- Disabled the async buffer in SurfaceFlinger.
  - Fixes a memory corruption on Super Mario All-Stars 3D, which now has improved compatibility but still does not go in-game without modifications. May also fix crashes on other games.

## 1.0.5493 - 2020-10-09
### Added
- Added a confirmation dialog prompt to the user when attempting to close the emulator while a game is being emulated.

## 1.0.5492 - 2020-10-09
### Fixed
- Fixed an issue where attempting to stop emulation while playing an embedded game within a multi-game collection would instead cause emulation to restart.

## 1.0.5485 - 2020-10-02
### Changed
- Improved BRX target detection heuristics.
  - Koi no Hanasaku Hyakkaen now renders properly. This may also potentially affect other games.

## 1.0.5480 - 2020-10-01
### Fixed
- Support 2D array ASTC texture decoding.
  - Fixes black objects on Donkey Kong Country: Tropical Freeze.

## 1.0.5476 - 2020-09-30
### Fixed
- Fixed a bug in GetStream. This function is not currently used but may be used in the future.

## 1.0.5468 - 2020-09-30
### Changed
- Miscellaneous legacy CPU code cleanup.

## 1.0.5459 - 2020-09-29
### Added
- Implemented motion controls.
  - See https://github.com/ryujinx-mirror/ryujinx/wiki/Ryujinx-Setup-&-Configuration-Guide#motion-controls for a usage guide.

## 1.0.5457 - 2020-09-29
### Changed
- Modified the auto-updater to not notify for an update if the Appveyor package is still building on the website.

## 1.0.5456 - 2020-09-29
### Changed
- Modified one-dimensional texture handling to be instead treated as two-dimensional textures with a height of 1.
  - Fixes fog/mist effects in Monster Hunter Generations Ultimate and potentially other one-dimensional texture situations.

## 1.0.5455 - 2020-09-29
### Added
- Implemented auto-updater functionality.
  - Ryujinx will now check on startup for the latest official master build and prompt the user to download & install it if a new version is found. It can also be performed by clicking Help > Check for Updates.
  - You may disable the 'Check for updates on launch' function in Options> Settings > General tab.

## 1.0.5442 - 2020-09-27
### Added
- Implemented a basic error applet.
  - Fixes crashes in REKT and Ghost Blade HD, both of which are now considered playable.
  - Adds certain necessary functionality for LDN.

## 1.0.5439 - 2020-09-26
### Changed
- Changed texture copy handling to avoid issues with overlapping textures.
  - Fixes a regression introduced in 1.0.5326 (PR 1408), resolving graphical issues in Professor Layton, Snack World, and potentially other games that use OpenGL on the guest.

## 1.0.5428 - 2020-09-25
### Changed
- Isolated more active services to their own threads: SurfaceFlinger processes, HID, and Time services.

## 1.0.5418 - 2020-09-23
### Changed
- Implemented small indexed draws and primitive topology override. This affects graphics emulation for games that use the Vulkan API on the Switch.
  - Enables graphics to be rendered on Turok 2, Doom 64, and other games that use the Vulkan API.
- Miscellaneous minor gpu fixes.

## 1.0.5417 - 2020-09-23
### Changed
- Return "NotAvailable" when no UserChannel data is present.
  - Fixes a crashing regression in Splatoon 2 and Super Kirby Clash (among possible others) introduced in 1.0.5396 (PR 1560).

## 1.0.5414 - 2020-09-23
### Changed
- Audio renderer services are now processed in a separate thread.
  - Fixes audio stutter in certain conditions introduced in 1.0.5402 (PR 1447).

## 1.0.5402 - 2020-09-21
### Changed
- This is part 1 of an intended 4 part IPC refactor. Part 1: Explicit IPC Servers to match Horizon IPC implementation and miscellaneous fixes.
  - Fixes Ryujinx application hang on certain games when emulation is stopped or when the application is commanded to exit.

## 1.0.5398 - 2020-09-21
### Fixed
- Fixed a texture bug revealed by 1.0.5326 (PR 1408).
  - Resolves graphical regressions in Hatsune Miku: Project DIVA Mega mix and a few other games.

## 1.0.5396 - 2020-09-20
### Added
- Implemented basic multi-program support.
  - Adds basic support for loading multiple NCA's. Improves support for Super Mario 3D All-Stars. 

## 1.0.5395 - 2020-09-20
### Changed
- Align register index between output targets on fragment shaders
  - Fixes behaviour in certain games where components would be incorrectly copied to fragment shader outputs. Improves Turok 2, and potentially other Vulkan-based games. 

## 1.0.5391 - 2020-09-19
### Added
- Stubbed/implemented ListQualifiedUsers and SetTouchScreenConfiguration calls.
  - Fixes missing service crashes in Clubhouse Games: 51 Worldwide Classics, Dr. Kawashima's Brain Training, Cyber Protocol, Bulletstorm: Duke of Switch Edition, Catherine Full Body, Truck & Logistics Simulator, CONTRA: ROGUE CORPS, Warhammer 40,000: Space Wolf, Borderlands 2: Game of the Year Edition, Our Two Bedroom Story, Grandia HD Collection, XCOM 2 Collection, Kissed by the Highest Bidder, Love Letter from Thief X, Baldur's Gate and Baldur's Gate II: Enhanced Editions, Planescape: Torment and Icewind Dale: Enhanced Editions, Psikyo Shooting Stars Alpha, Psikyo Shooting Stars Bravo, LA-MULANA 1&2, and possibly other games that are not yet tested in the Ryujinx game compatibility list.

## 1.0.5390 - 2020-09-19
### Changed
- Moved the "Open Logs Folder" function/button out from the application's Settings window and instead placed it under the main window File > menu.

## 1.0.5389 - 2020-09-19
### Added
- Stubbed/implemented GetCompletionEvent and AddPlayHistory calls.
  - Fixes missing service crash in Worms W.M.D.

## 1.0.5388 - 2020-09-19
### Added
- Stubbed several Begin/EndBlockingHomeButton calls.
  - Fixes network related crashes in ARMS.

## 1.0.5387 - 2020-09-19
### Added
- Stubbed SetShimLibraryVersion service call.
  - Fixes missing service crash in Puzzle Book.

## 1.0.5386 - 2020-09-19
### Fixed
- Fixes stack overflow crash in a few games, including Blaster Master Zero 2 and Terraria.

## 1.0.5383 - 2020-09-19
### Changed
- JIT optimization: Reorder blocks placing cold blocks at the end of the function.
  - Might improve speed of some games slightly.

## 1.0.5381 - 2020-09-19
### Fixed
- Better viewport flipping method.
  - Fixes upside down screen on some games on AMD, Intel and old NVIDIA GPUs. Example: Fire Emblem Three Houses on AMD.
- Better depth mode detection method.
  - Fixes lighting issues in a few games, including Zelda Link's Awakening and Burnout Paradise Remastered.

## 1.0.5374 - 2020-09-19
### Fixed
- Fixes threshold for control stick buttons reported to games.

## 1.0.5346 - 2020-09-12
### Fixed
- Fixes debug Config.json using resolution scaling 2x by default.

## 1.0.5341 - 2020-09-11
### Changed
- Miscellaneous CPU emulation code optimization.

## 1.0.5327 - 2020-09-10
### Fixed
- Allow swizzles to match with "undefined" components
  - Fixes motion blur and depth of field in Burnout Paradise.

## 1.0.5326 - 2020-09-10
### Fixed
- Write-back texture data from memory when it falls out of the cache, and perform untracked buffer write-back.
  - Fixes random texture corruption in Super Mario Odyssey (example: Black squares on Sand Kingdom).
  - Fixes black screen on Mario Kart 8 Deluxe in some courses.
  - Reduces or completely fix vertex explosions in some Unity games.
  - Improves performance of games that performs buffer copies (such as aforementioned Unity games, may also give a small boost to Pokémon games).

## 1.0.5306 - 2020-09-06
### Changed
- Miscellaneous CPU emulation code optimization.

## 1.0.5305 - 2020-09-06
### Fixed
- Fixed issue with music playing on only the left channel in several games using REV8 including Fairy Tail, SEGA AGES Sonic the Hedgehog 2, Family Mysteries: Poisonous Promises, and Harukanaru Toki no Naka de 7.

## 1.0.5292 - 2020-09-05
### Changed
- Removed redundant sentence in README.MD. No emulator code changes.

## 1.0.5271 - 2020-09-01
### Changed
- Updated LibHac to version 0.12.0. General improvements to system stability and backend file system fixes & enhancements.
  - Support reading NCA0 files and decrypted NCAs.
  - Improves the performance of some save data operations when large numbers of save data exist.

## 1.0.5270 - 2020-09-01
### Fixed
- Miscellaneous CPU code fixes.
  - Resolves a PPTC related crash during PPTC compilation.

## 1.0.5269 - 2020-09-01
### Fixed
- Miscellaneous shader instruction fixes.
  - Resolves incorrect dithering pattern in Super Mario Odyssey.

## 1.0.5266 - 2020-09-01
### Changed
- Reworked in-application user error prompts regarding keys & firmware to be more meaningful to the end user.
  - Replaces KEYS.MD pop-up with a more appropriate message including a link to the setup guide.

## 1.0.5264 - 2020-09-01
### Fixed
- Fixed a regression with texture compatibility checks introduced in build 1.0.5258 (PR 1482)

## 1.0.5258 - 2020-08-31
### Changed
- Miscellaneous GPU code refactoring/improvement.

## 1.0.5256 - 2020-08-31
### Changed
- Miscellaneous CPU code optimization.

## 1.0.5255 - 2020-08-31
### Added
- Implemented fixed point variants Scvtf_S_Fixed & Ucvtf_S_Fixed with Tests.
  - Resolves missing opcode errors in a particular Switch homebrew application.

## 1.0.5251 - 2020-08-30
### Changed
- Removed the unused Ryujinx.Debugger project.

## 1.0.5250 - 2020-08-30
### Added
- Ryujinx now allows launching with custom data directories i.e. "portable mode"

## 1.0.5249 - 2020-08-30
### Changed
- Cleaned up the FUNDING.yml file

## 1.0.5248 - 2020-08-30
### Changed
- Cleaned up and updated the readme file.
  - No emulator code changes.

## 1.0.5246 - 2020-08-29
### Fixed
- Fixed a page count calculation regression introduced in 1.0.4453 (PR 856).
  - Resolves a bug found in deko3d homebrew. There are no known games affected by the bug or this fix.

## 1.0.5239 - 2020-08-27
### Fixed
- Fixed inverted downmixing of center and LFE channels.
  - Resolves missing background music in Mario Tennis Aces, missing voices in Fire Emblem: Three Houses and Resident Evil: Revelations, and other 
 missing audio in similarly affected games. 

## 1.0.5221 - 2020-08-23
### Changed
- HID now allows reconfiguration of controllers during runtime! No need to restart emulation.
- Controller Applet now supports multiple controllers
  - Mario Tennis Aces console multiplayer now works properly. Other games that had similar issues with the controller applet should also be working now.
- Controller Applet now displays a Message dialog indicating config issues if any, and waits for user to correct them.
- HID now ignores Handheld config when Docked mode is enabled
- Auto-reassignment is dialed back (wasn't assigning valid configs most of the time anyway) to rely more on Controller Applet.
- Fun change: Adds official LED patterns to Player IDs
- Misc change: Hotkey toggles (such as Vsync) now only work when Emu window is focused.
- Controller Setup Window now only shows valid options for respective types e.g. prevents setting up Pro Controller to Handheld

## 1.0.5200 - 2020-08-19
### Fixed
- Fixed some build warnings and debug asserts caused by uninitialized fields on the VP9 decoder.
  - Does not affect gameplay or performance in any way.

## 1.0.5191 - 2020-08-18
### Added
- Implemented IManagerForApplication calls and IAsyncContext.
  - Fixes missing service crashes in many games, but nearly all of those games need further services implemented before they become bootable/playable.

## 1.0.5190 - 2020-08-18
### Changed
- Implemented software surround downmixing.
  - Fixes a crash when no audio renderer was created when stopping emulation.
- Disables support of 5.1 surround on OpenAL backend as Ryujinx cannot currently detect whether the hardware directly supports it.

## 1.0.5182 - 2020-08-17
### Changed
- Implemented all Switch audio renderer functions (code name Amadeus).
  - Resolves myriad audio issues, including: garbled audio in nearly every game that uses audren, softlock in Splatoon 2, slowness in NVDEC videos on Unity titles, loud effects that should be soft, super slow sounding audio during full speed gameplay, etc.  

Known issues: 
- Fire Emblem: Three Houses is missing voices in videos.
- Games that utilize two simultaneous audio renderers will likely have issues until the audio out interface is rewritten; Crash Team Racing is the only known game to suffer from this. 

## 1.0.5177 - 2020-08-14
### Added
- Added missing depth-color conversions in CopyTexture.
  - Resolves an openGL error message being spammed, and fixes the broken blur and performance regressions in Zelda: Link's Awakening after BGRA support was added. Note: there are still additional graphical glitches in this game.

## 1.0.5175 - 2020-08-13
### Fixed
- The Purge PPTC cache option now purges all versioned PPTC caches for a particular game; previously it was only purging the PPTC cache for the currently selected game version e.g. 1.0.3.

## 1.0.5173 - 2020-08-13
### Fixed
- Fixed MacroJIT SubtractWithBorrow Alu Reg Operation.
  - Resolves black screen & memory leak issues with Duke Nukem 3D: 20th Anniversary World Tour (and possibly other games), and a crash on launch in Waifu Uncovered. 

## 1.0.5171 - 2020-08-12
### Added
- Added VFMA/VFMS instructions.
 - Fixes missing opcode crashes on Duke Nukem 3D: 20th Anniversary World Tour, STURMWIND EX, Cabela's: The Hunt - Championship Edition, and Goat Simulator.
### Changed
- Fixed VCVT_FI & VCVT_RM. Added tests for VCVT_FI. Updated VRINT_RM & VRINT_Z.
  - Fixes missing graphics in several 32-bit titles including (but not limited to) SamuraiAces, and Duke Nukem 3D: 20th Anniversary World Tour.

## 1.0.5167 - 2020-08-12
### Fixed
- Fixed the InitializeBluetoothLe call which previously did not return any event handle.

## 1.0.5155 - 2020-08-09
### Changed
- Changed default audio backend to be OpenAL by default. If OpenAL is not found, it will default to SoundIO.
  - Resolves audio renderer crashes due to Dummy being selected by default.

## 1.0.5152 - 2020-08-09
### Fixed
- Incremented PPTC version, which was missed in 1.0.5144 (PR 1433).

## 1.0.5144 - 2020-08-08
### Fixed
- Miscellaneous CPU emulation fixes.

## 1.0.5143 - 2020-08-08
### Added
- Forwarded OpenSaveDataInfoReaderOnlyCacheStorage call to OpenSaveDataInfoReaderWithFilter.
  - Fixes a missing service crash in Mortal Kombat 11. The game now boots to the title screen.

## 1.0.5142 - 2020-08-07
### Added
- Added issue templates on the Ryujinx GitHub site. No emulator code was changed.

## 1.0.5140 - 2020-08-07
### Changed
- Renamed openGL debug logging option "performance" to "slowdowns" to reduce the likelihood of an end user, believing there to be some sort of associated performance gain in the emulator, errantly enabling the option.

## 1.0.5138 - 2020-08-06
### Changed
- Silenced several build warnings (for those who build their own version of Ryujinx) and performed minor code cleanup.

## 1.0.5131 - 2020-08-04
### Changed
- Miscellaneous CPU emulation code optimization. There should not be any noticeable changes in emulator performance.

## 1.0.5126 - 2020-08-03
### Changed
- Rewrote the logger to optimize operation and reduce unnecessary overhead. Also removed SendVibrationXXX logs from standard output; they are now only present if debug logging is enabled.

## 1.0.5123 - 2020-08-02
### Added
- Implemented a Macro JIT.
  - Improves the speed of GPU Macro execution by recompiling code on-demand; as a result, minor speed improvements may be seen in games that use instanced draws.

## 1.0.5122 - 2020-08-02
### Added
- Implemented software keyboard GTK frontend. 
  - Enables the user to enter a custom player name when prompted instead of having the default "Ryujinx" name hardcoded in, as well as input custom text any time the user is prompted throughout the game play experience.

## 1.0.5119 - 2020-08-02
### Added
- Facilitated OpenGL debug logging that can be configured via the GUI. This is for development use only.

## 1.0.5115 - 2020-08-01
### Added
- Implemented IServiceCreator::GetPlayHistoryRegistrationKey call.
  - Fixes missing service crashes on Diablo III: Eternal Collection, BLAZBLUE CENTRALFICTION, The Escapists 2, and other games that utilize this call.

## 1.0.5105 - 2020-07-31
### Added
- Added stub for ReadSaveDataFileSystemExtraDataWithMaskBySaveDataAttribute call.
  - Fixes missing service crash in Animal Crossing: New Horizons 1.4.0; "ignore Missing Services" no longer needs to be enabled.

## 1.0.5094 - 2020-07-30
### Fixed
- Fixed WMI exception errors on startup in edge cases e.g. where the WMI service is not running, etc.

## 1.0.5088 - 2020-07-30
### Changed
- Changed logging to print the guest stack trace on several crash conditions, which will provide necessary troubleshooting information.

## 1.0.5087 - 2020-07-29
### Changed
- Refactored shader translator ShaderConfig and reduced number of out args.
  - Improves code without changing any functionality.

## 1.0.5086 - 2020-07-29
### Changed
- Miscellaneous CPU emulation code optimization.

## 1.0.5072 - 2020-07-28
### Fixed
- Fixed a shader regression on Intel iGPUs (and possibly AMD GPUs) by reverting varying layout changes introduced in 1.0.4937 (PR 1370). NOTE: transform feedback is not going to work on Intel iGPUs in Windows.

## 1.0.5071 - 2020-07-28
### Added
- Implemented alpha test using legacy functions using an OpenGL compatibility profile. This will bridge the gap until alpha test is re-implemented via pixel shaders later on once a disk shader cache exists in Ryujinx.
  - Fixes alpha test issues on games including Mega Man 11 (black eyes, etc.).

## 1.0.5070 - 2020-07-28
### Added
- Implemented VIC BGRA output surface format.
  - Fixes swapped/incorrect colors during video decoding on games requiring it, including The Caligula Effect.

## 1.0.5049 - 2020-07-26
### Added
- Stubbed several eShop related calls.
  - Fixes related missing service crashes on Pokémon Café Mix, Dragon Quest XI, Dead by Daylight, Ninjala, Super Kirby Clash, and a few others.

## 1.0.5048 - 2020-07-26
### Added
- Implemented the following calls of IAudioInManager: ListAudioIns, ListAudioInsAUuto, ListAudioInsAutoFiltered.
  - Fixes related missing service crashes on 15+ games including Crysis Remastered, Torchlight 2, multiple Borderlands games, Baldur's Gate and Baldur's Gate II, Ion Fury, and Bioshock 2 Remastered.

## 1.0.5047 - 2020-07-26
### Changed
- Polygon offset clamp value is now set by the game if the host supports it. Should not affect any games.

## 1.0.5044 - 2020-07-25
### Added
- Implemented BGRA texture support.
  - Fixes incorrect texture colors (often manifesting as blue skin/blue tint issues) on affected games including (but not limited to) Valkyria Chronicles 4, Resident Evil 4, Onimusha Warlords, and Memories Off ~ Innocent Fille ~.

## 1.0.5034 - 2020-07-25
### Fixed
- Incremented PPTC version, which was missed in 1.0.5021 (PR 1410).

## 1.0.5030 - 2020-07-25
### Changed
- GPU memory access related code cleanup.

## 1.0.5021 - 2020-07-24
### Changed
- Minor CPU emulation fixes and code cleanup. 

## 1.0.5011 - 2020-07-23
### Added
- Updated LibHac to version 0.11.3.
  - Removes the timeout when deleting files from the local file system.

## 1.0.5010 - 2020-07-23
### Changed
- Implemented GPFifo and made other changes and improvements to GPFifoClass and GPFifo semaphore implementation, respectively.
- Added a fast path for guest constant buffer updates.
  - Brings significant performance boosts to games such as Diablo III: Eternal Collection that make heavy use of the functionality. 

## 1.0.5004 - 2020-07-23
### Fixed
- Fixed full-screen toggle issues.

## 1.0.5003 - 2020-07-23
### Added
- Implemented GetIndirectLayerImageRequiredMemoryInfo call from vi service.
  - Fixes missing service crashes in Dark Souls Remastered and God Eater 3.

## 1.0.5000 - 2020-07-21
### Added
- Implemented/stubbed some calls to the am service.
  - Fixes crashes in Daemon X Machina, Jump Rope Challenge, Fate/EXTELLA LINK, FIFA 19/20, Super Kirby Clash, and several other games.

## 1.0.4987 - 2020-07-20
### Changed
- Improved the display and sort order of time zones in Options > Settings > System tab.

## 1.0.4985 - 2020-07-20
### Changed
- Reconfigured audio backend GUI to select OpenAL by default (if installed), and Dummy as the fallback.
   - Fixes the inability to save any emulator settings until the audio backend had been selected.

## 1.0.4981 - 2020-07-20
### Changed
- Fixed GL errors related to Point size, and implemented more Point parameters.
   - Fixes some graphics effects in Super Mario Odyssey.

## 1.0.4980 - 2020-07-20
### Fixed
- Fixed an issue with IDeliveryCacheProgressService GetEvent introduced in 1.0.4948 (PR 1362).
   - Fixes a regression in Super Smash Bros. Ultimate.

## 1.0.4969 - 2020-07-19
### Fixed
- Implemented a small part missing from 1.0.4964 (PR 1397), now ensuring that all HLE ipc sessions will be disposed when needed.

## 1.0.4964 - 2020-07-19
### Changed
- Improved implementation of CreateTransferMemory and CloseHandle syscalls.
- Fixed session services not being disposed.

## 1.0.4963 - 2020-07-19
### Added
- Added VBIC, VTST, and VSRA 32-bit instructions.
  - Fixes missing opcode crashes in several 32-bit games.

## 1.0.4957 - 2020-07-17
### Changed
- No longer prints guest stack trace for svcBreak debug calls.
  - Fixes issues on some games that call svcBreak with the debug flag set.

## 1.0.4952 - 2020-07-17
### Fixed
- Miscellaneous ARM32 instruction fixes.
  - Fixes black screen issue on some 32-bit games.

## 1.0.4948 - 2020-07-16
### Changed
- Updates the WaitSynchronization syscall implementation to match what the kernel does. Should not affect any games.

## 1.0.4947 - 2020-07-16
### Added
- Added Vadd and Vsub Wide CPU instructions.
 - Fixes missing opcode crashes in Duke Nukem 3D: 20th Anniversary World Tour and other games.

## 1.0.4946 - 2020-07-16
### Changed
- Improve kernel IPC related syscalls. Should not affect any games.

## 1.0.4945 - 2020-07-16
### Fixed
- Fixed resource limit reserve taking too long.
  - Fixes games that suffered from the following error on boot: HLE.HostThread.0 KernelSvc PrintResult: SetHeapSize64 returned error ResLimitExceeded.

## 1.0.4941 - 2020-07-15
### Fixed
- Force transform feedback rebind after buffer modifications
  - Fixes grass on XC2 and possibly resolves TFB issues in other games.

## 1.0.4938 - 2020-07-15
### Fixed
- Corrected a decode exception condition introduced in 1.0.4725 (PR 1298).

## 1.0.4937 - 2020-07-14
### Added
- Implemented partial transform feedback support.
  - Fixes grass in Xenoblade Chronicles: Definitive Edition and resolves some TFB-related issues in other games.

## 1.0.4936 - 2020-07-14
### Fixed
- Fixes a crash involving the loading of mods when launching unpacked games.

## 1.0.4921 - 2020-07-13
### Changed
- Modified depth stencil format copy compatibility check to check for equivalent color formats.
  - Fixes broken fog on Pokémon Sword/Shield and Super Mario Odyssey.

## 1.0.4920 - 2020-07-13
### Changed
- Miscellaneous CPU instruction additions and fixes.
  - Fixes issues in FATE/Extella and improves behavior in Snipperclips/Snipperclips Plus.

## 1.0.4918 - 2020-07-13
### Fixed
- Miscellaneous CPU instruction optimizations.
  - Fixes issues in Monster Hunter (all versions) and Ni No Kuni (needs other instructions)

## 1.0.4916 - 2020-07-13
### Fixed
- Resolved a JIT issue where operand assignments were not getting cleaned up.

## 1.0.4914 - 2020-07-13
### Fixed
- Corrected a bug introduced in 1.0.4891 (PR 1359).

## 1.0.4909 - 2020-07-12
### Changed
- Updated README.md.

## 1.0.4906 - 2020-07-11
### Added
- Implemented initial support for FMV/in-game videos (NVDEC).

## 1.0.4902 - 2020-07-10
### Fixed
- Corrected bug introduced in 1.0.4893 (PR 1358).

## 1.0.4893 - 2020-07-10
### Changed
- Miscellaneous CPU optimization.

## 1.0.4891 - 2020-07-10
### Changed
- Miscellaneous CPU optimization.

## 1.0.4889 - 2020-07-10
### Added
- Implemented logical operation registers & functionality.
  - Fixes interior lighting volumes in Xenoblade Chronicles 2 and Xenoblade Chronicles: Definitive Edition.

## 1.0.4880 - 2020-07-09
### Added
- Implemented modding support based on Atmosphere's implementation.

## 1.0.4876 - 2020-07-08
### Fixed
- Resolved issue where games were crashing on launch in Windows 7 after enabling PPTC.

## 1.0.4864 - 2020-07-06
### Added
- Added resolution scaling with GUI-configurable ratio.

## 1.0.4847 - 2020-07-04
### Changed
- Added support for command 10104 & 10105 and updated naming of the old variants to match Switchbrew.
  - Fixes crash in Animal Crossing: New Horizons 1.3.0

## 1.0.4829 - 2020-07-03
### Changed
- Reintroduced SoundIO as a fallback backend of OpenAL if OpenAL is not found.

## 1.0.4827 - 2020-07-03
### Changed
- Fixed compilation warnings.
- Utilize new LibHac APIs for executable loading.

## 1.0.4826 - 2020-07-03
### Added
- Partially implemented LEA shader instruction.
- Extended bindless texture elimination optimization.

## 1.0.4825 - 2020-07-03
### Fixed
- Fixed copy from a buffer to a 3D texture when used for linear to block linear conversion.
  - Improves rendering in Persona 5 Scramble and similar games.

## 1.0.4824 - 2020-07-03
### Changed
- Game list reloads now only occur if a configuration change to game directories has been made.

## 1.0.4823 - 2020-07-03
### Added
- GUI update: it is now possible to choose the audio backend. 
  - The audio backend is set to OpenAL by default.

## 1.0.4822 - 2020-07-03
### Added
- Implemented new GUI options for managing PPTC cache files on a per-game basis.

## 1.0.4821 - 2020-07-03
### Added
- GUI update: it is now possible to add multiple game directories at once.

## 1.0.4820 - 2020-07-03
### Added
- Stubbed nifm IRequest GetAppletInfo to be consistent with current GetResult implementation.
  - Fixes crash associated with unimplemented nifm IRequest on at least 31 games.

## 1.0.4819 - 2020-07-03
### Added
- Inline index buffer data is now supported.
  - Fixes black screen issues on NinNin Days and other OpenGL games.

## 1.0.4818 - 2020-07-03
### Fixed
- Fixed compute restore of previous shader state.
  - Improves lighting issues on a specific subset of games including Zelda: Link's Awakening.

## 1.0.4817 - 2020-07-03
### Fixed
- Fixed an issue with syncpoint implementation that could cause a deadlock in a specific scenario.

## 1.0.4816 - 2020-07-03
### Fixed
- Fixed an issue where bsd:u read was not writing in the IPC output buffer.

## 1.0.4796 - 2020-07-01
### Changed
- Removed dummy LLE project from Github

## 1.0.4767 - 2020-06-26
### Changed
- Games list GUI changes:
  - Sort-by method is now persistent across application restarts.
  - Removed unused SaveDataPath to speed up game list loading.
  - Vertical scrollbar position now resets to the top when titles finish loading.

## 1.0.4761 - 2020-06-23
### Added
- Added VPMIN, VPMAX, VMVN (register).
### Fixed
- VMVN (immediate) has been renamed from VMVN_I to VMVN_II to make way for the integer variant; it has also been fixed to use BitwiseNot rather than xor the input with itself.



## 1.0.4759 - 2020-06-23
### Changed
- Import DLC title key from ticket when loading into content manager.
  - Fixes crash due to DLC failing to decrypt.
- Removed profiled build task from Appveyor.
  

## 1.0.4753 - 2020-06-22
### Added
- Implemented DLC management window.
  - DLC is now fully supported and able to be managed through the GUI.

## 1.0.4752 - 2020-06-22
### Fixed
- Copy the value of InputConfig to a new array before iterating. 
  - Fixes a rare exception crash. 

## 1.0.4751 - 2020-06-22
### Fixed
- Fixed regression caused by wrong SB descriptor offset.
  - Fixes crashes introduced in PR 924 on Tokyo Mirage Sessions.

## 1.0.4750 - 2020-06-22
### Changed
- Update NRR related structures to match latest public research.

## 1.0.4740 - 2020-06-20
### Added
- Implemented aoc:u
  - Supports loading AddOnContent - GUI to be implemented separately.

## 1.0.4739 - 2020-06-20
### Changed
- Updated README.md file on Github.
  - No Ryujinx code changes.

## 1.0.4728 - 2020-06-18
### Fixed
- Incremented PTC version
  - Hotfix for previous build: fixed PTC cache not being rebuilt (this is required, as part of the JIT was changed).

## 1.0.4725 - 2020-06-17
### Changed
- Branch cleanup: added exit blocks to reduce redundant tail continues; 
  - May slightly improve performance for some games.
  
## 1.0.4709 - 2020-06-16
### Added
- New Feature Addition: Profiled Persistent Translation Cache 
  - Reduces game load times by up to 70% after cache has been generated: two consecutive launches to title screen or beyond, then improvements are realized permanently on third launch and any future launches.
  - This option must be enabled in Options > Settings > System > Enable Profiled Persistent Translation Cache.

## 1.0.4697 - 2020-06-14
### Fixed
- Fixed an issue where part of the VABS instruction would be parsed as an input register. 
  - Resolved a particular DEADLY PREMONITION Origins missing opcode error; the instruction was not missing but was instead parsed incorrectly. 

## 1.0.4696 - 2020-06-14
### Changed
- LayoutConverter has separate optimizations for LinearStrided and BlockLinear. MethodCopyBuffer now determines the range that will be affected, and uses a faster per pixel copy and offset calculation. 
  - This should increase performance on Nintendo Switch Online: NES and Super NES games, as well as mitigate dropped frames during large black screen (nvdec) videos.

## 1.0.4687 - 2020-06-09
### Changed
- Interacting with the console window no longer affects the emulation.
  - This can reduce cases where the game deadlocks or crashes because the console is in Select mode or is manually scrolled.

## 1.0.4683 - 2020-06-06
### Changed
- Stubbed ssl ISslContext: 4 (ImportServerPki) service.
  - Fixed missing service crashes on Minecraft Dungeons and Rocket League.

## 1.0.4682 - 2020-06-05
### Added
- Added Pclmulqdq intrinsic
  - Implemented crc32 in terms of pclmulqdq, which improves performance in some games when performing data integrity verification.