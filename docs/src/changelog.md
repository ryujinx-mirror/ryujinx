# Ryujinx Changelog
All updates to the Ryujinx official master build will be documented in this file.

For 1.0.x releases, see [here](https://github.com/ryujinx-mirror/ryujinx/wiki/Older-Changelog).

## 1.1.1403 - 2024-10-01
### Fixed:
- Update audio renderer to REV13: Add support for compressor statistics and volume reset.
  - Allows Trouble Magia to go in-game.

## 1.1.1402 - 2024-09-30
### Fixed:
- Do not try to create a texture pool if shader does not use textures.
  - Fixes CrossCode and Tales of Vesperia: Definitive Edition crashing on boot when loading a shader cache.

## 1.1.1401 - 2024-09-28
### Fixed:
- SDL: set app name.
  - Fixes application name showing as "SDL Application" instead of "Ryujinx" on some platforms in volume control menus.

## 1.1.1400 - 2024-09-26
### Fixed:
- Convert MaxTextureCacheCapacity to Dynamic MaxTextureCacheCapacity for High Resolution Mod support.
  - Allows usage of mods that make games run at higher resolutions than 4k, such as the 8k option in the Ultracam mod for The Legend of Zelda: Tears of the Kingdom (these mods require "Expand DRAM to 8GB" to be enabled).
  - Fixes crashes and low performance in certain games when using high resolution mods.

## 1.1.1399 - 2024-09-26
### Fixed:
- GPU: Ensure all clip distances are initialized when used.
  - Fixes half-screen rendering on AMD GPUs in The Legend of Zelda: Echoes of Wisdom.

## 1.1.1398 - 2024-09-22
### Fixed:
- Fix quads draws after DrawTexture on Vulkan.
  - Fixes rendering in FUZE4 when using Vulkan.

## 1.1.1397 - 2024-09-20
### Fixed:
- Shader: Assume the only remaining source is the right one when all others are undefined.
  - Fixes character model rendering in Romancing SaGa 2: Revenge of the Seven Demo.

## 1.1.1396 - 2024-09-19
### Fixed:
- Add support for sampler sRGB disable.
  - Fixes the image being too dark in Cassette Beasts intro logos, Charge Kid, OlliOlli: Switch Stance, Sphinx and the Cursed Mummy, and The Bard's Tale ARPG.

## 1.1.1395 - 2024-09-18
### Fixed:
- Replace passing by IMemoryOwner<byte> with passing by concrete MemoryOwner<byte>.
  - Code cleanup. No expected user-facing changes.

## 1.1.1394 - 2024-09-18
### Fixed:
- Implement support for shader ATOM.EXCH instruction.
  - Fixes missing reflections in Lollipop Chainsaw RePOP.

## 1.1.1393 - 2024-09-17
### Changed:
- Revert "Ava: Wait for CheckLaunchState() to complete to handle exceptions properly".
  - Reverts the change in 1.1.1391 as it broke the UI.

## 1.1.1392 - 2024-09-17
### Fixed:
- Change image format view handling to allow view incompatible formats.
  - Fixes Lollipop Chainsaw RePOP crashing with a device loss error.

## 1.1.1391 - 2024-09-17
### Fixed:
- Ava: Wait for CheckLaunchState() to complete to handle exceptions properly.
  - Fixes an issue where the UI would freeze instead of handling an exception when launching a game directly from command line or from a shortcut.


## 1.1.1390 - 2024-09-17
### Fixed:
- Add area sampling scaler to allow for super-sampled anti-aliasing.
  - Under graphics settings > scaling filter, adds a new option for area scaling, which will function as SSAA when the rendering resolution is 2x (or more) higher than the screen resolution.

## 1.1.1389 - 2024-09-17
### Changed:
- Change 6GB DRAM expansion to 8GB.
  - Changes the DRAM expansion hack to use 8GB instead of the 6GB it previously used, and renames the setting back to what it was.
  - Pokémon Legends Arceus and Pokémon Scarlet/Violet will no longer crash with the DRAM expansion enabled, allowing mods for these games that required this option to be used again.

## 1.1.1388 - 2024-09-15
### Fixed:
- Implement fast DMA texture to texture copy.
  - Massively improves performance (around 2x as much) in Castlevania Dominus Collection.

## 1.1.1387 - 2024-09-15
### Fixed:
- Make GetFunctionPointerForDelegate as explicit as possible.
  - Required for AOT support in the future. No expected user-facing changes.

## 1.1.1386 - 2024-09-12
### Fixed:
- Implement Arm32 VSHLL and QADD16 instructions.
  - Allows "The Legend of Sword and Fairy" to go in-game.

## 1.1.1385 - 2024-09-01
### Fixed:
- Vulkan: Feedback loop detection and barriers.
  - On RDNA3 (RX 7000 series) AMD graphics cards, fixes purple lines seen across geometry in The Legend of Zelda: Breath of the Wild, Tears of the Kingdom, and likely other games that had graphics issues specific to these GPUs.
  - On Nvidia RTX 2000/3000/4000 GPUs, fixes blocky water artifacts in Mario Golf: Super Rush, Splatoon 3, and alleviates (but does not fix) the issue in Team Sonic Racing (v1.0.0).
    - On Nvidia RTX 3000 GPUs, fixes flickering and missing body parts in Kirby's Dream Buffet, and fixes flickering in Mario + Rabbids: Sparks of Hope. 

## 1.1.1384 - 2024-09-01
### Fixed:
- Fix incorrect depth texture 3D flag.
  - Fixes a crash in Neverwinter Nights: Enhanced Edition.

## 1.1.1383 - 2024-09-01
### Fixed:
- Vulkan: Update Silk.NET to 2.21.
  - Updates Silk.NET dependencies and Vulkan extensions. No expected user-facing changes.

## 1.1.1382 - 2024-08-31
### Fixed:
- Make HLE project AOT friendly.
  - No expected user-facing changes.

## 1.1.1381 - 2024-08-31
### Fixed:
- Replace ImageSharp with SkiaSharp everywhere.
  - Fixes text inputs in games not allowing more than one character to be typed.

## 1.1.1380 - 2024-08-27
### Fixed:
- Fix deadlock in background translation thread shutdown.
  - Fixes an issue where Ryujinx would sometimes freeze and stop responding when trying to stop the emulation.

## 1.1.1379 - 2024-08-21
### Fixed:
- nuget: bump DynamicData from 9.0.1 to 9.0.4.
  - Updates the DynamicData dependency. No expected user-facing changes.

## 1.1.1378 - 2024-08-20
### Fixed:
- Fix NRE when using image array from constant buffer.
  - Fixes a crash in World of Goo 2, though the game still does not work due to another issue.

## 1.1.1377 - 2024-08-20
### Fixed:
- nuget: bump ImageSharp from 2.1.8 to 2.1.9.
  - Updates the SixLabors.ImageSharp dependency. No expected user-facing changes.

## 1.1.1376 - 2024-08-17
### Fixed:
- nim:eca : Stub CreateServerInterface2.
  - Allows THE NEW DENPA MEN to go in-game without "Ignore Missing Services".

## 1.1.1375 - 2024-08-13
### Fixed:
- Fix arbitrary game ordering when sorting by Favorites.
  - Favorite games will now be sorted alphabetically on the games list.

## 1.1.1374 - 2024-08-12
### Fixed:
- Clamp amount of mipmap levels to max allowed for all backends.
  - On AMD graphics cards using Vulkan, fixes crashes when running certain mods, such as character swaps in Mario Kart 8 Deluxe.

## 1.1.1373 - 2024-08-08
### Added:
- Implement UQADD16, UQADD8, UQSUB16, UQSUB8, VQRDMULH, VSLI and VSWP Arm32 instructions.
  - Fixes DarkStar One crashing on non-ARM systems and shaky screen on ARM systems.
  - May allow 12 Labours of Hercules II: The Cretan Bull to go in-game.

## 1.1.1372 - 2024-08-05
### Fixed:
- Replace and remove obsolete ByteMemoryPool type.
  - Code cleanup. No expected user-facing changes.

## 1.1.1371 - 2024-08-05
### Fixed:
- Fix same textures with unmapped start being considered different.
  - Fixes The Legend of Heroes: Kuro no Kiseki II crashing shortly after gameplay starts.

## 1.1.1370 - 2024-08-04
### Fixed:
- Fix LocaleExtension SetRawSource usages + language perf improvement.
  - Fixes a small UI freeze when changing the UI language.

## 1.1.1369 - 2024-08-04
### Fixed:
- Infra: Update Microsoft.IdentityModel.JsonWebTokens.
  - Updates the Microsoft.IdentityModel.JsonWebTokens dependency. No expected user-facing changes.

## 1.1.1368 - 2024-08-03
### Fixed:
- Avoid race conditions while launching games directly from the command line.
  - Fixes games not booting when using shortcuts or launch arguments.

## 1.1.1367 - 2024-08-03
### Fixed:
- nuget: bump DynamicData from 8.4.1 to 9.0.1.
  - Updates the DynamicData dependency. No expected user-facing changes.

## 1.1.1366 - 2024-08-03
### Fixed:
- replace ByteMemoryPool usage in Ryujinx.Graphics.
  - Code cleanup. No expected user-facing changes.

## 1.1.1365 - 2024-08-03
### Fixed:
- Fix FileNotFoundException in TryGetApplicationsFromFile() and improve loading applications.
  - Fixes crashes when trying to load files from bad symlinks, non-existent files or hidden subdirectories.

## 1.1.1364 - 2024-07-31
### Fixed:
- Fix off-by-one on audio renderer PerformanceManager.GetNextEntry.
  - Fixes Kuro no Kiseki II crashing on startup.

## 1.1.1363 - 2024-07-30
### Fixed:
- Fix shader RegisterUsage pass only taking first operation dest into account.
  - Fixes red tint on THE NEW DENPA MEN. (Note that the game still won't run without "Ignore Missing Services".)

## 1.1.1362 - 2024-07-30
### Fixed:
- Vulkan: Force topology to PatchList for Tessellation.
  - On AMD graphics cards, fixes a crash on floor 15 of Luigi's Mansion 3.

## 1.1.1361 - 2024-07-25
### Fixed:
- Ava UI: Handle updates containing non numeric characters.
  - Fixes an issue where the title update manager would fail to display updates if they contained letters (for example, v1.0.5A).

## 1.1.1360 - 2024-07-25
### Fixed:
- Vulkan: Add missing barriers for texture to buffer copy.
  - Fixes a regression from 1.1.1352 exclusively affecting AMD graphics cards, which caused the water in The Legend of Zelda: Breath of the Wild to turn white.

## 1.1.1359 - 2024-07-22
### Fixed:
- Update kernel GetInfo SVC for firmware 18.0.0.
  - Allows Nintendo 64 NSO, or anything using the JIT service, to work with firmware 18.0.0+.

## 1.1.1358 - 2024-07-21
### Fixed:
- Fix checking for the wrong metadata files for applications launched with a different program index.
  - Fixes a regression from 1.1.1350 that caused updates to not apply for games that get launched with different program indices, such as Super Mario 3D All-Stars.
  - Fixes an issue where the emulator wouldn't apply DLC to these same games.

## 1.1.1357 - 2024-07-20
### Fixed:
- Make sure TryGetApplicationsFromFile() doesn't throw exceptions anymore.
  - Fixes remaining instances of crashing on loading invalid game files onto the games list since 1.1.1350.

## 1.1.1356 - 2024-07-20
### Fixed:
- Fix Skia saving screenshot with transparent background and incorrect origin.
  - Fixes a regression from 1.1.1346 causing emulator screenshots to save as blank image files.

## 1.1.1355 - 2024-07-20
### Fixed:
- Unlink server sessions from multi-wait when service stops processing requests.
  - Fixes an assert on debug builds when emulation is stopped. No expected user-facing changes.

## 1.1.1354 - 2024-07-19
### Fixed:
- Ava UI: Auto select newly added updates & DLC.
  - Updates and DLC will now be automatically enabled on the update and DLC managers when the files are first added to these menus.

## 1.1.1353 - 2024-07-18
### Fixed:
- Add missing Buffer attribute on NGC Check method.
  - Fixes a crash in Teenage Mutant Ninja Turtles: Splintered Fate, and other games that do profanity filter checks and target firmware 16.0.0+.

## 1.1.1352 - 2024-07-17
### Fixed:
- Vulkan: Defer guest barriers, and improve image barrier timings.
  - On Nvidia Ampere (and probably newer GPUs), fixes flickering artifacts in Cotton Guardian Force Saturn Tribute collection.
  - On Snapdragon X Elite Adreno GPU, fixes rendering issues in several games, including Super Mario Odyssey.

## 1.1.1351 - 2024-07-17
### Fixed:
- Catch exceptions when loading applications from invalid NSPs.
  - Fixes a regression from 1.1.1350 that caused the emulator to crash if an invalid game was loaded onto the game list.

## 1.1.1350 - 2024-07-16
### Added:
- Add support for multi game XCIs (second try).
  - Implements loader support for XCI packages that contain multiple title entries.
  - Fixes recognition of XCI files that contain title updates alongside the base game.

## 1.1.1349 - 2024-07-16
### Changed:
- Remove CommandBufferScoped Dependencies.
  - Code cleanup. No expected user-facing changes.

## 1.1.1348 - 2024-07-15
### Fixed:
- misc: Re-order and manually update DriverID to name.
  - NVK will now be properly displayed as the GPU driver on the status bar.

## 1.1.1347 - 2024-07-15
### Fixed:
- replace ByteMemoryPool usage in Ryujinx.HLE.
  - Code cleanup. No expected user-facing changes.

## 1.1.1346 - 2024-07-14
### Fixed:
- Use SkiaSharp for Avalonia in place of ImageSharp.
  - Updates the Avalonia UI project to use SkiaSharp for image processing, replacing the previously used SixLabors ImageSharp library. No expected user-facing changes.

## 1.1.1345 - 2024-07-10
### Fixed:
- Use draw clear on Adreno, instead of vkCmdClearAttachments.
  - Works around an Adreno driver bug causing a race condition when calling vkCmdClearAttachments.
  - Fixes incorrect Cascade Kingdom waterfall rendering and shadow flickering in Super Mario Odyssey.
  - Fixes Astral Chain freezing on boot.

## 1.1.1344 - 2024-07-10
### Fixed:
- Force dynamic state update after rasterizer discard disable on Adreno.
  - On Adreno drivers, significantly improves rendering in Xenoblade Chronicles 2.

## 1.1.1343 - 2024-07-07
### Fixed:
- Disallow concurrent fence waits on Adreno.
  - Works around an Adreno driver bug when waiting on a fence from multiple threads.
  - Fixes crashes in a variety of titles including The Legend of Zelda: Tears of the Kingdom at boot.

## 1.1.1342 - 2024-07-07
### Fixed:
- Disable descriptor set template updates for buffer textures on Adreno.
  - Works around an Adreno driver bug causing crashes in UE4 games (& others) such as Mario + Rabbids: Sparks of Hope.

## 1.1.1341 - 2024-07-07
### Fixed:
- Force Vulkan swapchain re-creation when window size changes.
  - Fixes an issue on Adreno GPUs where the renderer would not match the size of the window after a re-size.
  - No other vendors should be impacted.

## 1.1.1340 - 2024-06-26
### Fixed:
- Resolve some Vulkan validation errors.
  - No known changes in games.

## 1.1.1339 - 2024-06-26
### Fixed:
- discord: Fix TruncateToByteLength() not taking the string length into account before trimming.
  - Fixes a regression from 1.1.1303 that caused Yu-Gi-Oh! RUSH DUEL: Saikyo Battle Royale to crash on boot.

## 1.1.1338 - 2024-06-26
### Fixed:
- nuget: bump Microsoft.IdentityModel.JsonWebTokens from 7.6.0 to 7.6.2.
  - Updates the Microsoft.IdentityModel.JsonWebTokens dependency. No expected user-facing changes.

## 1.1.1337 - 2024-06-25
### Fixed:
- SetProcessMemoryPermission address and size are always 64-bit.
  - No expected user-facing changes.

## 1.1.1336 - 2024-06-19
### Fixed:
- JIT: Coalesce copies on LSRA with simple register preferencing.
  - Slightly reduces the size of code generated by the emulator, though performance change will likely not be noticeable.

## 1.1.1335 - 2024-06-19
### Fixed:
- JIT: Ensure entry block has no predecessors on RegisterUsage pass.
  - Code cleanup. May be required for future features. No expected user-facing changes.

## 1.1.1334 - 2024-06-16
### Fixed:
- Clear pooled memory on return when used to hold object references.
  - Code cleanup. No expected user-facing changes.

## 1.1.1333 - 2024-06-16
### Fixed:
- Extend bindless elimination to catch a few more specific cases.
  - Fixes smoke effects in Hogwarts Legacy and It Takes Two. May possibly fix particle effects in Tales of Kenzera: ZAU and other UE games.
  - Fixes vertex explosions on ice effects in Mortal Kombat 1.
  - Fixes log warnings in Shin Megami Tensei V: Vengeance.

## 1.1.1332 - 2024-06-16
### Fixed:
- Replace ByteMemoryPool in Audio projects.
  - Code cleanup. No expected user-facing changes.

## 1.1.1331 - 2024-06-16
### Fixed:
- nuget: bump Microsoft.IO.RecyclableMemoryStream from 3.0.0 to 3.0.1.
  - Updates the Microsoft.IO.RecyclableMemoryStream dependency. No expected changes in games.

## 1.1.1330 - 2024-06-02
### Fixed:
- Vulkan separate descriptor set fixes.
  - Fixes invisible characters on Intel GPUs in Paper Mario: The Thousand Year Door. 

## 1.1.1329 - 2024-06-02
### Fixed:
- GPU: Remove unused dynamic state and pipeline settings.
  - Code cleanup. No expected user-facing changes. 

## 1.1.1328 - 2024-06-02
### Fixed:
- New pooled memory types.
  - Reduces memory allocations done by the emulator. Likely no noticeable changes during normal gameplay.

## 1.1.1327 - 2024-06-02
### Fixed:
- Avoid inexact read with 'Stream.Read'.
  - Code cleanup. No expected user-facing changes.

## 1.1.1326 - 2024-06-02
### Fixed:
- nuget: bump Microsoft.IdentityModel.JsonWebTokens from 7.5.2 to 7.6.0.
  - Updates the Microsoft.IdentityModel.JsonWebTokens dependency. No expected user-facing changes.

## 1.1.1325 - 2024-05-26
### Fixed:
- Vulkan: Extend full bindless to cover cases with phi nodes.
  - Resolves black or missing textures and animations in Paper Mario: The Thousand Year Door such as save boxes, coins and boat transitions.
  - Fixes the missing floor textures in No Man's Sky.

*Note that there will be some "new" bugs on some affected surfaces on AMD GPUs that were not visible prior to this change.

## 1.1.1324 - 2024-05-26
### Changed:
- Change disk shader cache compression algorithm to Brotli (RFC 7932)
  - Improves the speed of the "Packaging shaders" stage of a disk cache rebuild by up to 40%.

## 1.1.1323 - 2024-05-26
### Fixed:
- Allow texture arrays to use separate descriptor sets on Vulkan.
  - Fixes a performance regression (mostly on macOS) caused by 1.1.1291 in games that use bindless textures from storage buffers.
  - Example titles are Mario Party Superstars and Penny's Big Breakaway.

## 1.1.1322 - 2024-05-24
### Fixed:
- nuget: bump Microsoft.IdentityModel.JsonWebTokens from 7.5.1 to 7.5.2.
  - Updates the Microsoft.IdentityModel.JsonWebTokens dependency. No expected user-facing changes.

## 1.1.1321 - 2024-05-23
### Fixed:
- Workaround bug on logic op with float framebuffer.
  - On Intel Vulkan, fixes the black screen in specific areas in Paper Mario: The Thousand-Year Door. Note that the game will still crash on Windows on Intel GPUs, so this improvement is only visible on Linux.

## 1.1.1320 - 2024-05-23
### Fixed:
- Workaround AMD bug on logic op with float framebuffer.
  - On AMD Vulkan, fixes the black screen in specific areas in Paper Mario: The Thousand-Year Door.

## 1.1.1319 - 2024-05-22
### Changed:
- Kernel: Wake cores from idle directly rather than through a host thread.
  - Slightly improves performance in titles with inefficient threading implementations.
  - Improves performance in Pokémon Legends Arceus on low core count devices like the Steam Deck by up to 20%, or reduces power consumption by up to 40% at equal performance.

## 1.1.1318 - 2024-05-20
### Fixed:
- Updating Concentus dependency to speed up Opus decoding.
  - May slightly reduce CPU usage in games that use Opus, such as Pokémon Legends Arceus.

## 1.1.1317 - 2024-05-19
### Fixed:
- GPU: Migrate buffers on GPU project, pre-emptively flush device local mappings.
  - Improves performance on systems with dedicated GPUs in Catherine Full Body, Hyrule Warriors: Age of Calamity v1.0.0, Pokémon Scarlet/Violet, The Legend of Zelda: Breath of the Wild and Tears of the Kingdom.
  - Fixes character shadows being too dark on the equip screen in Splatoon 3.

## 1.1.1316 - 2024-05-17
### Fixed:
- HID: Fix another NullReferenceException when unblocking input updates.
  - Fixes another instance of crashing after using the software keyboard, caused by 1.1.1315.

## 1.1.1315 - 2024-05-17
### Fixed:
- Disable keyboard controller input while swkbd is open (foreground) (second attempt).
  - Redo of 1.1.1307. Should also fix the crashing caused by the original change.

## 1.1.1314 - 2024-05-17
### Fixed:
- Update audio renderer to REV12: Add support for splitter biquad filter.
  - Allows Animal Well to run.
  - Fixes Charon's voice in Spiritfarer.

## 1.1.1313 - 2024-05-16
### Fixed:
- misc: Change Deflate compression level to Fastest.
  - Speeds up shader packaging process by up to 14x. Shader packaging occurs after GPU driver updates, switching between Vulkan and OpenGL, or significant changes to Ryujinx's GPU code. Note that shader caches will have slightly larger file sizes after this change.

## 1.1.1312 - 2024-05-16
### Fixed:
- Improves some log messages and fixes a typo.
  - Makes the logging messages for missing game directories and files clearer.

## 1.1.1311 - 2024-05-15
### Fixed:
- Revert "Disable keyboard controller input while swkbd is open (foreground)".
  - Reverts the change in 1.1.1307 due to it causing crashes in some games which use the software keyboard.

## 1.1.1310 - 2024-05-14
### Fixed:
- New Crowdin updates.
  - Updates Avalonia GUI localizations with the latest changes from Crowdin.

## 1.1.1309 - 2024-05-14
### Fixed:
- Bump Avalonia.Svg.
  - Updates the Avalonia.Svg dependency. No expected user-facing changes.

## 1.1.1308 - 2024-05-14
### Fixed:
- Add missing lock on texture cache UpdateMapping method.
  - Fixes a crash in Harmony: The Fall of Reverie upon entering the Naiads.

## 1.1.1307 - 2024-05-14
### Fixed:
- Disable keyboard controller input while swkbd is open (foreground).
  - Fixes an issue where playing Stardew Valley with a keyboard would cause the software keyboard prompt not to close.

## 1.1.1306 - 2024-05-14
### Fixed:
- Make TextureGroup.ClearModified thread safe.
  - Fixes crashes in Europa (Demo).

## 1.1.1305 - 2024-05-14
### Added:
- Add the "Auto" theme option in setting.
  - Adds an option for Avalonia to follow OS theme (light or dark).

## 1.1.1304 - 2024-05-14
### Fixed:
- Add support for bindless textures from storage buffer on Vulkan.
  - Fixes rendering in Castle Crashers Remastered.
  - Fixes missing shadows in certain minigames in Mario Party Superstars.

## 1.1.1303 - 2024-05-14
### Fixed:
- discordRPC: Truncate game title and details if they exceed Discord byte limit.
  - Fixes an issue where Discord RPC caused Ryujinx to crash if a game's title was longer than 128 characters.

## 1.1.1302 - 2024-05-14
### Fixed:
- HID: Stub IHidServer: 134 (SetNpadAnalogStickUseCenterClamp).
  - Allows eBaseball Powerful Pro Yakyuu 2020, Pawapoke R,  WBSC eBASEBALL: Power Pros, and possibly other "Power Pro" games to boot without "Ignore Missing Services".

## 1.1.1301 - 2024-05-14
### Fixed:
- Update outdated Windows version warning.
  - Updates the warning message displayed when an unsupported Windows version is detected.

## 1.1.1300 - 2024-05-14
### Fixed:
- Add Linux-specific files to local builds.
  - Ensures Linux-specific files are copied to the output directory when building locally. Useful for testing certain changes.

## 1.1.1299 - 2024-05-14
### Fixed:
- infra: Update ReSharper's DotSettings.
  - Code cleanup. No expected user-facing changes.

## 1.1.1298 - 2024-05-08
### Fixed:
- Replace "List.ForEach" for "foreach".
  - Code cleanup. No expected user-facing changes.

## 1.1.1297 - 2024-05-02
### Fixed:
- Fix system dateTime loading in Avalonia LoadCurrentConfiguration.
  - Fixes an issue where trying to change the time to an older date in the Avalonia UI caused the emulator to crash.

## 1.1.1296 - 2024-05-01
### Fixed:
- UI: Fix some MainWindow bugs and implement menubar items to change window size.
  - You can now set the emulator window size to 720p or 1080p from View > Window Size.
  - Window dimensions will no longer be saved when exiting from a maximized state, which caused the size to be reset every time.
  - Fixes an issue where the window startup location would reset to the middle of the screen.

## 1.1.1295 - 2024-04-29
### Fixed:
- Fix Alt key appearing as Control in settings menus.
  - Fixes an issue where the "Alt" key would display on the UI as "Control" when bound.

## 1.1.1294 - 2024-04-28
### Fixed:
- Fix cursor states on Windows.
  - Prevents the cursor from disappearing during the emulator's game loading screen.
  - Fixes an issue wherein the emulator window could not be resized due to the cursor flickering.
  - Fixes an issue which caused the cursor to disappear over submenus while cursor was set to always hide. 
  - Fixes an issue where the check for whether the cursor was within the active window did not take into account the windows position, leading to situations where it would hide where it shouldn't.

## 1.1.1293 - 2024-04-28
### Fixed:
- Fix direct keyboard not working when using a controller.
  - Allows the Ultracam benchmark tool for The Legend of Zelda: Tears of the Kingdom to be used without setting a keyboard as the controller. 
  - Allows Deltarune, Undertale, SpongeBob SquarePants: The Cosmic Shake, and likely other games to be played with direct keyboard controls.

## 1.1.1292 - 2024-04-28
### Fixed:
- HID: Correct direct mouse deltas.
  - Fixes mouse aiming in Quake, SpongeBob SquarePants: The Cosmic Shake, and likely the few other games that support mouse controls on the Switch. 

## 1.1.1291 - 2024-04-22
### Fixed:
- Add support for bindless textures from shader input (vertex buffer) on Vulkan.
  - On Vulkan, fixes the following:
  - Fixes rendering in mofumofusensen.
  - Fixes missing graphics in PAC-MAN 99, TETRIS 99 and Super Mario Bros. 35.
  - Fixes missing backgrounds in even if TEMPEST, Enchanted in the Moonlight, My Last First Kiss, Irresistible Mistakes, Diabolik Lovers games and likely other visual novels from Voltage.
  - Fixes missing coins in WarioWare: Get It Together.
  - Fixes missing player indicators and radars in FIFA games.

## 1.1.1290 - 2024-04-21
### Fixed:
- Implement MemoryManagerHostTracked.GetReadOnlySequence().
  - Fixes a regression from 1.1.1289 that caused games on macOS to crash on boot. 

## 1.1.1289 - 2024-04-21
### Changed:
- Use pooled memory and avoid memory copies.
  - Code cleanup. No expected user-facing changes.

## 1.1.1288 - 2024-04-21
### Fixed:
- End render target lifetime on syncpoint increment.
  - Fixes Balatro crashing on boot.
  - Fixes a regression in Pizza Tower causing a random crash on boot.

## 1.1.1287 - 2024-04-19
### Fixed:
- chore: remove repetitive words.
  - Code cleanup. No expected user-facing changes. 

## 1.1.1286 - 2024-04-19
### Fixed:
- Do not compare Span<T> to 'null' or 'default'.
  - Code cleanup. No expected user-facing changes. 

## 1.1.1285 - 2024-04-19
### Fixed:
- Update to new standard for volatility operations.
  - Code cleanup. No expected user-facing changes. 

## 1.1.1284 - 2024-04-18
### Fixed:
- Fix unmapped address check when reading texture handles.
  - Fixes a regression likely from 1.1.1098 that caused Sniper Elite 3 to crash on launch.

## 1.1.1283 - 2024-04-18
### Fixed:
- Update "SixLabors.ImageSharp" to fix vulnerabilities.
  - Updates the SixLabors.ImageSharp dependency. No expected user-facing changes.

## 1.1.1282 - 2024-04-17
### Fixed:
- Ava UI: Input Menu Refactor.
  - Refactors the input menu code.
  - Platform-specific keys (for instance, the Windows key) will now display properly when a button is bound to them.
  - Allows keys to be localized.

## 1.1.1281 - 2024-04-15
### Fixed:
- Fix crash when changing controller config.
  - Fixes a crash that occurred when switching from an input device without motion (i.e. a keyboard) to a controller with motion support while a game is running. 

## 1.1.1280 - 2024-04-14
### Fixed:
- Texture loading: reduce memory allocations.
  - Code cleanup. No expected user-facing changes. 

## 1.1.1279 - 2024-04-11
### Fixed:
- Account for swapchain image count change after re-creation.
  - Fixes a crash on AMD proprietary drivers on Linux when VSync is toggled. 

## 1.1.1278 - 2024-04-11
### Fixed:
- Allow BSD sockets Poll to exit when emulation ends.
  - Fixes a freeze when trying to stop emulation and/or close the emulator on Penny's Big Breakaway, and possibly on other games that use sockets with Poll.

## 1.1.1277 - 2024-04-10
### Fixed:
- Revert "Update StoreConstantToMemory to match StoreConstantToAddress on value read".
  - Reverts the previous change. The specified cheats were invalid and should not be loaded at all.

## 1.1.1276 - 2024-04-10
### Fixed:
- Update StoreConstantToMemory to match StoreConstantToAddress on value read.
  - Fixes some cheats with instructions starting with 6XXXXXXX failing to load, specifically when the cheat has bit width equal to 1, 2 or 4, and only one 32-bit value.

## 1.1.1275 - 2024-04-10
### Fixed:
- Ava UI: Prevent Status Bar Backend Update.
  - Fixes an issue where the GPU displayed on the status bar would change if the graphics backend setting was changed while a game was running.

## 1.1.1274 - 2024-04-10
### Fixed:
- nuget: bump Microsoft.IdentityModel.JsonWebTokens from 7.4.0 to 7.5.1.
  - Updates the Microsoft.IdentityModel.JsonWebTokens dependency. No expected user-facing changes.

## 1.1.1273 - 2024-04-10
### Fixed:
- Fix input consumed by audio renderer SplitterState.Update.
  - Fixes a regression from 1.1.1265 that caused crashing in Resident Evil after cinematics. Possibly affected other games.

## 1.1.1272 - 2024-04-09
### Fixed:
- CPU: Produce non-inf results for RSQRTE instruction with subnormal inputs.
  - Fixes terrain randomly disappearing in Penny's Big Breakaway.

## 1.1.1271 - 2024-04-09
### Fixed:
- Use ResScaleUnsupported flag for texture arrays.
  - Fixes rendering glitches in Penny's Big Breakaway when using resolution scale, however the game will no longer scale.

## 1.1.1270 - 2024-04-09
### Fixed:
- Fast D32S8 2D depth texture copy.
  - Improves performance in Penny's Big Breakaway by up to 1500%.

## 1.1.1269 - 2024-04-08
### Fixed:
- Pin audio renderer update output buffers.
  - Fixes a regression from 1.1.1265 that caused crashes in several games.

## 1.1.1268 - 2024-04-08
### Fixed:
- gui: Disable CLI setting persistence for HW-accelerated GUI.
  - CLI argument to enable UI software rendering no longer persists in config state.

## 1.1.1267 - 2024-04-07
### Added:
- Add support for large sampler arrays on Vulkan.
  - Fixes black textures present in most of Hogwarts Legacy.
  - Fixes most graphical rendering in Penny's Big Breakaway.
  - Fixes grass and other particle effects appearing blocky in The Legend of Zelda: Skyward Sword HD.

## 1.1.1266 - 2024-04-07
### Fixed:
- Fix PC alignment for ADR thumb instruction.
  - Ni no Kuni Wrath of the White Witch will no longer render a black background when the 1.0.2 update applied.

## 1.1.1265 - 2024-04-07
### Changed:
- Audio rendering: reduce memory allocations.
  - Code cleanup. No expected user-facing changes.

## 1.1.1264 - 2024-04-07
### Changed:
- Enhance Error Handling with Try-Pattern Refactoring.
  - Code cleanup. No expected user-facing changes.

## 1.1.1263 - 2024-04-07
### Changed:
- Replacing the try-catch block with null-conditional and null-coalescing operators.
  - Code cleanup. No expected user-facing changes.

## 1.1.1262 - 2024-04-06
### Added:
- misc: Add ANGLE configuration option to JSON and CLI.
  - Adds command line arguments to change how the UI will be rendered
--software-gui = Avalonia will use software rendering.
--hardware-gui = Avalonia will use ANGLE/GLX rendering.
  - Should help with using Renderdoc to debug graphics issues. No user-facing changes.

## 1.1.1261 - 2024-04-06
### Fixed:
- Delete old 16KB page workarounds.
  - Deletes unused code. No user-facing changes.

## 1.1.1260 - 2024-04-06
### Fixed:
- Vulkan: Fix swapchain image view leak.
  - Fixes two regressions from 1.1.1154, though it's unknown what games might have been visibly affected.

## 1.1.1259 - 2024-04-06
### Fixed:
- Vulkan: Skip draws for patches topology without a tessellation shader.
  - On AMD graphics cards, fixes a crash in Luigi's Mansion 3 on the sand level.

## 1.1.1258 - 2024-04-06
### Fixed:
- nuget: bump DynamicData from 8.3.27 to 8.4.1.
  - Updates the DynamicData dependency. No expected user-facing changes.

## 1.1.1257 - 2024-04-06
### Added:
- Add mod enablement status in the log message.
  - Displays what mods are enabled in the logs and logging console. Intended to help with troubleshooting.

## 1.1.1256 - 2024-04-06
### Changed:
- Remove Unnecessary Category from Docs ReadME.

## 1.1.1255 - 2024-04-06
### Fixed:
- "Task.Wait()" synchronously blocks, use "await" instead.
  - Code cleanup. No expected user-facing changes.

## 1.1.1254 - 2024-04-05
### Fixed:
- ts: Migrate service to Horizon project.
  - Allows nx-hbmenu to boot.

## 1.1.1253 - 2024-04-05
### Fixed:
- Ignore diacritics on game search.
  - When searching on the games list, allows "pokemon" to display Pokémon games, for instance.

## 1.1.1252 - 2024-04-05
### Fixed:
- Add missing ModWindowTitle locale key.
  - Fixes the title for the mod manager window.

## 1.1.1251 - 2024-04-05
### Added:
- Add support to IVirtualMemoryManager for zero-copy reads.
  - Code cleanup. No expected user-facing changes.

## 1.1.1250 - 2024-04-03
### Fixed:
- Stop clearing Modified flag on DiscardData.
  - Fixes a regression from 1.1.1024 which sank character models into the ground in Easy Come Easy Golf.

## 1.1.1249 - 2024-04-02
### Fixed:
- New Crowdin updates.
  - Updates Avalonia GUI localizations with the latest changes from Crowdin.

## 1.1.1248 - 2024-03-27
### Changed:
- UI: Friendly driver name reporting.
  - Makes graphics driver names on the bottom status bar easier to read.

## 1.1.1247 - 2024-03-26
### Added:
- Implement host tracked memory manager mode.
  - Changes host memory manager modes on ARM to a better tailored version for ARM systems with 16KB page sizes.
  - On macOS, fixes:
    - Vertex explosions in Shin Megami Tensei V.
    - MKTV thumbnails in Mario Kart 8 Deluxe.
    - Album photos not displaying correctly in The Legend of Zelda: Breath of the Wild.
    - Random crashes in Pokémon Legends Arceus with hypervisor disabled.
    - Crashes on boot with hypervisor disabled in Master Detective Archives: Rain Code, Super Mario Bros. Wonder and The Legend of Zelda: Tears of the Kingdom.
    - Improves performance in games when hypervisor is disabled, most notably in Mario Kart 8 Deluxe (32-bit game, can't use hypervisor) and Super Mario Odyssey. This also means that games which would softlock, freeze or crash (such as Pokémon games) will be a lot more playable with hypervisor disabled.

## 1.1.1246 - 2024-03-26
### Fixed:
- Vulkan: Recreate swapchain correctly when toggling VSync.
  - Fixes an issue where, under certain conditions, toggling VSync via hotkey while in-game would not uncap the framerate beyond the monitor's refresh rate.

## 1.1.1245 - 2024-03-26
### Fixed:
- Disable push descriptors for Intel ARC GPUs on Windows.
  - Fixes Intel Arc graphics cards crashing on several games since 1.1.1198.

## 1.1.1244 - 2024-03-23
### Fixed:
- New gamecard icons.
  - Changes gamecard icons displayed on the games list for applications without icons.

## 1.1.1243 - 2024-03-23
### Fixed:
- Add a few missing locale strings on Avalonia.
  - Makes more UI elements localizable.

## 1.1.1242 - 2024-03-21
### Fixed:
- Updates the default value for BufferedQuery.
  - Fixes RDNA3 graphics cards (RX 7000 series) freezing on some UE4 games, such as Shin Megami Tensei V.

## 1.1.1241 - 2024-03-21
### Fixed:
- [UI] Fix Display Name Translations & Update some Chinese Translations.

## 1.1.1240 - 2024-03-20
### Fixed:
- New Crowdin updates.
  - Updates the Avalonia UI translations and adds Arabic and Thai languages.

## 1.1.1239 - 2024-03-16
### Fixed:
- nuget: bump Microsoft.CodeAnalysis.CSharp from 4.8.0 to 4.9.2.
  - Updates the Microsoft.CodeAnalysis.CSharp. dependency. No expected user-facing changes.

## 1.1.1238 - 2024-03-16
### Fixed:
- Ava UI: Fix locale crash.
  - Fixes a UI crash when an invalid locale value is taken from system, or present in config.

## 1.1.1237 - 2024-03-16
### Fixed:
- Ava UI: Content Dialog Fixes.
  - Fixes a macOS-specific error: "Can't have a toolbar in a window with <NSNextStepFrame: 0x4835f5670> as its borderView", though this did not affect emulator functionality. 

## 1.1.1236 - 2024-03-16
### Fixed:
- nuget: bump Microsoft.IdentityModel.JsonWebTokens from 7.3.0 to 7.4.0.
  - Updates the Microsoft.IdentityModel.JsonWebTokens dependency. No expected user-facing changes.

## 1.1.1235 - 2024-03-16
### Fixed:
- nuget: bump the avalonia group with 2 updates.
  - Updates Avalonia dependencies. No expected user-facing changes.

## 1.1.1234 - 2024-03-16
### Fixed:
- chore: remove repetitive words.
  - Fixes a few typos in the code.

## 1.1.1233 - 2024-03-16
### Fixed:
- Ava UI: Fix Title Update Manager not refreshing app list.
  - Fixes an issue where game updates would not show as applied on the UI immediately after being applied.

## 1.1.1232 - 2024-03-16
### Fixed:
- Update ApplicationID for Discord Rich Presence.
  - Fixes an issue where the Discord icon for Ryujinx activity did not display proper transparency.

## 1.1.1231 - 2024-03-14
### Fixed:
- GPU: Rebind RTs if scale changes when binding textures.
  - Fixes an issue where some games would show a couple frames of garbled graphics after camera switches, only when running at resolutions higher than native. Affected games include Super Mario Odyssey and The Legend of Zelda: Tears of the Kingdom.

## 1.1.1230 - 2024-03-14
### Fixed:
- Consider Polygon as unsupported if triangle fans are unsupported on Vulkan.
  - On macOS, should fix the stats chart in Pokémon Legends Arceus and Pokémon Scarlet/Violet. 

## 1.1.1229 - 2024-03-14
### Fixed:
- Separate guest/host tracking + unaligned protection.
  - Required for the upcoming host tracked memory manager mode. No expected user-facing changes. 

## 1.1.1228 - 2024-03-13
### Fixed:
- Ava UI: Update Ava.
  - Updates the Avalonia package to v11.0.10. 

## 1.1.1227 - 2024-03-13
### Fixed:
- infra: Fix updater for old Ava users.
  - Fixes an issue where mainline Avalonia builds before 1.1.1216 would get stuck in a loop when trying to update to newer versions.

## 1.1.1226 - 2024-03-13
### Fixed:
- Increase texture cache total size limit to 1024 MB.
  - Fixes a regression from 1.1.606 that caused 1440p/2160p resolution mods to significantly drop performance or crash. 

## 1.1.1225 - 2024-03-13
### Fixed:
- Fix geometry shader passthrough issue.
  - Fixes a regression from 1.1.993 that broke character rendering in Game Builder Garage.

## 1.1.1224 - 2024-03-11
### Fixed:
- Passthrough mouse for win32.
  - Should fix touchscreen controls on games not working on the Avalonia UI on Windows systems.

## 1.1.1223 - 2024-03-10
### Fixed:
- Fix lost copy and swap problem on shader SSA deconstruction.
  - Fixes fog in Princess Peach: Showtime! Demo.
  - Fixes z-fighting in The Legend of Zelda: Tears of the Kingdom.
  - Fixes puddles of water and carpets in No More Heroes 3.
  - Fixes fences and the floor in special stages in Kirby and the Forgotten Land.

## 1.1.1222 - 2024-03-09
### Fixed:
- Refactor memory managers to a common base class, consolidate Read() method logic.
  - Code cleanup. No expected user-facing changes.

## 1.1.1221 - 2024-03-08
### Fixed:
- Update dependencies from SixLabors to the latest version before the license change.
  - Fixes a missing fonts crash on games such as Monster Hunter Generations Ultimate (hopefully for good now).
  - Fixes a security vulnerability present in previous versions of ImageSharp.

## 1.1.1220 - 2024-03-07
### Fixed:
- LightningJit: Disable some cache ops and CTR_EL0 access on Windows Arm.
  - Allows LightningJit to work on Windows ARM systems.

## 1.1.1219 - 2024-03-07
### Changed:
- UI: Reduce minimum window size to 800x500.
  - Allows the emulator window to be resized down to a minimum of 800x500.

## 1.1.1218 - 2024-03-07
### Added:
- Add title of game to screenshot text.
  - Ryujinx screenshot filenames will now contain the application title.

## 1.1.1216-1.1.1217 - 2024-03-02
### Changed:
- infra: Make Avalonia the default UI.
- Replaces the old GTK user interface with the Avalonia-based UI as the default on Windows and Linux (macOS already had it). Avalonia has feature parity with GTK, plus the following differences:
  - Volume level can now be adjusted from the bottom status bar.
  - Default controller profile will now be automatically loaded upon selecting a new controller. 
  - Improved the controller applet considerably, with a menu button to go directly into input settings.
  - Added a customizable grid view for the games list.  
  - Added a game loading screen which displays PPTC and shader cache progress.
  - Added configurable hotkeys for vsync toggle (framerate limiter), screenshots, mute/unmute audio, increase/decrease volume and increase/decrease resolution.
  - Added a save manager under Options > Manage User Profiles. Allows for easy file deletion and quick save folder opening. Also adds an option to restore deleted user profiles using existing saves. (Backup/restore functionality is still being worked on.)
  - Added Brazilian Portuguese, Castilian Spanish, French, German, Greek, Hebrew, Italian, Japanese, Korean, Polish, Russian, Simplified Chinese, Traditional Chinese, Turkish and Ukrainian localizations for UI text.
  - Fixes issues with emulator files not being properly extracted sometimes.
  - Fixes an issue where PCs with 2 graphics cards (especially laptops) wouldn't properly detect the GPU, crashing on boot.
  - Fixes the occasional "GTK Critical" crash when double-clicking to run games.
  - Fixes a crash where some games, such as the Monster Hunter series, would error out when bringing up the software keyboard due to missing fonts in the system.
  - Fixes an extremely rare issue where attempting to install firmware would freeze Ryujinx.
  - Many, many more smaller changes.

## 1.1.1215 - 2024-03-02
### Fixed:
- Avalonia: only enable gamescope workaround under it.
  - Fixes a regression from 1.1.1203 that caused Avalonia's drop-down menus to not show until after several clicks on some Linux installations.

## 1.1.1214 - 2024-02-24
### Fixed:
- Change packed aliasing formats to UInt.
  - On AMD GPUs, fixes graphical glitches in CEIBA, Wet Steps and Yokai Watch 1.

## 1.1.1213 - 2024-02-23
### Fixed:
- nuget: bump System.Drawing.Common from 8.0.1 to 8.0.2.
  - Dependency update. No expected user-facing changes.

## 1.1.1212 - 2024-02-23
### Fixed:
- IPC code gen improvements.
  - Code cleanup. No expected changes in games.

## 1.1.1211 - 2024-02-22
### Fixed:
- Migrate Audio service to new IPC.
  - Fixes missing sound in Unicorn Overlord Demo.
  - Fixes an issue where emulator volume would get reset to 100% in certain games that set custom volumes (tested with River City Girls Zero).

## 1.1.1210 - 2024-02-22
### Fixed:
- OpenGL: Mask out all color outputs with no fragment shader.
  - Fixes shadows in Penny's Big Breakaway on OpenGL.

## 1.1.1209 - 2024-02-22
### Fixed:
- Ensure service init runs after Horizon constructor.
  - Fixes an uncommon crash when launching games after stopping emulation multiple times.

## 1.1.1208 - 2024-02-22
### Fixed:
- Implement virtual buffer dependencies.
  - Fixes model flickering in Apollo Justice: Ace Attorney Trilogy on macOS and OpenGL.
  - Allows Monster Hunter Rise: Sunbreak to go in-game on macOS.

## 1.1.1207 - 2024-02-22
### Fixed:
- Vulkan: Properly reset barrier batch when splitting due to mismatching flags.
  - Fixes a regression from 1.1.1205 that caused several games to crash.

## 1.1.1206 - 2024-02-21
### Fixed:
- Vulkan: Disable push descriptors on older NVIDIA GPUs.
  - Fixes a regression from 1.1.1198 that caused rendering issues on Nvidia GPU series 1000 and older.

## 1.1.1205 - 2024-02-21
### Fixed:
- Vulkan: Fix barrier batching past limit.
  - Fixes a regression from 1.1.1199 that caused some games to freeze.

## 1.1.1204 - 2024-02-19
### Fixed:
- Avalonia UI: Update English tooltips.
  - Updates a few settings tooltips on the Avalonia UI to better explain what the settings do.

## 1.1.1203 - 2024-02-19
### Fixed:
- Avalonia: Fix gamescope once and for all.
  - Fixes Avalonia context menus not showing on the Steam Deck's gaming mode.

## 1.1.1202 - 2024-02-17
### Fixed:
- LightningJit: Add a limit on the number of instructions per function for Arm64.
  - Fixes a crash in Bluey: The Videogame with LightningJit.

## 1.1.1200-1.1.1201 - 2024-02-17
### Fixed:
- hid: Stub SetTouchScreenResolution.
  - Allows Tomb Raider I-III Remastered to go in-game without enabling "Ignore Missing Services".

## 1.1.1199 - 2024-02-16
### Fixed:
- Vulkan: Improve texture barrier usage, timing and batching.
  - Fixes graphical issues on the Turnip Mesa driver in Mario Kart 8 Deluxe, Super Mario Odyssey and other games.
  - Fixes Bayonetta Origins: Cereza and the Lost Demon water surfaces on Vulkan on desktop GPUs.

## 1.1.1198 - 2024-02-16
### Fixed:
- Vulkan: Use push descriptors for uniform bindings when possible.
  - Improves Vulkan performance significantly on AMD Mesa drivers and to a lesser degree on AMD Windows drivers.
  - May improve stability on more underpowered systems.


<details hidden>
<summary>Older releases</summary>
<br>


## 1.1.1197 - 2024-02-15
### Fixed:
- Implement X8Z24 texture format.
  - Fixes some lighting issues on Tomb Raider I-III Remastered.

## 1.1.1196 - 2024-02-15
### Fixed:
- Fix PermissionLocked check on UnmapProcessCodeMemory.
  - Fixes a crash on Tomb Raider I-III Remastered when the game is changed on the selection menu.

## 1.1.1195 - 2024-02-15
### Fixed:
- Remove Vulkan SubgroupSizeControl enablement code.
  - Fixes a crash when trying to run the emulator with Dozen (Vulkan emulated with Direct3D) driver on Windows.

## 1.1.1194 - 2024-02-15
### Changed:
- Stub VSMS related ioctls.
  - Fixes a crash when starting Tomb Raider I-III Remastered. It still requires enabling the "ignore missing services" option to run.

## 1.1.1193 - 2024-02-15
### Changed:
- Updaters: Fix ARM Linux Updater.
  - Allows the auto-updater to work on Arm64 Linux.

## 1.1.1192 - 2024-02-11
### Fixed:
- Handle exceptions when checking user data directory for symlink.
  - Fixes a crash introduced on version 1.1.1191 when the emulator is started for the first time.

## 1.1.1191 - 2024-02-11
### Fixed:
- macOS: Stop storing user data in Documents for some users; fix symlinks.
  - Stops storing user data in Documents for some users on macOS and fixes symlinks.

## 1.1.1190 - 2024-02-11
### Changed:
- Force add linux-x64 apphost in flathub nuget source
  - Some misc changes to handle deployment on flatpak for arm64.


## 1.1.1189 - 2024-02-11
### Changed:
- Restore Nuget packages for linux-arm64 for Flatpak
  - Some misc changes to handle deployment on flatpak for arm64.


## 1.1.1188 - 2024-02-11
### Changed:
- Add Open Mod Dir button again
  - Add the Open Mod Dir button again on Avalonia


## 1.1.1187 - 2024-02-11
### Changed:
- Capitalisation Consistency
  - Some misc code cleanup


## 1.1.1186 - 2024-02-11
### Changed:
- Standardize logging locations across desktop platforms
  - Create logs in a more logical path depending on the platform
    - `%APPDATA%\Ryujinx\Logs` on Windows.
    - `~/.config/Ryujinx/Logs` on Linux.
    - `~/Library/Logs/Ryujinx` (or `~/Library/Application Support/Ryujinx/Logs`) on macOS.


## 1.1.1185 - 2024-02-11
### Changed:
- Reorder available executables in Ryujinx.sh
  - Avoid Ryujinx.Headless.SDL2 as a last resort in Ryujinx.desktop when you have more than one executable installed


## 1.1.1184 - 2024-02-11
### Changed:
- Remove ReflectionBinding in Mod Manager
  - Some cleanup in the mod manager on Avalonia


## 1.1.1183 - 2024-02-11
### Changed:
- Update Avalonia About Window
  - Fix minor style issue in the about window on Avalonia


## 1.1.1182 - 2024-02-11
### Fixed:
- Fix mip offset/size for full 3D texture upload on Vulkan
  - Fix broken water in Hyrule Warriors Age of Calamity


## 1.1.1181 - 2024-02-10
### Fixed:
- Retrigger CI after failure of complete release


## 1.1.1180 - 2024-02-10
### Fixed:
- Add missing RID exclusions for linux-arm64
  - Clean up unrelated libraries from linux-arm64 release tar


## 1.1.1179 - 2024-02-10
### Changed:
- Enable Linux ARM64 on build and release
  - We now provide linux-arm64 builds


## 1.1.1178 - 2024-02-10
### Fixed:
- Set PointSize in shader on OpenGL
  - Fix possible undefined behaviour on some drivers


## 1.1.1177 - 2024-02-10
### Fixed:
- Make IOpenGLContext.HasContext context dependent
  - Fix SDL2 headless on Wayland


## 1.1.1176 - 2024-02-10
### Fixed:
- Load custom SDL mappings from application data folder
  - Fix regression on macOS for SDL_GameControllerDB.txt loading path


## 1.1.1175 - 2024-02-10
### Fixed:
- Force CPU copy for non-identity DMA remap
  - Fix some bugs with inverted RGBA components on the game 30XX


## 1.1.1174 - 2024-02-10
### Changed:
- Update Ryujinx.Graphics.Nvdec.Dependencies to 5.0.3-build14
  - Add linux-arm64 and update to ffmpeg 5.0.3


## 1.1.1173 - 2024-02-08
### Fixed:
- Revert Avalonia bump
  - X popup position broke entirely

## 1.1.1172 - 2024-02-08
### Changed:
- Remove SDC.
  - Removes remaining usages of System.Drawing.Common.

## 1.1.1171 - 2024-02-08
### Fixed:
- LightningJit: Reduce stack usage for Arm32 code.
  - Significantly reduces stack usage in some cases.
  - Fixes a crash in Gothic.

## 1.1.1170 - 2024-02-08
### Changed:
- Remove Vic Reference to Host1x.
  - Cleanup. No expected changes.

## 1.1.1169 - 2024-02-08
### Added:
- GPU: Implement BGR10A2 render target format.
  - Improves rendering on Infinite Tanks World War 2.

## 1.1.1168 - 2024-02-08
### Fixed:
- Bump Ava.
  - Dependency update. Fixes windows not appearing in Gamescope. (SteamOS Game Mode)

## 1.1.1167 - 2024-02-08
### Changed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.8.0 to 17.9.0.
  - Dependency update. No expected changes.

## 1.1.1166 - 2024-02-08
### Changed:
- GUI: Replace Flex Panels in favor of Wrap Panels for Avalonia.
  - Allows keyboard navigation in grid view, improves consistency, and removes a dependency.

## 1.1.1165 - 2024-02-08
### Fixed:
- GPU: Clamp vertex buffer size to mapped size if too high.
  - Fixes crash in Infinite Tanks World War 2 and Blades of Time.

## 1.1.1164 - 2024-02-07
### Changed:
- chore: Update Ryujinx.SDL2-CS to 2.30.0.
  - Dependency update. No expected changes.

## 1.1.1163 - 2024-02-06
### Added:
- Redact usernames from logs.
  - Improves user privacy in logs by redacting usernames.

## 1.1.1162 - 2024-02-06
### Fixed:
- HLE/SERVICES: Cache AccountService token data.
  - Fixes low framerates in games that request token data often, such as Monopoly for Nintendo Switch.

## 1.1.1161 - 2024-02-06
### Fixed:
- GUI: Fix crash when window is moved after a modal is dismissed.
  - Fixes a crash that could occur if the main window's position was changed if a modal was dismissed beforehand on Ava UI.

## 1.1.1160 - 2024-02-05
### Changed:
- nuget: bump SPB from 0.0.4-build28 to 0.0.4-build32.
  - Dependency update. No expected changes.

## 1.1.1159 - 2024-02-04
### Fixed:
- GPU: Revert change to skip flush when range size is 0.
  - Fixes a regression caused by 1.1.1066 in Sonic Team Racing.

## 1.1.1158 - 2024-02-04
### Fixed:
- GPU: Fix depth compare value for TLD4S shader instruction with offset.
  - Fixes shadows in Hotshot Racing.

## 1.1.1157 - 2024-02-04
### Fixed:
- GPU: Remove component operand for texture gather with depth compare.
  - Fixes a crash in Hotshot Racing on Vulkan.

## 1.1.1156 - 2024-02-03
### Fixed:
- Limit remote closed session removal to SM service.
  - Fixes a regression caused by 1.1.1155 in "multi-program" games (e.g. Super Mario 3D All-Stars).

## 1.1.1155 - 2024-02-02
### Fixed:
- Ensure SM service won't listen to closed sessions.
  - Resolves a bug where titles that use the JIT service (e.g. N64 NSO) would fail to boot after 1.1.1131.

## 1.1.1154 - 2024-01-31
### Added:
- Vulkan: Add Render Pass / Framebuffer Cache.
  - Reduces driver specific costs for render pass/framebuffer re-creation.

## 1.1.1153 - 2024-01-31
### Changed:
- nuget: bump Microsoft.IdentityModel.JsonWebTokens from 7.2.0 to 7.3.0.
  - Dependency update. No expected changes.

## 1.1.1152 - 2024-01-30
### Fixed:
- Fix opening the wrong log directory.
  - "Open Log directory" button now opens the correct log directory again.

## 1.1.1151 - 2024-01-30
### Fixed:
- Fix exception when trying to read output pointer buffer size.
  - Fixes a crash caused by the friend service IPC migration in Animal Crossing: New Horizons.

## 1.1.1150 - 2024-01-29
### Added:
- Cpu: Implement Vpadal and Vrintr instructions.
  - Adds implementations for the above CPU instructions to the JIT.
  - Required by Super Putty Squad and potentially other 32-bit titles.

## 1.1.1149 - 2024-01-29
### Fixed:
- deps: Update Avalonia.Svg.
  - Dependency update. No expected user changes.

## 1.1.1148 - 2024-01-29
### Fixed:
- Ava UI: Mod Manager Fixes (Again).
  - Fixed an error message typo.
  - Fixed deleting mods from directories.
  - Fixed deleting non-subdirectory mods.
  - Fixed excessive looping if parent directory found.

## 1.1.1147 - 2024-01-29
### Fixed:
- UI: Clarify Create Application Shortcut tooltip text.
  - Accurate information on where a shortcut is being created is now present on macOS releases.

## 1.1.1146 - 2024-01-29
### Fixed:
- Avalonia: Fix dialog issues caused by 1.1.1105.
  - Fixes a bug where multiple dialogs could be open at the same time and cause a deadlock.

## 1.1.1145 - 2024-01-29
### Fixed:
- Migrate friends services to new IPC.
  - Remaining friend services migrated and implemented/stubbed if required.
  - No expected changes in games.

## 1.1.1144 - 2024-01-29
### Fixed:
- Make config filename changable for releases & Log to Ryujinx directory if application directory is not writable.
  - Adjustments to where logs and other configuration files are stored under certain environments.
  - Most platforms will be unaffected**.

**INFO: This change has temporarily caused the wrong directory to open when selecting File->Open Log folder.
If you need to access logs before this is fixed, they will be in a folder next to where you keep the executable!

## 1.1.1143 - 2024-01-29
### Fixed:
- Mod: Do LayeredFs loading Parallel to improve speed.
  - Reduces the boot times on games with a large number of mods applied.

## 1.1.1142 - 2024-01-29
### Fixed:
- Fix NRE when calling GetSockName before Bind.
  - Zengeon will no longer crash on boot with a socket error.

## 1.1.1141 - 2024-01-27
### Fixed:
- infra: Reformat README.md & add new generic Mako workflow.
  - No expected changes to emulator functionality.

## 1.1.1140 - 2024-01-26
### Fixed:
- nuget: bump NetCoreServer from 7.0.0 to 8.0.7.
  - Updates the NetCoreServer dependency. No expected changes in games.

## 1.1.1139 - 2024-01-26
### Fixed:
- Allow skipping draws with broken pipeline variants on Vulkan.
  - Fixes a crash on macOS in Fire Emblem: Three Houses, triggered by using the "Divine Pulse" ability.

## 1.1.1138 - 2024-01-26
### Fixed:
- Ava UI: Mod Manager Fixes.
  - Fixes a crash when selecting an invalid mod directory.
  - Fixes a crash when trying to enumerate through a directory that Ryujinx doesn't have permissions to access.
  - Fixes mods not deleting at the parent directory, leaving invisible orphan files that would prevent re-importing of the same mod.
  - Better logging and user feedback.

## 1.1.1137 - 2024-01-25
### Added:
- Fs: Log when Commit fails due to PathAlreadyInUse.
  - Adds a logging message for situations where files that the emulator needs to access are being used by another process. Meant to help with troubleshooting.

## 1.1.1136 - 2024-01-25
### Added:
- Ava UI: Mod Manager.
  - Adds a mod manager to the Avalonia UI. To access it, right click on a game and click on "Manage Mods".

## 1.1.1135 - 2024-01-25
### Changed:
- Use driver name instead of vendor name in the status bar for Vulkan.
  - For instance, the status bar will display "RADV" instead of just "AMD" on Linux when using the RADV driver.

## 1.1.1134 - 2024-01-25
### Fixed:
- nuget: bump System.Drawing.Common from 8.0.0 to 8.0.1.
  - Updates the System.Drawing.Common dependency. No expected changes in games.

## 1.1.1133 - 2024-01-25
### Changed:
- Ava UI: Fix Warn & Remove Custom Theming.
  - Removes custom theme functionality from the Avalonia UI, as it was not being used, in part due to being too much work for the user.

## 1.1.1132 - 2024-01-25
### Fixed:
- nuget: bump DynamicData from 7.14.2 to 8.3.27.
  - Updates the DynamicData dependency. No expected changes in games.

## 1.1.1131 - 2024-01-25
### Fixed:
- Horizon: Implement arp:r and arp:w services.
  - Migrates arp:r and arp:w services to the Horizon project and updates them to reflect newest firmware changes. No expected changes in games.

## 1.1.1130 - 2024-01-25
### Fixed:
- ssl: Work around missing remote hostname for authentication.
  - Workaround for an issue where Ryujinx would crash when trying to interact with the GitHub API when using certain mods.

## 1.1.1129 - 2024-01-25
### Fixed:
- nuget: bump Microsoft.IO.RecyclableMemoryStream from 2.3.2 to 3.0.0.
  - Updates the Microsoft.IO.RecyclableMemoryStream dependency. No expected changes in games.

## 1.1.1128 - 2024-01-25
### Fixed:
- nuget: bump Microsoft.CodeAnalysis.CSharp from 4.7.0 to 4.8.0.
  - Updates the Microsoft.CodeAnalysis.CSharp dependency. No expected changes in games.

## 1.1.1127 - 2024-01-25
### Fixed:
- Vulkan: Use staging buffer for temporary constants.
  - May improve Vulkan performance slightly on some games.

## 1.1.1126 - 2024-01-25
### Fixed:
- Deps: OpenTK Bump.
  - Updates OpenTK dependencies. No expected changes in games.

## 1.1.1125 - 2024-01-24
### Added:
- Implement SQSHL (immediate) CPU instruction.
  - Fixes a crash when playing AV1 encoded videos on NXMP using dav1d decoder (homebrew).

## 1.1.1124 - 2024-01-24
### Fixed:
- Vulkan: Enumerate Query Pool properly.
  - Minor performance improvement on some games.

## 1.1.1123 - 2024-01-24
### Fixed:
- Use unix timestamps on GetFileTimeStampRaw.
  - The GetFileTimeStampRaw file system function now returns the correct timestamps.
  - Fixes incorrect file date and time on NXMP and possibly other homebrew.

## 1.1.1122 - 2024-01-22
### Fixed:
- Fix architecture preference for MacOS game shortcuts.
  - Fixes an issue where shortcuts created on Apple Silicon Macs would start games using Rosetta emulation by default, in some cases lowering performance.

## 1.1.1121 - 2024-01-22
### Fixed:
- Fix missing data for new copy dependency textures with mismatching size.
  - Fixes a texture cache bug where part of the texture could be missing, if its data overlaps with an existing, smaller texture.
  - Fixes text being corrupted in Shantae and the Pirate's Curse in a few cases, a regression introduced on build 1.1.605.
  - Might also fix random texture corruption in other games.

## 1.1.1120 - 2024-01-22
### Added:
- Add a separate device memory manager.
  - Reading or writing video data to unmapped memory regions will no longer cause the emulator to crash.
  - Should fix random crashes on PERSONA 5 Tactica while pre-rendered cutscenes are played, but that is unconfirmed.

## 1.1.1119 - 2024-01-22
### Fixed:
- Input: Improve controller identification.
  - Ensures that controllers are connected to a consistent player number when multiple controllers are connected and disconnected.
  - This ensures that, for example, Player 1 will not change to Player 2 and vice-versa if one of the controllers is disconnected.

## 1.1.1118 - 2024-01-21
### Fixed:
- Fix integer overflow on downsample surround to stereo.
  - Fixes audio peaking and crackling with SDL2 backend on Shin Megami Tensei V, and likely other games that use surround 5.1 output, when the connected audio device only supports stereo or mono.

## 1.1.1117 - 2024-01-20
### Added:
- Implement a new JIT for Arm devices.
  - Significantly improves code compilation speed and size on ARM devices.
  - 32-bit and 64-bit games will boot faster and no longer need PPTC when using JIT on ARM devices.
  - Close to native execution without need for platform specific hypervisor (e.g. Linux/Windows ARM devices).

  *Note: Games that are not playable via hypervisor on macOS, such as Breath of the Wild/Animal Crossing, may now work better via JIT.

## 1.1.1116 - 2024-01-20
### Added:
- Vulkan: Use templates for descriptor updates.
  - Improves performance for the open-source AMD mesa driver (RADV) by up to 12%.
  - Other drivers are not expected to see any noticable changes.

## 1.1.1115 - 2024-01-20
### Added:
- Support portable mode using the macOS app bundle.
  - Allows using the `portable` directory next to the macOS bundle.

## 1.1.1114 - 2024-01-18
### Fixed:
- Change shader cache init wait method.
  - Code cleanup. No expected changes in games.

## 1.1.1113 - 2024-01-18
### Fixed:
- Move most of signal handling to Ryujinx.Cpu project.
  - Code cleanup, will help the upcoming ARM JIT. No expected changes in games.

## 1.1.1112 - 2024-01-17
### Fixed:
- Ava UI: Update Ava & Friends.
  - Updates all Avalonia-related dependencies. May fix some issues with the UI.

## 1.1.1111 - 2024-01-16
### Fixed:
- Vulkan: Cache delegate for EndRenderPass.
  - Small Vulkan optimization. No known visible changes in games.

## 1.1.1110 - 2024-01-14
### Fixed:
- Fix vertex buffer size when switching between inline and state draw parameter.
  - Fixes rendering in Citizens of Space.

## 1.1.1109 - 2024-01-13
### Fixed:
- Revert Apple hypervisor force ordered memory change.
  - Reverts the change in 1.1.1072 due to causing freezes in several games on macOS, including Pokémon Scarlet/Violet and Red Dead Redemption. 
    - Animal Crossing and Splatoon 2 players will likely prefer to stay on 1.1.1108 (Breath of the Wild was still not playable with this change).

## 1.1.1108 - 2024-01-13
### Fixed:
- Fix Amiibo regression and some minor code improvements.
  - Fixes a regression introduced in 1.1.1102 that caused Amiibo functionality to not work if the Amiibo.json file didn't already exist, affecting mostly new Ryujinx installs.
  - Makes the Amiibo functionality more robust against certain exceptions.
  - Related exceptions are now logged.

## 1.1.1107 - 2024-01-13
### Fixed:
- Switch to Microsoft.IdentityModel.JsonWebTokens.
  - Minor dependency change. No expected changes in games.

## 1.1.1106 - 2024-01-12
### Added:
- Ava UI: RTL Language Support.
  - On the Avalonia UI, adds support for languages that read right to left.
  - Adds Hebrew as an option for UI language.

## 1.1.1105 - 2024-01-12
### Fixed:
- Ava UI: Better Controller Applet.
  - On the Avalonia UI, redesigns the controller applet to be clearer and easier to understand.
  - Clicking "Open Settings Window" on the controller applet will now take you directly to input settings.

## 1.1.1104 - 2024-01-03
### Fixed:
- Fix PPTC version string for firmware titles.
  - Allows NSO Nintendo 64 games to work with firmware versions 13.0 and up.

## 1.1.1103 - 2024-01-03
### Fixed:
- Add support for PermissionLocked attribute added on firmware 17.0.0.
  - Allows NSO Nintendo 64 games to work with firmware version 17.0.

## 1.1.1102 - 2023-12-27
### Fixed:
- Local Amiibo.json should be used if connection failed.
  - Allows offline usage of Amiibo if the Amiibo.json file has been loaded before.

## 1.1.1101 - 2023-12-25
### Fixed:
- Ava UI: Fix crash when clicking on a cheat's name.
  - On the Avalonia GUI, fixes a crash caused by clicking on a cheat name in the cheats manager while the game is running.
  - Fixes the game title disappearing from the cheats manager when accessing it while the game is running.

## 1.1.1100 - 2023-12-11
### Fixed:
- [macOS] Correctly set filetypes in Info.plist.
  - .nsp and .xci files will now be properly associated to open with Ryujinx on macOS, if the user chooses to do so.

## 1.1.1099 - 2023-12-11
### Fixed:
- Ava UI: Fix temporary volume not being set after unmute.
  - On the Avalonia UI, fixes an issue where muting and unmuting audio after changing the volume via the bottom status bar, would reset the volume to the original value, instead of returning it to what it was before muting.

## 1.1.1098 - 2023-12-04
### Fixed:
- Implement support for multi-range buffers using Vulkan sparse mappings.
  - Monster Hunter Rise: Sunbreak is now playable using Vulkan on Nvidia, AMD and Intel graphics cards. Apple Silicon GPUs and OpenGL still won't be able to run any updates newer than 3.9.1 for this game.

## 1.1.1097 - 2023-12-04
### Fixed:
- ApplicationLibrary: Skip invalid symlinks.
  - Fixes a FileNotFoundException caused by attempting to run a game on the games list that is no longer in the specified games directory.

## 1.1.1096 - 2023-12-04
### Fixed:
- Improve indication of emulation being paused by the User.
  - On emulation pause, a "Paused" indicator will be added to the application title bar, and the pause will also be logged on the console and log files.

## 1.1.1095 - 2023-12-04
### Fixed:
- editorconfig: Set default encoding to UTF-8.
  - Code cleanup. No expected changes to emulator functionality.

## 1.1.1094 - 2023-11-30
### Added:
- HLE: Add OS-specific precise sleep methods to reduce spinwaiting.
  - Reduces energy usage, especially on Linux and macOS. Should help battery-powered systems (such as the Steam Deck or laptops) not run out of battery as quickly while running games on Ryujinx.
  - May help devices that are thermal throttling.

## 1.1.1093 - 2023-11-22
### Fixed:
- Extend bindless elimination to see through shuffle.
  - Fixes missing cubemap reflections in Detective Pikachu Returns.

## 1.1.1092 - 2023-11-19
### Fixed:
- Enable copy dependency between RGBA8 and RGBA32 formats.
  - Fixes cicadas not rendering when you catch them in Yo-kai Watch 1.
  - Improves rendering in Wet Steps.
  - May improve rendering on other games that use OpenGL on the Switch.

## 1.1.1091 - 2023-11-19
### Fixed:
- Extend bindless elimination to see through Phis with the same results.
  - Fixes some shadow issues in Super Mario RPG.

## 1.1.1090 - 2023-11-18
### Fixed:
- misc: Default to Vulkan if available or running on macOS.
  - The default graphics backend will be Vulkan if a supported graphics card is detected.

## 1.1.1089 - 2023-11-16
### Fixed:
- Fix JitCache.Unmap called with the same address freeing memory in use.
  - Fixes a crash that would occur after launching the 3D All-Stars version of Super Mario 64, stopping emulation and then launching another game.

## 1.1.1088 - 2023-11-16
### Fixed:
- Fix macOS Path on .NET 8.
  - Fixes an issue on macOS where, after 1.1.1084, Ryujinx would look for the data folder in the user's Documents directory, rather than in Application Support.

## 1.1.1087 - 2023-11-15
### Fixed:
- Fix missing texture flush for draw then DMA copy sequence without render target change.
  - Fixes duplicate item icons in Fashion Dreamer.

## 1.1.1086 - 2023-11-15
### Fixed:
- chore: Update OpenTK to 4.8.1.
  - Updates the OpenTK dependency. No expected changes to emulator functionality.

## 1.1.1085 - 2023-11-15
### Fixed:
- Fix flatpak not building after .NET 8.

## 1.1.1084 - 2023-11-15
### Fixed:
- Migrate to .NET 8.
  - Migrates Ryujinx to the new .NET version.
  - Improves performance slightly on almost every game. 

## 1.1.1083 - 2023-11-14
### Fixed:
- Disable DMA GPU copy for block linear to linear copies.
  - Fixes rendering issues in Fashion Dreamer. 

## 1.1.1082 - 2023-11-14
### Fixed:
- Fix TextureGroup.SignalModifying not being called.
  - Fixes a regression from 1.1.1080 that broke textures in some games. 

## 1.1.1081 - 2023-11-14
### Changed:
- Change minimum OS to macOS 12 in Info.plist.
  - Ryujinx will no longer open on macOS 11 and older, as games could never run on those versions anyway. 

## 1.1.1080 - 2023-11-13
### Fixed:
- Do not set modified flag again if texture was not modified.
  - Fixes a regression from 1.1.1066 that caused garbled textures on OpenGL in Digimon Story Cyber Sleuth: Complete Edition.
  - On macOS, fixes lens flare flickering in The Legend of Zelda: Breath of the Wild.

## 1.1.1079 - 2023-11-11
### Fixed:
- Revert "Add support for multi game XCIs".
  - Reverts the change in 1.1.1076 due to causing issues with certain game dumps.

## 1.1.1078 - 2023-11-11
### Fixed:
- Switch back to ubuntu-20.04.
  - Fixed GitHub actions not building for Linux and macOS.

## 1.1.1077 - 2023-11-11
### Fixed:
- Switch to LLVM 15.
  - Attempted to fix GitHub actions not building for macOS.

## 1.1.1076 - 2023-11-11
### Fixed:
- Add support for multi game XCIs.

## 1.1.1075 - 2023-11-11
### Fixed:
- Create Desktop Shortcut fixes.
  - Fixes the application shortcuts not working when used by other software (such as Nvidia Game Experience).
  - Fixes the shortcuts not working on macOS.

## 1.1.1074 - 2023-11-11
### Fixed:
- Ava UI: Add accelerator keys for Options and Help.
  - Adds Alt+O for Options and Alt+H for Help shortcuts to the Avalonia UI.

## 1.1.1073 - 2023-11-11
### Fixed:
- UI: Change default hide cursor mode to OnIdle.
  - Mouse cursor will now be hidden on idle by default while a game is running. (Only affects new installs.)

## 1.1.1072 - 2023-11-07
### Fixed:
- Force all exclusive memory accesses to be ordered on AppleHv.
  - On macOS, fixes crashes and softlocks when running Animal Crossing: New Horizons, Splatoon 2 and The Legend of Zelda: Breath of the Wild with the hypervisor enabled.

## 1.1.1071 - 2023-11-06
### Fixed:
- Overhaul of string formatting/parsing/sorting logic for TimeSpans, DateTimes, and file sizes.
  - The console and logs will no longer show a warning for every game on the games list that has never been played.

## 1.1.1070 - 2023-11-05
### Fixed:
- Better handle instruction aborts when hitting unmapped memory.
  - Helps with debugging. No expected changes in games.

## 1.1.1069 - 2023-11-01
### Fixed:
- Fix AddSessionObj NRE regression.
  - Fixes a regression from 1.1.1067 that caused Baten Kaitos and Streets of Rage to crash after booting.

## 1.1.1068 - 2023-10-31
### Fixed:
- Implement copy dependency for depth and color textures.
  - Fixes shadow issues in Luigi's Mansion 3.

## 1.1.1067 - 2023-10-30
### Changed:
- [HLE] Remove ServerBase 1ms polling.
  - Reduces the presence of ServerBase on CPU profiles, especially for games that aren't particularly busy.
  - Greatly reduces power usage on Mario Kart 8 Deluxe and other games.

## 1.1.1066 - 2023-10-30
### Fixed:
- Skip some invalid texture flushes.
  - Fixes a memory corruption crash in Neptunia GameMaker R:Evolution, allowing it to go in-game.

## 1.1.1065 - 2023-10-25
### Added:
- Add ldn_mitm as a network client for LDN.
  - Adds ldn_mitm as a multiplayer option. ldn_mitm allows for connecting to hacked Switch consoles via local network. 
  - Contributes to upstreaming the closed-source LDN build.

## 1.1.1064 - 2023-10-24
### Fixed:
- macOS: Use user-friendly macOS version string.
  - Changes the displayed macOS version on logs to be the commonly used denomination, instead of the Darwin kernel version number.

## 1.1.1063 - 2023-10-24
### Fixed:
- Fix loading tickets from a Sha256PartitionFileSystem.
  - Fixes a regression from 1.1.1061 that caused certain game dumps to hang on boot.

## 1.1.1062 - 2023-10-23
### Fixed:
- Fix the AOC manager using incorrect paths.
  - Fixes a regression from 1.1.1061 that caused game files with DLC bundled in them to crash on boot.

## 1.1.1061 - 2023-10-22
### Fixed:
- Update to LibHac 0.19.0.
  - Allows Cassette Beasts, DeepOne and Tiny Thor to go in-game.

## 1.1.1060 - 2023-10-22
### Fixed:
- Fix NRE on shader gather operations with depth compare on macOS.
  - Fixes a macOS crash in Luigi's Mansion 3.

## 1.1.1059 - 2023-10-21
### Fixed:
- Revert "Ava UI: Input Menu Refactor" 
  - Removes changes made in 1.1.1058.
  - Should resolve issues with button values not appearing/saving correctly.

## 1.1.1058 - 2023-10-21
### Fixed:
- Ava UI: Input Menu Refactor.
  - Cleans up the Avalonia input settings window and code.
  - Allows using platform-specific keys (such as the Win key).
  - Allows for button names to be localized.

## 1.1.1057 - 2023-10-20
### Added:
- Add "Create Shortcut" To app context menu.
  - You can now right click a game on the list and click on "Create Application Shortcut" to create a shortcut for that specific game on your desktop.

## 1.1.1056 - 2023-10-20
### Added:
- Avalonia: Make slider scrollable with mouse wheel.
  - Allows changing slider values with the mouse wheel on the Avalonia UI.

## 1.1.1055 - 2023-10-20
### Fixed:
- Ava UI: Update to 11.0.5.
  - Updates the Avalonia package. May fix some issues with the Avalonia UI.

## 1.1.1054 - 2023-10-20
### Fixed:
- GPU: Add fallback when textureGatherOffsets is not supported.
  - Required for Xenoblade Chronicles: Definitive Edition to render on the latest MoltenVK, when it gets updated in the future.
  - May fix some minor issues on macOS.

## 1.1.1053 - 2023-10-18
### Fixed:
- Enable copy between MS and non-MS textures with different height.
  - Fixes a regression from 1.1.742 that caused Perky Little Things to not render. 

## 1.1.1052 - 2023-10-13
### Fixed:
- Horizon: Migrate usb and psc services.
  - Migrates usb and psc services to the Horizon project and updates them to reflect newest firmware changes. No expected changes in games. 

## 1.1.1051 - 2023-10-12
### Fixed:
- Replace ReaderWriterLock with ReaderWriterLockSlim.
  - Code cleanup. No expected changes in games. 

## 1.1.1050 - 2023-10-09
### Fixed:
- Fix games freezing after initializing LDN 1021 times.
  - Fixes a regression from 1.1.1026 that caused Pokémon Sword/Shield to softlock after 20 minutes. 

## 1.1.1049 - 2023-10-08
### Added:
- Avalonia: Show aspect ratio popup options in status bar.
  - Allows choosing the aspect ratio from the status bar on Avalonia.

## 1.1.1048 - 2023-10-07
### Fixed:
- Symbols.cs Get function return value fix.
  - Fixes a regression from 1.1.1043, though no games should have been affected.

## 1.1.1047 - 2023-10-07
### Added:
- GPU: Add HLE macros for popular NVN macros.
  - Implements HLE versions for popular NVN macros. Small performance improvements when using .NET 7 JIT.
  - Mainly improves performance for a future NativeAOT build with .NET 8.

## 1.1.1046 - 2023-10-07
### Fixed:
- HLE: Fix Mii CRC generation and minor issues.
  - Fixes all generated Miis having invalid CRCs. This does not fix the `Mario Kart 8: Deluxe` title screen crash.

## 1.1.1045 - 2023-10-06
### Fixed:
- Fix SPIR-V call out arguments regression.
  - Fixes a regression from 1.1.1041 that caused rendering issues in Master Detective Archives: Rain Code, some UE4 games and possibly others.

## 1.1.1044 - 2023-10-05
### Fixed:
- nuget: bump Microsoft.CodeAnalysis.CSharp from 4.6.0 to 4.7.0.
  - Updates the Microsoft.CodeAnalysis.CSharp dependency. No expected changes to emulator functionality.

## 1.1.1043 - 2023-10-05
### Fixed:
- Strings should not be concatenated using '+' in a loop.
  - Code cleanup. No expected changes in games.

## 1.1.1042 - 2023-10-04
### Fixed:
- Fix SPIR-V function calls.
  - Fixes a regression from the previous change that caused Nvidia GPUs to crash while compiling shaders.

## 1.1.1041 - 2023-10-04
### Fixed:
- Use unique temporary variables for function call parameters on SPIR-V.
  - Fixes games rendering as horizontal lines on AMD drivers 23.9.2 and newer. This affected Pokémon Legends Arceus, Pokémon Scarlet/Violet, The Legend of Zelda: Tears of the Kingdom, Xenoblade Chronicles: Definitive Edition and possibly others.

## 1.1.1040 - 2023-10-04
### Fixed:
- Avalonia: Add macOS check for Color Space Passthrough.
  - Hides the Color Space Passthrough setting on Windows and Linux, as the feature is only currently supported on macOS and MoltenVK.

## 1.1.1039 - 2023-10-03
### Fixed:
- Implement textureSamples texture query shader instruction.
  - Fixes water rendering in Cocoon.

## 1.1.1038 - 2023-10-02
### Fixed:
- Decrement nvmap reference count on surface flinger prealloc.
  - Fixes a GuestProgramBrokeExecution crash in Sifu.

## 1.1.1037 - 2023-09-29
### Fixed:
- Signal friends completion event and stub CheckBlockedUserListAvailability.
  - Allows Super Bomberman R 2 to go in-game.

## 1.1.1036 - 2023-09-29
### Fixed:
- Fix audio renderer compressor effect.
  - Fixes Ys X: Nordics having no audio.

## 1.1.1035 - 2023-09-27
### Fixed:
- nuget: bump FluentAvaloniaUI from 2.0.1 to 2.0.4.
  - Updates the FluentAvaloniaUI dependency to 2.0.4 and the Avalonia package version to 11.0.4. No expected changes to emulator functionality.

## 1.1.1034 - 2023-09-27
### Fixed:
- Implement NGC service.
  - Baten Kaitos Ⅰ & Ⅱ HD Remaster is now playable.
  - Allows Star Ocean The Second Story R Demo to boot.

## 1.1.1033 - 2023-09-27
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.31.0 to 7.0.0.
  - Updates the System.IdentityModel.Tokens.Jwt dependency. No expected changes to emulator functionality.

## 1.1.1032 - 2023-09-26
### Fixed:
- GPU: Don't create tracking handles for buffer textures.
  - Improves performance in Mortal Kombat 1, R-TYPE FINAL 2, and certain UE4 games that reached 99% FIFO.

## 1.1.1031 - 2023-09-26
### Fixed:
- Ava: Fix regressions by rewriting CheckLaunchState.
  - Fixes a regression from 1.1.1027 that caused Avalonia to hang on launch when trying to bring up warnings for missing prod.keys or low limits for memory mappings.

## 1.1.1030 - 2023-09-25
### Fixed:
- Reduce the amount of descriptor pool allocations on Vulkan.
  - Fixes VK_ERROR_UNKNOWN on AMD GPUs in Puyo Puyo Tetris.

## 1.1.1029 - 2023-09-25
### Fixed:
- Make Vulkan memory allocator actually thread safe.
  - Attempts to fix a rare crash reported on macOS in The Legend of Zelda: Tears of the Kingdom.

## 1.1.1028 - 2023-09-25
### Fixed:
- Add VTimer as alternative interrupt method on Apple Hypervisor.
  - Fixes softlocks/infinite loading screens on macOS with hypervisor enabled in Bravely Default II, Life is Strange: True Colors, Persona 5 Scramble/Strikers and possibly other games.

## 1.1.1027 - 2023-09-25
### Fixed:
- Ava UI: Refactor async usage.
  - Significantly speeds up Avalonia's main window startup time.

## 1.1.1026 - 2023-09-25
### Added:
- Add ldn:u implementation, INetworkClient interface and DisabledLdnClient.
  - Initial LDN service implementation. (Does NOT contain all necessary changes to actually play LDN multiplayer.)
  - Contributes to upstreaming the closed-source LDN build.

## 1.1.1025 - 2023-09-25
### Added:
- Headless: Add support for Scaling Filters, Anti-aliasing and Exclusive Fullscreen.
  - Adds Scaling Filters (Bilinear, Nearest, FSR), Anti-aliasing (None, FXAA, SMAA (all levels)) to the Headless SDL2 client. 
  - Adds support for an exclusive fullscreen mode on the Headless SDL2 client. (Currently not available on builds with a user interface.)

## 1.1.1024 - 2023-09-25
### Fixed:
- GPU: Discard data when getting texture before full clear.
  - Required for fixing a certain bug in Luigi's Mansion 3 in the future. 
  - Might reduce stuttering in some situations, though no noticeable changes were observed.

## 1.1.1023 - 2023-09-25
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.6.3 to 17.7.2.
  - Updates the Microsoft.NET.Test.Sdk dependency. No changes to emulator functionality.

## 1.1.1022 - 2023-09-23
### Fixed:
- Vulkan: Fix barriers on macOS.
  - Fixes a regression from 1.1.1014 that caused flickering graphics on macOS in Red Dead Redemption, The Legend of Zelda: Tears of the Kingdom and possibly other games.

## 1.1.1021 - 2023-09-22
### Added:
- [INFRA] Addition of basic contributor guides and docs framework.
  - Adds a contributing guide to the project on GitHub. 

## 1.1.1020 - 2023-09-20
### Fixed:
- Horizon: Migrate wlan and stubs latest services.
  - No expected changes in games.

## 1.1.1019 - 2023-09-19
### Fixed:
- Stub unsupported BSD socket options.
  - Allows Crysis Remastered to go in-game.

## 1.1.1018 - 2023-09-19
### Fixed:
- Fixes compiled bindings in cheat window.
  - Fixes a regression from 1.1.1016 that caused cheats not to show in the cheat manager on Avalonia.

## 1.1.1017 - 2023-09-18
### Fixed:
- Use compiled binding for localizations.
  - Code cleanup. No expected changes in games.

## 1.1.1016 - 2023-09-18
### Fixed:
- Remove more usages of reflection binding.
  - Code cleanup. No expected changes in games.

## 1.1.1015 - 2023-09-16
### Fixed:
- Replace ShaderOutputLayer with equivalent ShaderViewportIndexLayerEXT capability.
  - Fixes Vulkan crashes introduced in 1.1.1002 on old AMD GPUs in Pokémon Scarlet/Violet.

## 1.1.1014 - 2023-09-14
### Fixed:
- Fix some Vulkan validation errors (mostly related to barriers).
  - Fixes several Vulkan validation errors to make debugging easier. No expected changes in games.

## 1.1.1013 - 2023-09-14
### Fixed:
- Fix gl_Layer to geometry shader change not writing gl_Layer.
  - Fixes a regression from 1.1.1002 that caused black rendering on older GPUs in Pokémon Scarlet/Violet (for example, Nvidia MX cards).

## 1.1.1012 - 2023-09-14
### Fixed:
- lbl: Migrate service to Horizon.
  - Code cleanup. No expected changes in games.

## 1.1.1011 - 2023-09-10
### Fixed:
- Fix shader GlobalToStorage pass when base address comes from local or shared memory.
  - Fixes a regression from 1.1.896 causing a few failures to find storage buffers in Splatoon 3 and probably other games, though no visible changes were found.

## 1.1.1010 - 2023-09-07
### Fixed:
- Replacing 'Assembly.GetExecutingAssembly()' with 'Type.Assembly'.
  - Code cleanup. No expected changes in games.

## 1.1.1009 - 2023-09-05
### Fixed:
- Delete ResourceAccess.
  - Code cleanup. No expected changes in games.

## 1.1.1008 - 2023-09-04
### Added:
- Add macOS Headless release workflow.
  - Adds macOS headless builds to master releases and pull requests.

## 1.1.1007 - 2023-09-04
### Fixed:
- Fix ShaderTools GpuAcessor default values.
  - Fixes a regression from 1.1.985 that caused ShaderTools to assert in debug mode and produce "incorrect" code on release due to the default graphics state being invalid. No changes in user builds.

## 1.1.1006 - 2023-09-04
### Fixed:
- Fix layer size for 3D textures with NPOT depth.
  - Fixes a regression from 1.1.863 which stopped grass/bushes/flowers/tree animations in Pokémon Scarlet/Violet. 

## 1.1.1005 - 2023-09-02
### Fixed:
- Vulkan: Device Local and higher invocation count for buffer conversions.
  - Improves performance in Super Mario Sunshine on AMD and Nvidia GPUs.

## 1.1.1004 - 2023-09-02
### Fixed:
- Fix numeric SWKB validation.
  - Fixes Super Mario Odyssey Online's IP address selection.

## 1.1.1003 - 2023-08-30
### Fixed:
- opus: Implement GetWorkBufferSizeExEx and GetWorkBufferSizeForMultiStreamExEx.
  - Sea of Stars is now playable.

## 1.1.1002 - 2023-08-29
### Fixed:
- Geometry shader emulation for macOS.
  - Fixes several missing graphics on macOS in Crash Bandicoot N. Sane Trilogy, Luigi's Mansion 3, Mario Strikers: Battle League, Marvel Ultimate Alliance 3, Nier Automata: The End of YoRHa Edition, Splatoon 3, Super Mario Maker 2, The Liar Princess and the Blind Prince and possibly other games.
  - Fixes a crash in Shin Megami Tensei III and allows it to go in-game on macOS.
  - Contributes towards upstreaming the closed-source macOS changes.

## 1.1.1001 - 2023-08-29
### Fixed:
- Add SmallChange properties to all sliders.
  - Pressing arrow keys on Avalonia while a slider is selected will no longer make the slider jump to the highest or lowest values.

## 1.1.1000 - 2023-08-23
### Fixed:
- Vulkan: Fix MoltenVK flickering.
  - Fixes graphical bugs on macOS in Super Mario Odyssey and several other games.

## 1.1.999 - 2023-08-20
### Fixed:
- Fix invalid audio renderer buffer size when end offset < start offset.
  - Fixes a crash in Disgaea 5 Complete at the end of the mission "Dreaming Mushroom" in Episode 4.

## 1.1.998 - 2023-08-19
### Fixed:
- Fix debug assert on services without pointer buffer.
  - Fixes an assert introduced in 1.1.996 that only affected debug builds. No changes in user builds.

## 1.1.997 - 2023-08-18
### Fixed:
- Implement support for masked stencil clears on Vulkan.
  - Resolves foliage and other smearing/ghosting effects in Red Dead Redemption when using Vulkan.

## 1.1.996 - 2023-08-17
### Fixed:
- mm: Migrate service in Horizon project.
  - Code cleanup. No expected changes in games.

## 1.1.995 - 2023-08-16
### Fixed:
- Fix vote and shuffle shader instructions on AMD GPUs.
  - Fixes black shadows/spots/flickering on AMD graphics cards in Luigi's Mansion 3, Marvel Ultimate Alliance 3, Master Detective Archives: Rain Code, Monster Hunter Rise, Nier Automata: The End of YoRHa Edition, Triangle Strategy and possibly other games.

## 1.1.994 - 2023-08-16
### Fixed:
- Prefer jagged arrays over multidimensional.
  - Code cleanup. No expected changes in games.

## 1.1.993 - 2023-08-16
### Fixed:
- Declare and use gl_PerVertex block for VTG per-vertex built-ins.
  - Code cleanup. No expected changes in games.

## 1.1.992 - 2023-08-16
### Fixed:
- Vulkan: Periodically free regions of the staging buffer.
  - Fixes an edge case exposed by 1.1.988 where some games on Windows (for instance, Super Mario Odyssey) would suffer a large stutter periodically.

## 1.1.991 - 2023-08-16
### Fixed:
- GPU: Add Z16RUnormGUintBUintAUint format.
  - Fixes graphical issues in Asterix & Obelix XXL: Romastered, Go Rally, Monster Blast, Pyramid Quest and Spencer.
  - May fix similar issues in games using OpenGL on the Switch.

## 1.1.990 - 2023-08-16
### Fixed:
- UI: New Crowdin updates.
  - Updates Avalonia GUI localizations with the latest changes from Crowdin.

## 1.1.989 - 2023-08-16
### Fixed:
- Implement scaled vertex format emulation.
  - Required for geometry shader emulation on macOS. No expected changes in games.

## 1.1.988 - 2023-08-14
### Fixed:
- Vulkan: Buffer Mirrors for MacOS performance.
  - Improves macOS performance greatly in literally every single game. 
  - Contributes towards upstreaming the closed-source macOS changes.

## 1.1.987 - 2023-08-14
### Fixed:
- Simplify resolution scale updates.
  - Code cleanup. No expected changes in games.

## 1.1.986 - 2023-08-14
### Fixed:
- GPU: Track basic buffer copies that modify texture memory.
  - Fixes broken icons in Dragon Quest Builders.
  - May fix similar issues in games using OpenGL on the Switch.

## 1.1.985 - 2023-08-13
### Changed:
- Delete ShaderConfig and organize shader resources/definitions better.
  - Required for geometry shader emulation on macOS. No expected changes in games.

## 1.1.984 - 2023-08-13
### Fixed:
- "static readonly" constants should be "const" instead.
  - Code cleanup. No expected changes in games.

## 1.1.983 - 2023-08-13
### Changed:
- Ava UI: Remove animations on listbox items.
  - Removes fade-in animations on list items, as they looked inconsistent with the rest of the Avalonia menus.

## 1.1.982 - 2023-08-12
### Fixed:
- Ava UI: Make some settings methods async.
  - Vulkan device, audio backend, network interface and time-zone configuration converted to asynchronous Tasks.
  - Reduces the start-up time of the settings window in Avalonia.

## 1.1.981 - 2023-08-12
### Fixed:
- Ava UI: Allow DPI switching on Windows.
  - Fixes a Windows issue where Avalonia would look blurrier if system scaling was higher than 100% and multiple monitors with different DPI were used.

## 1.1.980 - 2023-08-12
### Fixed:
- Ava UI: Avalonia 11 & FluentAvalonia 2 Support.
  - Fixes an issue where windowed game performance on Avalonia would be worse than on the GTK UI (fullscreen performance was equal between the two).
  - Title bar color now matches Windows theme.
  - Fixes text alignment issues on non-Windows platforms.
  - Fixes janky textboxes, toggle buttons and checkboxes.

## 1.1.979 - 2023-08-09
### Fixed:
- [Hotfix] hid: Prevent out of bounds array access.
  - Fixes a regression from 1.1.978 that caused ARCropolis mods to crash.

## 1.1.978 - 2023-08-09
### Fixed:
- Allow access to code memory for exefs mods.
  - Fixes an issue where games would crash if mods utilizing JIT (most notably exlaunch) were used.

## 1.1.977 - 2023-08-07
### Added:
- Implement Color Space Passthrough option (Vulkan/Avalonia only).
  - Added the option to pass the color space selector to the native display instead of the backend.
  - Allows P3 and other wide-gamut displays to utilize their entire color space at the cost of intended sRGB color accuracy.

## 1.1.976 - 2023-08-07
### Fixed:
- Do not add more code after alpha test discard on fragment shader.
  - On macOS, fixes a crash in Pikmin 3 when finishing the tutorial or on the results screen at the end of a day.

## 1.1.975 - 2023-08-06
### Fixed:
- GPU: Don't sync/bind index buffer when it's not in use.
  - On macOS, improves performance in Pokémon Legends Arceus.

## 1.1.974 - 2023-08-03
### Fixed:
- GPU: Enable VK_EXT_4444_formats.
  - Fixes a Vulkan validation error. No expected changes in games.

## 1.1.973 - 2023-08-02
### Fixed:
- nuget: bump DiscordRichPresence from 1.1.3.18 to 1.2.1.24.
  - Updates the DiscordRichPresence dependency. No changes to emulator functionality.

## 1.1.972 - 2023-07-30
### Fixed:
- (Graphics.Shader): Handle EmitSuatom constant dests and EmitSuld zero dest reg.
  - Jurassic World Evolution now goes in-game.

## 1.1.971 - 2023-07-30
### Fixed:
- CPU (A64): Add Fmaxp & Fminp Scalar Inst.s, Fast & Slow Paths; with Tests.
  - Fixes a crash in Jurassic World Evolution, though it still does not go in-game.

## 1.1.970 - 2023-07-29
### Fixed:
- Fix incorrect fragment origin when YNegate is enabled.
  - Fixes upside-down rendering in 20XX and Go Rally, and possibly other games using OpenGL on the Switch.

## 1.1.969 - 2023-07-24
### Added:
- Add workflow to automatically check code style issues for PRs.
  - As a result of this and all previous dotnet reformatting changes, code formatting reviews on new Pull Requests on GitHub will now be automated, saving the developers a lot of time.

## 1.1.968 - 2023-07-21
### Fixed:
- Ava UI: Remove IsGameRunning from some dialog methods.
  - Fixes a regression from 1.1.967 that caused the title updater, dlc manager, about, and check for updates to not spawn their content dialogs.

## 1.1.967 - 2023-07-21
### Fixed:
- Ava UI: Remove IsActive checks from dialog methods.
    - Fixes content dialogs for the controller applet and software keyboard not spawning when the window was unfocused.

## 1.1.966 - 2023-07-19
### Fixed:
- HLE: Fix corrupted Mii structs.
  - Fixes a regression from 1.1.962 that caused Mario Kart 8 Deluxe to crash after playing one race in VS mode and returning to the course selection.

## 1.1.965 - 2023-07-18
### Fixed:
- sdl2: Update to Ryujinx.SDL2-CS 2.28.1.
  - May improve support for third-party Nintendo Switch Controllers.
  - Fixes Xbox One Controllers from powering off when opening the settings menu.
  - Fixes Xbox One Controllers from randomly not receiving inputs.

## 1.1.964 - 2023-07-17
### Fixed:
- [Hotfix] sockets: Resolve empty port requests to 0 again.
  - Fixes a regression from 1.1.962 that caused DNS (2306-0520) errors and guest internet access not to work on several games. (This error will still appear if you try to connect to Nintendo Switch Online.)

## 1.1.963 - 2023-07-16
### Fixed:
- [CPU] Hotfix missing ToNearest rounding mode cases.
  - Fixes a regression from 1.1.923 that caused some tests to fail. No known changes in games.

## 1.1.962 - 2023-07-16
### Fixed:
- [Ryujinx.HLE] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.961 - 2023-07-14
### Fixed:
- Fix some Vulkan validation errors.
  - No expected changes in games.

## 1.1.960 - 2023-07-11
### Fixed:
- Move ShaderBinaries into individual .spv files.
  - No expected changes in games.

## 1.1.959 - 2023-07-11
### Fixed:
- Move support buffer update out of the backends.
  - Moves support buffer update to the GPU project, removes duplicate code. No expected changes in games.

## 1.1.958 - 2023-07-10
### Fixed:
- MacOS: Allow barriers inside a render pass for non-Apple GPUs and don't treat as TBDR.
  - No known changes in games.

## 1.1.957 - 2023-07-10
### Fixed:
- MacOS: Fix rendering on AMD GPUs.
  - Fixes flickering/partial rendering on macOS systems with AMD graphics cards in Pokémon Sword/Shield and possibly other games.

## 1.1.956 - 2023-07-07
### Fixed:
- [Ryujinx.Ava] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.955 - 2023-07-06
### Changed:
- Revert "sdl: set SDL_HINT_GAMECONTROLLER_USE_BUTTON_LABELS to 0".
  - Reverted due to causing issues with official Nintendo controllers.

## 1.1.954 - 2023-07-06
### Changed:
- sdl: set SDL_HINT_GAMECONTROLLER_USE_BUTTON_LABELS to 0.
  - ABXY buttons should have the correct default mapping on the controller profile regardless of brand.

## 1.1.953 - 2023-07-06
### Added:
- Headless: Add support for fullscreen option.
  - Adds a "--fullscreen" argument to headless builds so they can be launched in fullscreen.
  - Makes it easier to launch games in fullscreen mode from third party launchers (such as Steam Big Picture).

## 1.1.952 - 2023-07-03
### Changed:
- Stop identifying shader textures with handle and cbuf, use binding instead.
  - Improves flexibility of shader and texture-related code. No expected changes in games.

## 1.1.951 - 2023-07-01
### Fixed:
- [Ryujinx.Graphics.Gpu] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.950 - 2023-07-01
### Fixed:
- [Hotfix] Fix naming issue in ControllerWindow.
  - Fixes a regression from 1.1.948 that caused controller configuration to error out when saving.

## 1.1.949 - 2023-07-01
### Fixed:
- [Ryujinx.Audio] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.948 - 2023-07-01
### Fixed:
- [Ryujinx] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.947 - 2023-07-01
### Fixed:
- [Ryujinx.Horizon] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.946 - 2023-07-01
### Fixed:
- [Ryujinx.Graphics.Vulkan] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.945 - 2023-07-01
### Fixed:
- Prefer indexing instead of "Enumerable" methods on types implementing "IList".
  - Code cleanup. No expected changes in games.

## 1.1.944 - 2023-06-30
### Fixed:
- [Ryujinx.Cpu] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.943 - 2023-06-30
### Fixed:
- [Ryujinx.Tests] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.942 - 2023-06-28
### Fixed:
- [Ryujinx.Ui.Common] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.941 - 2023-06-28
### Fixed:
- [Ryujinx.Graphics.GAL] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.940 - 2023-06-28
### Fixed:
- macOS: Fix warning in some shell scripts.
  - Code cleanup. No expected changes in games.

## 1.1.939 - 2023-06-28
### Fixed:
- [Ryujinx.Headless.SDL2] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.938 - 2023-06-28
### Fixed:
- [Spv.Generator] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.937 - 2023-06-28
### Fixed:
- [Ryujinx.Graphics.Texture] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.936 - 2023-06-28
### Fixed:
- [Ryujinx.Common] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.935 - 2023-06-28
### Fixed:
- [Ryujinx.Memory] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.934 - 2023-06-28
### Fixed:
- [Ryujinx.Input] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.933 - 2023-06-28
### Fixed:
- [Ryujinx.Graphics.OpenGL] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.932 - 2023-06-28
### Added:
- Cpu: Implement VCVT (between floating-point and fixed-point) instruction.
  - Allows Death Road to Canada and Limbo to go in-game.

## 1.1.931 - 2023-06-27
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.6.2 to 17.6.3.
  - Updates the Microsoft.NET.Test.Sdk dependency. No expected changes to emulator functionality.

## 1.1.930 - 2023-06-27
### Fixed:
- [Ryujinx.Graphics.Nvdec.Vp9] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.929 - 2023-06-27
### Fixed:
- [Ryujinx.Graphics.Shader] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.928 - 2023-06-27
### Fixed:
- [Ryujinx.Horizon.Kernel.Generators] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.927 - 2023-06-27
### Fixed:
- dotnet-format: Apply new naming rule to all projects except Vp9.
  - Code cleanup. No expected changes in games.

## 1.1.926 - 2023-06-27
### Fixed:
- [Ryujinx.Graphics.Video] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.925 - 2023-06-27
### Fixed:
- [Ryujinx.Graphics.Host1x] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.924 - 2023-06-25
### Fixed:
- [Ryujinx.Horizon.Generators] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.923 - 2023-06-25
### Fixed:
- [ARMeilleure] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.922 - 2023-06-25
### Fixed:
- [Ryujinx.Input.SDL2] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.921 - 2023-06-25
### Fixed:
- [Ryujinx.SDL2.Common] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.920 - 2023-06-25
### Fixed:
- misc: memory: Migrate from OutOfMemoryException to SystemException entirely.
  - Fixes a regression with address space allocation while providing more information about the context of the exception. No known changes in games.

## 1.1.919 - 2023-06-25
### Fixed:
- [Ryujinx.Graphics.Device] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.918 - 2023-06-25
### Fixed:
- [Ryujinx.Audio.Backends.SDL2] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.917 - 2023-06-25
### Fixed:
- [Ryujinx.Graphics.Nvdec] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.916 - 2023-06-25
### Fixed:
- [Ryujinx.ShaderTools] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.915 - 2023-06-25
### Fixed:
- [Ryujinx.Graphics.Nvdec.FFmpeg] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.914 - 2023-06-25
### Fixed:
- [Ryujinx.Tests.Memory] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.913 - 2023-06-25
### Fixed:
- [Ryujinx.Graphics.Vic] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.912 - 2023-06-25
### Fixed:
- [Ryujinx.Tests.Unicorn] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.911 - 2023-06-25
### Fixed:
- Set COMPlus_DefaultStackSize to 2M in macOS.
  - Increases the default stack size to 2MB on macOS.
  - Fixes a SPIRV-Cross stack overflow that caused crashes on boot in Splatoon 3 and Mortal Kombat 11. Both titles are now playable.

## 1.1.910 - 2023-06-25
### Fixed:
- [Ryujinx.Horizon.Common] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.909 - 2023-06-24
### Fixed:
- [Ryujinx.Audio.Backends.SoundIo] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.908 - 2023-06-24
### Fixed:
- [Ryujinx.Audio.Backends.OpenAL] Address dotnet-format issues.
  - Code cleanup. No expected changes in games.

## 1.1.907 - 2023-06-24
### Fixed:
- Empty "case" clauses that fall through to the "default" should be omitted.
  - Code cleanup. No expected changes in games.

## 1.1.906 - 2023-06-24
### Fixed:
- Mutable fields should not be "public static".
  - Code cleanup. No expected changes in games.

## 1.1.905 - 2023-06-23
### Fixed:
- MemoryManagement: Change return types for Commit/Decommit to void.
  - Code cleanup. No expected changes in games.

## 1.1.904 - 2023-06-22
### Fixed:
- "Where" should be used before "OrderBy".
  - Code cleanup. No expected changes in games.

## 1.1.903 - 2023-06-22
### Fixed:
- "StartsWith" and "EndsWith" overloads that take a "char" should be used instead of the ones that take a "string".
  - Code cleanup. No expected changes in games.

## 1.1.902 - 2023-06-22
### Fixed:
- "Find" method should be used instead of the "FirstOrDefault" extension.
  - Code cleanup. No expected changes in games.

## 1.1.901 - 2023-06-22
### Fixed:
- "Exists" method should be used instead of the "Any" extension.
  - Code cleanup. No expected changes in games.

## 1.1.900 - 2023-06-22
### Fixed:
- Fix regression introduced by 1.1.733 on Intel GPUs.
  - Fixes a regression affecting Intel GPUs that caused erroneous lighting on character models in Pokémon Scarlet/Violet.

## 1.1.899 - 2023-06-22
### Fixed:
- GetHashCode should not reference mutable fields.
  - Code cleanup. No expected changes in games.

## 1.1.898 - 2023-06-20
### Added:
- misc: Implement address space size workarounds.
  - Required for ARM64 support when the kernel is configured to use between 63 and 39 bits for kernel space.

## 1.1.897 - 2023-06-17
### Fixed:
- Ensure shader local and shared memory sizes are not zero.
  - Should fix the Mysterio boss crash in Marvel Ultimate Alliance 3: The Black Order. Currently untested.

## 1.1.896 - 2023-06-15
### Fixed:
- Implement Load/Store Local/Shared and Atomic shared using new instructions.
  - Refactors some of the GPU code and makes it easier to implement more graphics backends in the future. No expected changes in games.

## 1.1.895 - 2023-06-15
### Fixed:
- Inheritance should not be redundant.
  - Code cleanup. No expected changes in games.

## 1.1.894 - 2023-06-14
### Fixed:
- Blocks should be synchronized on read-only fields.
  - Code cleanup. No expected changes in games.

## 1.1.893 - 2023-06-14
### Fixed:
- nuget: bump System.Management from 7.0.1 to 7.0.2.
  - Updates the System.Management dependency. No expected changes to emulator functionality.

## 1.1.892 - 2023-06-14
### Fixed:
- test: Make tests runnable on system without 4KiB page size.
  - Fixes running tests on Linux distros that use 16KiB pages, such as Asahi Linux.

## 1.1.891 - 2023-06-14
### Fixed:
- Fix Arm32 double to int/uint conversion on Arm64.
  - Fixes bad audio in Prinny Presents NIS Classics Volume 3: La Pucelle: Ragnarok / Rhapsody: A Musical Adventure on ARM64 (Apple Silicon macOS) devices.

## 1.1.890 - 2023-06-13
### Fixed:
- Mod Loader: Stop loading mods from folders that don't exactly match titleId.
  - Mods for a given game will no longer load from folders that have a slight, wrong variation of the game's title ID.

## 1.1.889 - 2023-06-12
### Fixed:
- UI: Correctly set 'shell/open/command; registry key for file associations.
  - Fixes Ryujinx file association/disassociation with .nsp and .xci formats via Tools > Manage file types.

## 1.1.888 - 2023-06-12
### Fixed:
- Make LM skip instead of crashing for invalid messages.
  - Fixes a crash when trying to start a match in Mortal Kombat 11 with no game update applied.

## 1.1.887 - 2023-06-12
### Fixed:
- hle: Stub IHidbusServer.GetBusHandle.
  - Allows newer versions of NES Switch Online and Starlink: Battle for Atlas to boot.

## 1.1.886 - 2023-06-12
### Added:
- infra: Add PR triage action.
  - Automatically assigns reviewers and some labels on pull requests on GitHub.

## 1.1.885 - 2023-06-11
### Fixed:
- Ava: Fix OpenGL on Linux again.
  - Fixes an issue where running games with OpenGL would crash on boot on Linux when using the Avalonia UI.

## 1.1.884 - 2023-06-11
### Fixed:
- ava: Show/Hide UI Hotkey fix.
  - Fixes the ability to Show/Hide the UI on Avalonia when using F4 or the assigned Hotkey.

## 1.1.883 - 2023-06-10
### Fixed:
- Implement fast path for AES crypto instructions on Arm64.
  - Fixes stuttering on ARM64 (Apple Silicon macOS) devices in Animal Crossing: New Horizons when saving the game with hypervisor disabled.

## 1.1.882 - 2023-06-10
### Added:
- Implement transform feedback emulation for hardware without native support.
  - Adds emulation for transform feedback for macOS devices and other hardware that do not support it in their drivers.
  - Allows Pokémon Scarlet/Violet, Pokémon Legends Arceus, Metroid Prime Remastered, Donkey Kong Country: Tropical Freeze and more to boot/render on macOS.
  - Contributes to upstreaming of the closed-source macOS build.

## 1.1.881 - 2023-06-09
### Fixed:
- Non-flags enums should not be used in bitwise operations.
  - Code cleanup. No expected changes to emulator functionality.

## 1.1.880 - 2023-06-09
### Fixed:
- Using 'ThenBy' instead.
  - Code cleanup. No expected changes to emulator functionality.

## 1.1.879 - 2023-06-09
### Fixed:
- macOS: Fix regression in macOS updater.
  - Fixes an issue introduced with 1.1.869 that caused the macOS updater not to work.

## 1.1.878 - 2023-06-09
### Fixed:
- macOS: Configuration Directory Fix.
  - Fixes an issue introduced with 1.1.630 that would cause an unhandled exception if the configuration directory was deleted.

## 1.1.877 - 2023-06-09
### Fixed:
- Prefer a 'TryGetValue' call over a Dictionary indexer access guarded by a 'ContainsKey.
  - Code cleanup. No expected changes to emulator functionality.

## 1.1.876 - 2023-06-09
### Fixed:
- Software Keyboard Applet Fixes.
  - Fixes an issue introduced in 1.1.862 that prevented numbers or spaces to be input in the keyboard applet in some games, like Pokémon Sword/Shield when naming Pokémon.

## 1.1.875 - 2023-06-09
### Fixed:
- Removing shift by 0.
  - Code cleanup. No expected changes to emulator functionality.

## 1.1.874 - 2023-06-09
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.6.1 to 17.6.2.
  - Updates the Microsoft.NET.Test.Sdk dependency. No expected changes to emulator functionality.

## 1.1.873 - 2023-06-09
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.30.1 to 6.31.0.
  - Updates the System.IdentityModel.Tokens.Jwt dependency. No expected changes to emulator functionality.

## 1.1.872 - 2023-06-08
### Fixed:
- Vulkan: Use aspect flags for identity views for bindings.
  - Fixes a regression from 1.1.805 that caused visual glitches or crashes on RADV (Mesa Vulkan drivers for AMD on Linux) in Kirby and the Forgotten Land, Mario + Rabbids Kingdom Battle, Metroid Prime Remastered, Pokémon Legends Arceus, Pokémon Scarlet/Violet, Pokémon Sword/Shield, The Legend of Zelda: Link's Awakening, Super Mario 3D World and possibly other games.

## 1.1.871 - 2023-06-08
### Fixed:
- Remove barrier on Intel if control flow is potentially divergent.
  - The Legend of Zelda: Tears of the Kingdom now runs on Intel GPUs.

## 1.1.870 - 2023-06-08
### Fixed:
- Implement soft float64 conversion on shaders when host has no support.
  - Contributes towards upstreaming the closed-source macOS changes.
  - Fixes rendering in Rune Factory 4 Special and possibly other games on macOS.
  - Fixes some ErrorDeviceLost crashes on Intel GPUs in The Legend of Zelda: Tears of the Kingdom. The game still won't run on Intel GPUs due to another issue.

## 1.1.869 - 2023-06-05
### Fixed:
- Updater: Ignore files introduced by the user in base directory.
  - Updater will no longer delete user added files in the base directory when updating.

## 1.1.868 - 2023-06-05
### Fixed:
- Fix wrong unaligned SB state when fetching compute shaders.
  - Fixes a regression caused by 1.1.861 that introduced glitches in some games on Intel GPUs.

## 1.1.867 - 2023-06-05
### Fixed:
- Fix 3D texture size when totalBlocksOfGobsInZ > 1.
  - Fixes some 3D texture issues in UE4 games.
  - Fixes crashes on newer versions of Dragon Ball Z: Kakarot, Dragon Quest X Offline and possibly other UE4 games.

## 1.1.866 - 2023-06-04
### Fixed:
- Avalonia: Adjust Grid Library alignment.
  - Game rows that aren't filled will now align left instead of center on Avalonia's grid view.

## 1.1.865 - 2023-06-04
### Fixed:
- Dont Error on Invalid Enum Values.
  - Prevents Ryujinx from deleting the Config.json file when switching between emulator versions with minor configuration differences.

## 1.1.864 - 2023-06-04
### Fixed:
- Replacing ZbcColorArray with Array4<uint>.
  - Code cleanup. No expected changes in games.

## 1.1.863 - 2023-06-04
### Fixed:
- Texture: Fix layout conversion when gobs in z is used with depth = 1.
  - Fixes gloom textures randomly breaking in The Legend of Zelda: Tears of the Kingdom, on both OpenGL and Vulkan.
  - Fixes character rendering in Spiritfarer.

## 1.1.862 - 2023-06-03
### Fixed:
- Check KeyboardMode in GUI.
  - Fixes some issues when entering a different character type than a game requests into the keyboard applet.

## 1.1.861 - 2023-06-03
### Added:
- Implement shader storage buffer operations using new Load/Store instructions.
  - Allows for the implementation of transform feedback and geometry shader emulation on macOS.

## 1.1.860 - 2023-06-03
### Fixed:
- ava: Fix Input Touch.
  - Fixes an Avalonia regression from 1.1.557 causing touch inputs to never be released.
  - Resolves issues in some games where input would no longer be accepted after a single click.

## 1.1.859 - 2023-06-03
### Fixed:
- ava: Fix Open Applet menu enabled.
  - The "open applet" menu is now inaccessible while games are running.
  - Fixes possible error where this was attempted during gameplay.

## 1.1.858 - 2023-06-03
### Fixed:
- Armeilleure: Fix support for Windows on ARM64.
  - Makes required changes to the CPU JIT to support Windows on ARM in future.

## 1.1.857 - 2023-06-03
### Fixed:
- Allow BGRA storage images on Vulkan.
  - Fixes red and blue being swapped when FXAA or SMAA are enabled in Persona 4 Golden and any other games presenting BGRA textures.

## 1.1.856 - 2023-06-03
### Fixed:
- ava: Fix exit dialog while guest is running.
  - Fixes an issue on the Avalonia UI where content dialogs would not pop up if the Ryujinx window happened to be out of focus.

## 1.1.855 - 2023-06-01
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.6.0 to 17.6.1.
  - Updates the Microsoft.NET.Test.Sdk dependency. No expected changes to emulator functionality.

## 1.1.854 - 2023-06-01
### Fixed:
- UI: Fix empty homebrew icon.
  - Avalonia will no longer crash when running a homebrew application that has no icon. 

## 1.1.853 - 2023-06-01
### Fixed:
- Fix Avalonia Library header changes size when switching between List/Grid view.
  - Adds an explicit height to the panel so that it won't grow/shrink when the "show names" checkbox is added/removed. 

## 1.1.852 - 2023-06-01
### Fixed:
- [Logger] Add print with stacktrace method.
  - Adds stacktraces to Ryujinx logs for easier debugging. 

## 1.1.851 - 2023-06-01
### Fixed:
- nuget: bump DynamicData from 7.13.8 to 7.14.2.
  - Updates the DynamicData dependency. No expected changes to emulator functionality.

## 1.1.850 - 2023-06-01
### Fixed:
- Only run one workflow for a PR at a time.
  - Any given pull request will no longer build multiple times if commits are pushed rapidly.

## 1.1.849 - 2023-06-01
### Fixed:
- Vulkan: Include DepthMode in ProgramPipelineState.
  - Fixes an issue that could cause a few additional stutters during shader compilation.

## 1.1.848 - 2023-05-31
### Fixed:
- GPU: Dispose Renderer after running deferred actions.
  - Fixes a lot more cases of Ryujinx hanging after stopping emulation/closing.

## 1.1.847 - 2023-05-31
### Fixed:
- Avalonia UI: Fix letter "x" in Ryujinx logo being cut off in About dialog + make its pronunciation center-aligned.
  - Fixes the aforementioned problems on the Avalonia "About" window.

## 1.1.846 - 2023-05-31
### Fixed:
- Skip draws with zero vertex count.
  - Fixes a crash on macOS in Marvel Ultimate Alliance 3: The Black Order.

## 1.1.845 - 2023-05-31
### Fixed:
- Share ResourceManager between vertex A and B shaders.
  - Fixes a regression from 1.1.811 that caused Borderlands 2 to crash before the title screen while generating a shader.

## 1.1.844 - 2023-05-31
### Fixed:
- Headless: MacOS Headless Fixes.
   - Properly sign headless builds so they can use Hypervisor.
   - Bundle MoltenVK in headless builds when building for macOS.
   - Force Vulkan on macOS.

## 1.1.843 - 2023-05-30
### Fixed:
- Add Context Menu Option to Run Application.
  - You can now launch games by right clicking them and selecting "Run Application" on the Avalonia UI.

## 1.1.842 - 2023-05-29
### Fixed:
- Linux: Automatically increase vm.max_map_count if it's too low.
  - Works around a Linux issue where several games, including Shin Megami Tensei V and The Legend of Zelda: Tears of the Kingdom, would segfault after exceeding the max amount of memory mappings.

## 1.1.841 - 2023-05-28
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.5.0 to 17.6.0.
  - Updates the Microsoft.NET.Test.Sdk dependency. No expected changes to emulator functionality.

## 1.1.840 - 2023-05-28
### Fixed:
- Make sure blend is disabled if render target has integer format.
  - Contributes to upstreaming the closed-source changes in the macos1 build.
  - Fixes crashes in Luigi's Mansion 3 and Xenoblade Chronicles games on macOS. Note that Xenoblade games won't work yet as they require transform feedback to run, which hasn't been upstreamed yet.

## 1.1.839 - 2023-05-28
### Fixed:
- Workaround for MoltenVK barrier issues.
  - Contributes to upstreaming the closed-source changes in the macos1 build.
  - Fixes vertex explosions in Xenoblade Chronicles games on macOS. Note that the games won't work yet as they require transform feedback to run, which hasn't been upstreamed yet.

## 1.1.838 - 2023-05-28
### Fixed:
- Fix incorrect vertex attribute format change.
  - On macOS, fixes a regression from 1.1.821 which caused Gun Gun Pixies and other games to render incorrectly.

## 1.1.837 - 2023-05-28
### Fixed:
- Allow surround sound for SDL2 in more scenarios.
  - Fixes an issue where the SDL2 audio backend would not output surround sound in certain setups.

## 1.1.836 - 2023-05-28
### Added:
- Linux: Use gamemode if it is available when using Ryujinx.sh.
  - On Linux, when using the Ryujinx.sh script to start the emulator, checks for "gamemoderun" and uses it if it exists. May improve performance on supported systems.

## 1.1.835 - 2023-05-28
### Fixed:
- Add support for VK_EXT_depth_clip_control.
  - Significantly reduces z-fighting on distant geometry in The Legend of Zelda: Tears of the Kingdom when using Vulkan.

## 1.1.834 - 2023-05-28
### Fixed:
- chore: Update Avalonia to 0.10.21 .
  - Updates the Avalonia package from version 0.10.19 to 0.10.21. No expected changes to Avalonia UI functionality.

## 1.1.833 - 2023-05-28
### Added:
- About window: Add changelog link under ver. number.
  - Adds a link to the changelog page on GitHub in the emulator's "About" tab.

## 1.1.832 - 2023-05-28
### Fixed:
- Update LastPlayed date on emulation end.
  - "Last played" stat will now use the time when the game was last closed, instead of the time when it was last launched.

## 1.1.831 - 2023-05-28
### Fixed:
- Improve macOS updater.
  - (Hopefully) fixes the remaining issues with the updater on macOS.

## 1.1.830 - 2023-05-28
### Fixed:
- Added Custom Path case when saving screenshots.
  - Fixes an issue where emulator screenshots wouldn't save to the portable folder in portable mode if Ryujinx was launched using a cli argument for --root-data-dir.

## 1.1.829 - 2023-05-28
### Fixed:
- actions: revert timeout-minutes changes for PR workflow.
  - Reverts the previous change for pull request builds.

## 1.1.827-1.1.828 - 2023-05-28
### Fixed:
- Use variables to configure job timeouts.
  - Adds a config variable so developers can more easily control timeouts on GitHub workflows.

## 1.1.826 - 2023-05-26
### Fixed:
- Ava UI: Fixes for random hangs on exit.
  - Fixes some of the hanging issues when exiting on Avalonia.

## 1.1.825 - 2023-05-26
### Fixed:
- Force reciprocal operation with value biased by constant to be precise on macOS.
  - On macOS, fixes overbright clothing items in The Legend of Zelda: Tears of the Kingdom and Quaxly's hair in Pokémon Scarlet/Violet.

## 1.1.824 - 2023-05-25
### Fixed:
- Fix resolution scaling of image operation coordinates.
  - Fixes a regression from 1.1.822 that caused flickering/graphical issues in Xenoblade Chronicles: Definitive Edition, Xenoblade 3 and possibly other games when using resolution scaling.

## 1.1.823 - 2023-05-25
### Fixed:
- Fix mod names.
  - Allows Ryuko to read mod names in log files again.

## 1.1.822 - 2023-05-25
### Fixed:
- Generate scaling helper functions on IR.
  - Separates the resolution scaling code from the shader backends so scaling behaves more similarly between OpenGL and Vulkan.
  - Makes it easier to implement more graphics backends in the future.

## 1.1.821 - 2023-05-25
### Fixed:
- Truncate vertex attribute format if it exceeds stride on MoltenVK.
  - Fixes vertex explosions in The Legend of Zelda: Tears of the Kingdom on macOS.

## 1.1.820 - 2023-05-25
### Fixed:
- Update release.yml.
  - Updates github-script so it uses Node 16 instead of Node 12.

## 1.1.819 - 2023-05-23
### Fixed:
- Vulkan: Do not set storage flag for multisample textures if not supported.
  - Fixes a crash in Dark Souls Remastered and other games that occurred on macOS when using a newer MoltenVK version.

## 1.1.818 - 2023-05-22
### Added:
- Implement p2rc, p2ri, p2rr and r2p.cc shaders.
  - Implements the aforementioned missing shader instructions. These are used in The Legend of Zelda: Tears of the Kingdom, however it's not known what they affect, or if they affect anything at all.

## 1.1.817 - 2023-05-22
### Fixed:
- Revert "Bump MVK Version to 1.2.3".
  - Reverts the previous change as it caused graphical regressions in some games while showing no noticeable benefit.

## 1.1.816 - 2023-05-22
### Fixed:
- Bump MVK Version to 1.2.3.
  - Updates the MoltenVK dependency from version 1.2.0 to 1.2.3.
  - Might fix graphical issues in some games on macOS. 

## 1.1.815 - 2023-05-21
### Fixed:
- Ava UI: Input Menu Redesign.
  - Redesigns and cleans up the input settings window on the Avalonia UI.

## 1.1.814 - 2023-05-21
### Fixed:
- Fix crash in SettingsViewModel when Vulkan isn't available.
  - Fixes a crash when opening the settings window on devices (e.g. Windows ARM) where Vulkan is not available.

## 1.1.813 - 2023-05-21
### Fixed:
- ServerBase thread safety.
  - Fixes a possible `RecyclableMemoryStreamManager` crash when closing Ryujinx.
  - Fixes some possible cases of Ryujinx hanging when stopping emulation.

## 1.1.812 - 2023-05-21
### Changed:
- Replace ShaderBindings with new ResourceLayout structure for Vulkan.
  - Will allow for a cleaner implementation of transform feedback and geometry shader emulation on macOS in future.

## 1.1.811 - 2023-05-20
### Fixed:
- Replace constant buffer access on shader with new Load instruction.
  - Fixes vertex explosions in Super Mario Sunshine and Super Mario Galaxy on AMD GPUs.

## 1.1.810 - 2023-05-20
### Fixed:
- Limit compute storage buffer size.
  - Fixes a regression that caused DOOM Eternal to crash in-game. Note that the game still isn't playable due to other issues.

## 1.1.809 - 2023-05-20
### Fixed:
- SPIR-V: Only allow implicit LOD sampling on fragment.
  - Fixes incorrect gloom hitboxes on AMD GPUs on Windows in the Depths in The Legend of Zelda: Tears of the Kingdom.

## 1.1.808 - 2023-05-19
### Fixed:
- Fix macOS Update Script.
  - Fixes the remaining issues with the autoupdater on macOS. 

## 1.1.807 - 2023-05-19
### Fixed:
- Eliminate redundant multiplications by gl_FragCoord.w on the shader.
  - Fixes black dots and lines on character models in Demon Slayer -Kimetsu no Yaiba- The Hinokami Chronicles.

## 1.1.806 - 2023-05-19
### Fixed
- nuget: bump DynamicData from 7.13.5 to 7.13.8.
  - Updates the DynamicData dependency. No changes in games.

## 1.1.805 - 2023-05-18
### Fixed:
- Fix Vulkan blit-like operations swizzle.
  - Fixes colors flashing in Omega Strikers.

## 1.1.804 - 2023-05-18
### Fixed:
- Avoid using garbage size for non-cb0 storage buffers
  - Fixes the performance loss in the Depths in The Legend of Zelda: Tears of the Kingdom.

## 1.1.803 - 2023-05-17
### Fixed:
- ava: Fix crash when extracting sections from NCA with no data section.
  - Fixes a crash on the Avalonia UI when extracting sections from NCAs that have no data section.

## 1.1.802 - 2023-05-17
### Fixed:
- Start GPU performance counter at 0 instead of host GPU value.
  - Fixes The Legend of Zelda: Tears of the Kingdom locking at 20fps in situations where it shouldn't. Note that the game may still lock to 20fps if your hardware is unable to maintain 30fps; in these environments, a 30fps mod may still be needed.

## 1.1.801 - 2023-05-17
### Fixed:
- macos: Fix relaunch with updater when no arguments were provided to the emulator.
  - Fixes macOS builds not restarting after an update if the emulator was started without arguments (for example on non-portable builds).

## 1.1.800 - 2023-05-14
### Fixed:
- [GUI] Fix always hide cursor mode not hiding the cursor until it was moved.
  - Fixes a bug on the Avalonia UI where the hide cursor mode wasn't applied correctly if the cursor wasn't moved first.

## 1.1.799 - 2023-05-13
### Fixed:
- Vulkan: Device map buffers written more than flushed.
  - Increases performance in The Legend of Zelda: Tears of the Kingdom on Nvidia GPUs* using Vulkan.
  - Significantly reduces the large performance impact of resolution scaling for Nvidia GPUs using Vulkan in the above title.

* AMD does not suffer from the same limitation that was causing the large performance losses here and should be unaffected.

## 1.1.798 - 2023-05-13
### Added:
- Add timeout of 35 minutes to workflow jobs.
  - GitHub workflows will now time out after 35 minutes instead of going on forever when they get stuck.

## 1.1.797 - 2023-05-13
### Fixed:
- audio: SDL2: Do not report 5.1 if the device doesn't support it.
  - Fixes volume being too low on SDL2 if a game was converting 5.1 to stereo sound.

## 1.1.796 - 2023-05-12
### Fixed:
- Set OpenGL PixelPackBuffer to 0 when done.
  - Fixes emulator screenshots being taken as black pictures when using OpenGL.
  - Fixes camera photos and save previews being taken as black pictures in The Legend of Zelda: Breath of the Wild and The Legend of Zelda: Tears of the Kingdom when using OpenGL.
  - May fix similar issues in other games when using OpenGL.

## 1.1.795 - 2023-05-12
### Fixed:
- macOS CI Adjustments.
  - Allows macOS mainline builds to autoupdate.
  - Removes the x86 macOS builds from Pull Request artifacts as they didn't actually work.

## 1.1.794 - 2023-05-12
### Fixed:
- Ava: Fix wrong MouseButton.
  - Fixes a random crash on the Avalonia UI that could occur upon clicking the option to go fullscreen.

## 1.1.793 - 2023-05-12
### Fixed:
- Bump shader cache version.
  - Corrects the missing cache bump in 1.1.785.
  - Fix mentioned in that version is now correctly applied to existing shader caches.

## 1.1.792 - 2023-05-11
### Fixed:
- Vulkan: Partially workaround MoltenVK InvalidResource error.
  - Adds a workaround for a MoltenVK issue where binding a storage buffer more than once with different stage flags causes resource usage to register incorrectly, which causes the command buffer to fail. No known changes in games.

## 1.1.791 - 2023-05-11
### Fixed:
- GPU: Remove swizzle undefined matching and rework depth aliasing.
  - Fixes UI textures in The Legend of Zelda: Tears of the Kingdom.

## 1.1.790 - 2023-05-11
### Fixed:
- Fix the restart after an update.
  - Fixes an issue where Ryujinx would not restart after updating, and would instead error out with "The application was unable to start correctly (0xc0000142)".

## 1.1.789 - 2023-05-11
### Fixed:
- Changed LastPlayed field from string to nullable DateTime.
  - Fixes an issue where Ryujinx would crash after stopping emulation when the system and date format was too custom (for example, when using the Holocene calendar).

## 1.1.788 - 2023-05-11
### Fixed:
- amadeus: Allow 5.1 sink output.
  - Allows Ryujinx to output 5.1 surround sound in games that support it.

## 1.1.787 - 2023-05-11
### Fixed:
- UI: Adjust input mapping view.
  - Makes the Avalonia input settings window prettier.

## 1.1.786 - 2023-05-11
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.30.0 to 6.30.1.
  - Updates the System.IdentityModel.Tokens.Jwt dependency. No changes in games.

## 1.1.785 - 2023-05-11
### Fixed:
- Enable explicit LOD for array textures with depth compare on SPIR-V.
  - Fixes some visual glitches in The Legend of Zelda: Tears of the Kingdom when using the Vulkan backend.

## 1.1.784 - 2023-05-11
### Fixed:
- Fix incorrect ASTC endpoint color when using LuminanceDelta mode.
  - Fixes white square artifacting on some textures in The Legend of Zelda: Tears of the Kingdom.

## 1.1.783 - 2023-05-11
### Fixed:
- amadeus: Fix wrong channel mapping check and an old typo.
  - Fixes broken audio in The Legend of Zelda: Tears of the Kingdom.

## 1.1.782 - 2023-05-11
### Fixed:
- Stop SDL from inhibiting sleep (#4828).
  - Fixes an issue where Ryujinx would prevent the computer from entering Sleep mode when no game would be running or when game execution was suspended.

## 1.1.781 - 2023-05-11
### Fixed:
- Fix the issue of unequal check for Amiibo file date due to the lack of sub-second units in the header.
  - The Amiibo menu now opens faster.

## 1.1.780 - 2023-05-11
### Fixed:
- GPU: Fix shader cache assuming past shader data was mapped.
  - Fixes a random crash in The Legend of Zelda: Tears of the Kingdom upon entering new areas.

## 1.1.778-1.1.779 - 2023-05-11
### Added:
- Update release workflow & Add jobs for macOS.
  - Adds macOS builds to Pull Requests and to the GitHub releases channel.

Note that **not all of the macOS-specific changes have been upstreamed yet**; you may run into issues or reduced performance that don't exist in the macos1 preview build and vice-versa.

## 1.1.777 - 2023-05-10
### Fixed:
- Ensure background translation threads exited before disposing JIT.
  - Fixes an uncommon issue where the emulator would crash on exit.

## 1.1.776 - 2023-05-10
### Fixed:
- Fix missing domain service object dispose.
  - Fixes a regression from 1.1.774 that caused Animal Crossing: New Horizons to get stuck on the loading screen after pressing "Continue".

## 1.1.775 - 2023-05-09
### Fixed:
- fix(mvk): resumeLostDevice.
  - Fixes MoltenVK crashes in Hyrule Warriors: Age of Calamity, Mario Strikers: Battle League and possibly other games.

## 1.1.774 - 2023-05-09
### Fixed:
- IPC - Refactor Bcat service to use new ipc - Revisit.
  - Redo of 1.1.749, with fixes for the affected titles. No known changes in games. 

## 1.1.773 - 2023-05-09
### Fixed:
- Replace DelegateHelper with pre-generated delegates.
  - Should allow this to be compiled with NativeAOT in the future. No expected changes in games.

## 1.1.772 - 2023-05-08
### Fixed:
- Vulkan: Pass Vk instance to VulkanRenderer
  - Allows future flexibility in driver selection where multiple could be installed.
  - No expected changes in games.

## 1.1.771 - 2023-05-08
### Fixed:
- Vulkan: Avoid hardcoding features in CreateDevice.
  - No expected changes in games.

## 1.1.770 - 2023-05-08
### Fixed:
- Vulkan: Simplify MultiFenceHolder and managing them.
  - Slightly improves performance in Vulkan backend-bottlenecked games such as The Legend of Zelda: Breath of the Wild.

## 1.1.769 - 2023-05-08
### Fixed:
- Vulkan: Batch vertex buffer updates.
  - Batches vertex buffer updates to reduce individual update calls and avoid rebinding those which have not changed.
  - Slightly improves performance in The Legend of Zelda: Breath of the Wild.

## 1.1.768 - 2023-05-07
### Fixed:
- misc: Avoid copy of ApplicationControlProperty.
  - Follow-up of 1.1.765, avoids more large copies. No expected changes in games.

## 1.1.767 - 2023-05-07
### Fixed:
- Ava: Fix SystemTimeOffset calculation.
  - Fixes a bug in the Avalonia UI where the system time offset was being incorrectly calculated.
  - Formats system settings AXAML. No expected changes in-games.

## 1.1.766 - 2023-05-07
### Fixed:
- time: Update for 15.0.0 changes and fixes long standing issues.
  - Updates the time service HLE code to match RE of firmware 15.0.0.
  - Fixes many time-related gameplay elements such as daily events and PokéJobs in Pokémon Sword/Shield.

## 1.1.765 - 2023-05-07
### Changed:
- Switch ProcessResult to a class.
  - Avoids large copies when passing or returning the ProcessResult.
  - No expected changes in games.

## 1.1.764 - 2023-05-07
### Fixed:
- UI: Expose games build ID for cheat management.
  - Shows the BID for any given game on the cheat manager so users can copy it without needing to look for it in log files or the internet.

## 1.1.763 - 2023-05-06
### Added:
- Add progress bar for re-packaging shaders.
  - After rebuilding shaders on game boot, the emulator then proceeds to recompress the shader cache files; this change adds an extra loading bar for that process. Before, it would seem as if the emulator had hanged, though this was not the case.

## 1.1.762 - 2023-05-05
### Fixed:
- AM: Stub some service calls.
  - No known changes in games.

## 1.1.761 - 2023-05-05
### Fixed:
- Shader: Use correct offset for storage constant buffer elimination.
  - Fixes a regression from 1.1.755 which increased GPU usage in some games and caused performance drops when upscaling.

## 1.1.760 - 2023-05-05
### Changed:
- Remove CPU region handle containers.
  - Memory region handles are now accessed directly instead of through the CpuRegionHandle class.
  - Improves performance in Super Mario Odyssey and other titles by up to 5%.

## 1.1.759 - 2023-05-05
### Fixed:
- Fix sections extraction.
  - Fixes a crash when attempting to use the logo extraction feature.

## 1.1.758 - 2023-05-05
### Fixed:
- Correct tooltips for add,remove,removeall buttons.
  - DLC Manager tooltips wrongly referred to updates instead of DLC.

## 1.1.757 - 2023-05-05
### Fixed:
- Fix typo in TextureBindingsManager.cs.
  - "accomodate" -> "accommodate".

## 1.1.756 - 2023-05-05
### Fixed:
- Use ToLowerInvariant when detecting GPU vendor.
  - Fixes GPU vendor detection on Turkish systems where "NVIDIA Corporation" for example would become "nvıdıa corporation" and fail the check.

## 1.1.755 - 2023-05-05
### Added:
- Allow any shader SSBO constant buffer slot and offset.
  - May help homebrew that use APIs other than NVN, such as vita2hos or emulated Mario Kart DS.

## 1.1.754 - 2023-05-05
### Fixed:
- GPU: Allow granular buffer updates from the constant buffer updater.
  - Improves performance in NieR Automata: The End of YoRHa Edition, Splatoon 2 and The Legend of Zelda: Breath of the Wild.

## 1.1.753 - 2023-05-05
### Fixed:
- ModLoader: Fix case sensitivy issues.
  - Fixes the previous issue where cheat support broke by creating new instruction lists for every new cheat instead of clearing and share the same list.

## 1.1.752 - 2023-05-05
### Fixed:
- Fix unescaped string in the Linux launch script which fails when there are spaces in directory path.

## 1.1.751 - 2023-05-04
### Fixed:
- Revert "IPC - Refactor Bcat service to use new ipc".
  - Reverts the change in 1.1.749 as it broke several games.

## 1.1.750 - 2023-05-04
### Fixed:
- UI: Move ApplicationContextMenu in a separated class.
  - Fixes an issue where shader cache would be purged regardless of your answer to the "Are you sure?" prompt.

## 1.1.749 - 2023-05-04
### Fixed:
- IPC - Refactor Bcat service to use new ipc.
  - Refactors Bcat service code. No known changes in games.

## 1.1.748 - 2023-05-03
### Fixed:
- Fix some invalid blits involving depth textures.
  - Fixes a crash on MoltenVK in AI: The Somnium Files.

## 1.1.747 - 2023-05-03
### Fixed:
- Update SettingsWindow.cs.
  - Fixes saving settings if directory path is directly pasted into the text field instead of using the file chooser.

## 1.1.746 - 2023-05-03
### Fixed:
- Revert "ModLoader: Fix case sensitivy issues".
  - Reverts the change in 1.1.744 as it broke cheat support on Windows.

## 1.1.745 - 2023-05-03
### Fixed:
- Vulkan: Record modifications for barriers after changing the framebuffer.
  - Fixes visual glitches at lower resolutions in Xenoblade Chronicles 2 on Nvidia RTX 3000 and 4000 series GPUs.
  - Fixes godrays covering the screen on AMD and Nvidia GPUs in Bayonetta 3.

## 1.1.744 - 2023-05-02
### Fixed:
- ModLoader: Fix case sensitivy issues.
  - Fixes an issue where the mod subdirectories (exefs, romfs) had to be lowercase to be recognized by Ryujinx.

## 1.1.743 - 2023-05-01
### Added:
- Add hide-cursor command line argument & always hide cursor option.
  - Adds "--hide-cursor" as a new command line argument which overrides the current settings. Possible values are: "Always", "OnIdle", "Never". The default value for the hide-cursor option will still be "Never".

## 1.1.742 - 2023-04-29
### Added:
- Keep rendered textures without any pool references alive.
  - Slightly improves performance in The Legend of Zelda: Breath of the Wild.

## 1.1.741 - 2023-04-29
### Added:
- Pre-emptively flush textures that are flushed often (to imported memory when available).
  - Improves performance in The Legend of Zelda: Breath of the Wild, The Legend of Zelda: Skyward Sword HD, Xenoblade Chronicles: Definitive Edition, Xenoblade Chronicles 2 and possibly other titles.

## 1.1.740 - 2023-04-29
### Fixed:
- Fix errors handling texture remapping.
- Fixes the following regressions from 1.1.664:
  - Bomb texture corruption while encountering Master Stalfos in The Legend of Zelda: Link's Awakening.
  - UI textures in WVSC eBASEBALL: POWER PROS.
  - White screen in Xenoblade Chronicles 3 and possibly the rainbow screen in Future Redeemed DLC.

## 1.1.739 - 2023-04-29
### Fixed:
- Signal interrupt event to improve back-end frame presentation.
  - Improves frame-time consistency and presentation in all titles.

## 1.1.738 - 2023-04-28
### Added:
- Allow window to remember its size, position and state.
  - Main window will start-up at its last opened position and state.

## 1.1.737 - 2023-04-28
### Changed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.29.0 to 6.30.0.
  - Dependency update. No functionality changes.

## 1.1.736 - 2023-04-28
### Fixed:
- Fix paths and typos for macOS scripts.
  - Resolves a bug in the macOS build script preventing compile.

## 1.1.735 - 2023-04-27
### Changed:
- Move Ryujinx projects to a new src directory.
  - Restructures the directory layout of the source code. No program changes.

## 1.1.734 - 2023-04-27
### Fixed:
- Fix geometry shader layer passthrough regression.
  - Fixes a regression from 1.1.733 which caused Pokémon Scarlet/Violet to render as if it had a green filter over the screen. This only affected older GPUs that don't support writing gl_Layer on the vertex shader (1st gen Maxwell and older for Nvidia).

## 1.1.733 - 2023-04-25
### Fixed:
- Refactor attribute handling on the shader generator.
  - Refactors shader translator code to better handle "attributes".
  - Fixes compilation failures for tessellation shaders on MoltenVK, which would cause some games to render nothing except the UI on macOS.
  - Fixes an issue on older GPUs that don't support setting gl_Layer on the vertex shader should be fixed. This allows Dragon Quest 3 and possibly other games to render on older GPUs.

## 1.1.732 - 2023-04-25
### Fixed:
- Add missing check for thread termination on ArbitrateLock.
  - Fixes one instance of the emulator freezing when you stop emulation or close it (doesn't fix the issue entirely).

## 1.1.731 - 2023-04-24
### Fixed:
- Implement DMA texture copy component shuffle.
  - Fixes inverted red and blue in 20XX and Dragon Quest Builders, possibly other games that use OpenGL on the Switch.

## 1.1.730 - 2023-04-24
### Fixed:
- Use vector transform feedback outputs with fragment shaders.
  - Works around an AMD driver regression in Pokémon Legends Arceus causing invisible characters in Vulkan and distorted models in OpenGL.

## 1.1.729 - 2023-04-24
### Fixed:
- Set the console title for GTK.
  - Fixes a regression that removed the program title from the GTK console.

## 1.1.728 - 2023-04-23
### Fixed:
- UI: Fix Amiibo issues & log errors and exceptions.
  - Fixes an issue where scanning an Amiibo would be impossible if the user's internet connection was too slow to load the API before timing out. 

## 1.1.727 - 2023-04-23
### Added:
- Reducing Memory Allocations 202303.
  - Optimizes memory allocations on various emulator tasks. No known changes in games.

## 1.1.726 - 2023-04-22
### Fixed:
- Shader: Bias textureGather instructions on AMD/Intel.
  - Apply small positive bias to textureGather to return correct texels.
  - Fixes broken shadows on grass and character models in The Legend of Zelda: Breath of the Wild on AMD/Intel GPUs.

## 1.1.725 - 2023-04-22
### Fixed:
- Removed MotionInput Calibration.
  - Fixes an issue where motion controls would re-center themselves every few seconds.

## 1.1.724 - 2023-04-20
### Fixed:
- Avoid LM service crashes by not reading more than the buffer size.
  - Fixes a crash in Rune Factory 4 Special on newlywed mode when talking with your spouse.

## 1.1.723 - 2023-04-17
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.28.1 to 6.29.0.
  - Updates the System.IdentityModel.Tokens.Jwt dependency. No changes in games.

## 1.1.722 - 2023-04-17
### Fixed:
- nuget: bump System.Management from 7.0.0 to 7.0.1.
  - Updates the System.Management dependency. No changes in games.

## 1.1.721 - 2023-04-17
### Fixed:
- Support copy between multisample and non-multisample depth textures.
  - Fixes some missing graphics in Fate/EXTELLA: The Umbral Star.

## 1.1.720 - 2023-04-16
### Changed:
- Revert "chore: Update Silk.NET to 2.17.1".
  - Reverts the change in 1.1.715 as it caused issues on self-built macOS builds.

## 1.1.719 - 2023-04-16
### Fixed:
- Vulkan: HashTableSlim lookup optimization.
  - Small Vulkan optimization. No known changes in games.

## 1.1.718 - 2023-04-16
### Fixed:
- Change SMAA filter texture clear method.
  - Fixes a crash on Intel GPUs and macOS when using SMAA.

## 1.1.717 - 2023-04-16
### Added:
- [GUI] Add network interface dropdown.
  - Adds a setting to allow choosing the network interface used for LAN games (and LDN in the future). This feature is already present on LDN builds.

## 1.1.716 - 2023-04-16
### Fixed:
- Headless: Fix a crash in Ryujinx.Headless.SDL2 when loading an app.
  - Fixes a crash caused by the recent application loader changes, should also log the PTC progress now.

## 1.1.715 - 2023-04-16
### Fixed:
- chore: Update Silk.NET to 2.17.1.
  - Updates the Silk.NET dependency. No changes in games.

## 1.1.714 - 2023-04-16
### Fixed:
- Ensure the updater doesn't delete hidden or system files.
  - Prevents the autoupdater from deleting system files in the Ryujinx folder, such as desktop.ini.

## 1.1.713 - 2023-04-16
### Fixed:
- nuget: bump DynamicData from 7.13.1 to 7.13.5.
  - Updates the DynamicData dependency. No changes in games.

## 1.1.712 - 2023-04-16
### Fixed:
- Ava: Fix nca extraction window never closing & minor cleanup.
  - Fixes an issue on Avalonia where the nca extraction window would not close after finishing.

## 1.1.711 - 2023-04-15
### Added:
- Ability to hide file types in Game List.
  - You can now choose which file types will appear on the games list under Options > Show File Types.

## 1.1.710 - 2023-04-15
### Fixed:
- Added check for eventual symlink when displaying game files.
  - Fixes an issue where Ryujinx wouldn't follow file size on symbolic links.

## 1.1.709 - 2023-04-14
### Changed:
- Rename Hipc to Cmif where appropriate.

## 1.1.708 - 2023-04-12
### Fixed:
- Avalonia - Move swkbd message null check into constructor.
  - Fixes an issue where the software keyboard in Avalonia did not populate any example text the game provides, such as default character names.

## 1.1.707 - 2023-04-11
### Fixed:
- HLE: Deal with empty title names properly.
  - Fixes a regression from 1.1.689 that caused title names to not appear under certain system languages.

## 1.1.706 - 2023-04-11
### Fixed:
- Vulkan: add situational "Fast Flush" mode.
  - Improves Vulkan performance in Bayonetta 3, Pokémon Scarlet/Violet (both only when using resolution scaling) and The Legend of Zelda: Breath of the Wild (in general).

## 1.1.705 - 2023-04-11
### Fixed:
- ARMeilleure: Move TPIDR_EL0 and TPIDRRO_EL0 to NativeContext.
  - Improves performance slightly in Pokémon Scarlet/Violet and The Legend of Zelda: Breath of the Wild.

## 1.1.704 - 2023-04-11
### Fixed:
- OpenGL: Fix OBS/Overlays again by binding FB before present.
  - Fixes a regression that caused OBS and other software to record inverted video on OpenGL.

## 1.1.703 - 2023-04-10
### Fixed:
- Avalonia - Force activate parent window before dialog is shown.
  - Fixes an issue on the Avalonia UI where content dialogs (such as the controller applet) would not spawn if the Ryujinx window happened to be minimized or out of focus.

## 1.1.702 - 2023-04-10
### Fixed:
- [GUI] Fix a NRE in GTK when disposing GLRenderer.
  - Fixes an issue where the emulator would sometimes crash if the Switch instance wasn't set up yet or an invalid file was loaded which didn't initialize it at all.

## 1.1.701 - 2023-04-10
### Fixed:
- ARMeilleure: Respect FZ/RM flags for all floating point operations.
  - Fixes random crashes when Lynels are in the vicinity and inside the Yah Rin shrine when using the scales in The Legend of Zelda: Breath of the Wild.

## 1.1.700 - 2023-04-09
### Fixed:
- Implement remaining Arm64 HINT instructions as NOP.
  - Fixes a crash on homebrew applications that use the borealis UI library.

## 1.1.699 - 2023-04-05
### Fixed:
- Eliminate boxing allocations caused by ISampledData structs.
  - Code cleanup. No changes in games.

## 1.1.698 - 2023-04-05
### Fixed:
- Vulkan: Cleanup PhysicalDevice and Instance querying.
  - Code cleanup. No changes in games.

## 1.1.697 - 2023-04-05
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.27.0 to 6.28.1
  - Updates .NET System.IdentityModel.Tokens.Jwt dependency. No changes in games.

## 1.1.696 - 2023-04-04
### Fixed:
- Use index fragment shader output when dual source blend is enabled.
  - Fixes a MoltenVK crash in Metroid Prime Remastered and possibly other games.
  - Contributes towards upstreaming the currently closed-source macOS build.

## 1.1.695 - 2023-04-04
### Fixed:
- HLE: Set ProcessResult name from NACP.
  - Fixes a regression from 1.1.689 that caused application names to not display properly on Discord statuses, Avalonia loading screens and logs.

## 1.1.694 - 2023-04-03
### Fixed:
- Fix missing string enum converters for the config.
  - Fixes issues caused by the previous change, including configurations resetting and hotkey settings not updating on Avalonia.

## 1.1.693 - 2023-04-03
### Fixed:
- Source generated json serializers.
  - Fixes some trimming warnings and condenses configuration code.
*Note: this change may reset your configuration file.

## 1.1.692 - 2023-04-01
### Fixed:
- nuget: bump DynamicData from 7.12.11 to 7.13.1.
  - Updates the DynamicData dependency. No changes in games.

## 1.1.691 - 2023-04-01
### Fixed:
- Vulkan: Separate debug utils logic from VulkanInitialization.
  - Cleans up Vulkan debug code. No changes in games.

## 1.1.690 - 2023-04-01
### Fixed:
- nuget: bump Avalonia dependencies from 0.10.18 to 0.10.19.
  - Updates the Avalonia dependencies. No changes in games.

## 1.1.689 - 2023-03-31
### Fixed:
- Refactoring of ApplicationLoader.
  - Cleans up ApplicationLoader code and contributes towards multi-process support in the future (for example, EdiZon running alongside a game).

## 1.1.688 - 2023-03-30
### Fixed:
- Fix Linux hang on shutdown.
  - Fixes an issue on Linux where the Ryujinx process would not disappear after closing it.

## 1.1.687 - 2023-03-28
### Fixed:
- Slight code refactoring.

## 1.1.686 - 2023-03-27
### Fixed:
- audout: Fix a possible crash with SDL2 when the SDL2 audio backend is dummy.
  - Fixes a crash when launching Ryujinx with SDL_AUDIODRIVER=dummy.

## 1.1.685 - 2023-03-27
### Fixed:
- Vulkan: Fix access level of extension fields and make them read-only.
  - Code cleanup. No changes to emulator functionality.

## 1.1.684 - 2023-03-26
### Fixed:
- Vulkan: Remove CreateCommandBufferPool from Vulkan Initialization.
  - Code cleanup. No changes to emulator functionality.

## 1.1.683 - 2023-03-26
### Fixed:
- Vulkan: fix broken "VK_EXT_subgroup_size_control" support check.
  - Fixes flickering graphics on AMD GPUs in Crisis Core -Final Fantasy VII- Reunion, Shin Megami Tensei V and possibly other games.

## 1.1.682 - 2023-03-26
### Fixed:
- Vulkan: Insert barriers before clears.
  - Fixes black square artifacts ("puzzle pieces") on Vulkan, on Nvidia RTX 3000-4000 GPUs running driver version 522.25 or newer, in Mario Kart 8 Deluxe, Xenoblade Chronicles: Definitive Edition, Xenoblade 2 and Xenoblade 3.

## 1.1.681 - 2023-03-24
### Fixed:
- sdl2: Update to Ryujinx.SDL2-CS 2.26.3.
  - Fixes infinite loop shutting down WGI controllers.
  - Fixes centering the D-pad on some Xbox controllers.
  - Allows some copycat DualShock 4 controllers and possibly other gamepads to work natively on Ryujinx.

## 1.1.680 - 2023-03-24
### Fixed:
- Batch inline index buffer update.
  - Fixes low Nvidia Vulkan performance in Genkai Tokki Moero Crystal H, La-Mulana and possibly other games that use the OpenGL API on the Switch.
  - Improves OpenGL performance in the same games for other GPU vendors.

## 1.1.679 - 2023-03-24
### Fixed:
- Update short cache textures if modified.
  - Fixes a regression from 1.1.566 that broke rendering in Sonic Colors: Ultimate.

## 1.1.678 - 2023-03-24
### Fixed:
- Fix handle leak on IShopServiceAccessServerInterface.CreateServerInterface.
  - Fixes a crash in SD Shin Kamen Rider Rumble.

## 1.1.677 - 2023-03-22
### Fixed:
- ARMeilleure: Check for XSAVE cpuid flag for AVX{2,512}.
  - Fixes an issue introduced in 1.1.673 where the emulator would crash if running on an extremely old CPU.

## 1.1.676 - 2023-03-22
### Added:
- CI: add a version tag to correlate release versions with commits.
  - Main releases on GitHub will now have tags that link to their respective commits.

## 1.1.675 - 2023-03-21
### Fixed:
- Revert "Use source generated json serializers in order to improve code trimming" #4576.
  - Reverts the previous change. Fixes issues it caused such as configuration files not being parsed properly and games not booting.

## 1.1.674 - 2023-03-21
⚠️ This version does not work properly, do not update to it. ⚠️ 
### Changed:
- Use source generated json serializers in order to improve code trimming.

## 1.1.673 - 2023-03-20
### Fixed:
- ARMeilleure: Add initial support for AVX512 (EVEX encoding) (cont).
  - Redo of 1.1.478 with added fixes for the black screen issues it caused.

## 1.1.672 - 2023-03-19
### Fixed:
- Vulkan: Migrate buffers between memory types to improve GPU performance.
  - Greatly improves Nvidia Vulkan performance in Bayonetta 3, Blue Reflection: Second Light, Catherine Full Body, Ghost 'n Goblins Resurrection, Hyrule Warriors: Age of Calamity, Monster Hunter Rise, NieR Automata: The End of YoRHa Edition, Persona 5 Royal, Shin Megami Tensei V, Sonic Frontiers, Subnautica, Xenoblade Chronicles: Definitive Edition, Xenoblade 2, Xenoblade 3 and possibly other games.

## 1.1.671 - 2023-03-19
### Fixed:
- Remove MultiRange Min/MaxAddress and rename GetSlice to Slice.
  - Code cleanup. No changes in games.

## 1.1.670 - 2023-03-19
### Fixed:
- Avoid copying more handles than we have space for.
  - Fixes a regression from 1.1.668 that caused crashes in Fire Emblem Engage.

## 1.1.669 - 2023-03-18
### Fixed:
- OpenGL: Fix inverted conditional for counter flush from #4471.
  - Fixes a regression from 1.1.662 that caused OpenGL to time out and softlock in Mario Kart 8 Deluxe and other games.

## 1.1.668 - 2023-03-17
### Fixed:
- Reducing memory allocations.
  - Speeds up boot times by a few seconds on Metroid Prime Remastered and likely other games.

## 1.1.667 - 2023-03-17
### Fixed:
- Update syscall capabilites to include SVCs from FW 15.0.0.
  - Allows the homebrew menu (using hbl.nsp) to boot again.

## 1.1.666 - 2023-03-17
### Fixed:
- nuget: bump UnicornEngine.Unicorn from 2.0.2-rc1-f7c841d to 2.0.2-rc1-fb78016.
  - Updates the UnicornEngine.Unicorn dependency. No changes in games.

## 1.1.665 - 2023-03-14
### Fixed:
- GPU: Fast path for adding one texture view to a group.
  - Greatly improves loading screens and fixes open zone getting stuck at ~3fps in Sonic Frontiers.
  - May improve loading times in other games.

## 1.1.664 - 2023-03-14
### Fixed:
- Update range for remapped sparse textures instead of recreating them.
  - Significantly reduces stuttering when going through doors in Metroid Prime Remastered.
  - Greatly reduces FIFO% in NieR Automata: The End of YoRHa Edition when travelling between areas.
  - May improve texture streaming stutters in other games.

## 1.1.663 - 2023-03-14
### Fixed:
- Ava UI: DownloadableContentManager Refactor.
  - Refactors the Avalonia DLC manager and makes it consistent with the title update manager.

## 1.1.662 - 2023-03-12
### Fixed:
- GPU: Scale counter results before addition.
  - Fixes resolution scaling in WarioWare: Get It Together and Wreckfest.

## 1.1.661 - 2023-03-12
### Fixed:
- Increase access permissions for AvaloniaList<Timezone>.
  - Fixes a regression from 1.1.513 that caused the timezone list to not show.

## 1.1.660 - 2023-03-12
### Fixed:
- Misc: Support space in path on macOS distribution.
  - Fixes build errors on macOS when spaces are present in the file path.

## 1.1.659 - 2023-03-12
### Fixed:
- [Flatpak] Beautify multiline strings again & Add full git commit hash.
  - Code cleanup. No changes to emulator functionality.

## 1.1.658 - 2023-03-11
### Fixed:
- misc: Some dependencies cleanup.
  - Removes unused dependencies.

## 1.1.657 - 2023-03-11
### Fixed:
- Misc performance tweaks.
  - Minor code optimizations. No noticeable changes.

## 1.1.652-1.1.656 - 2023-03-11
### Fixed:
- [Flatpak] Add release github workflow.
  - Flathub builds will now update again.

## 1.1.651 - 2023-03-08
### Fixed:
- CPU: Avoid argument value copies on the JIT.
  - JIT optimizations and refactoring. May result in a minor performance improvement.

## 1.1.650 - 2023-03-04
### Fixed:
- nuget: bump Microsoft.CodeAnalysis.CSharp from 4.4.0 to 4.5.0.
  - Updates the Microsoft.CodeAnalysis.CSharp dependency. No changes in games.

## 1.1.649 - 2023-03-04
### Fixed:
- Minor code formatting.

## 1.1.648 - 2023-03-04
### Fixed:
- nuget: bump UnicornEngine.Unicorn from 2.0.2-rc1-a913199 to 2.0.2-rc1-f7c841d.
  - Updates the UnicornEngine.Unicorn dependency. No changes in games.

## 1.1.647 - 2023-03-01
### Fixed:
- Update LibHac to 0.18.0.
  - Fixes a regression where the emulator wouldn't create a BCAT save if any other BCAT save already existed, throwing a "ResultFsTargetNotFound (2002-1002)" error when attempting to open the BCAT save directory.
  - Loading personalized ticket title keys is now supported with the right console keys dumped.

## 1.1.646 - 2023-02-27
### Fixed:
- Sockets: Properly convert error codes on MacOS.
  - Changes sockets error codes on macOS accordingly rather than using the same error codes as Windows and Linux.
  - Defaults IsDhcpEnabled to true when interfaceProperties.DhcpServerAddresses is not available.
  - Contributes towards upstreaming the currently closed-source macOS build.

## 1.1.645 - 2023-02-27
### Added:
- Add Support for Post Processing Effects.
  - Adds FXAA and SMAA post processing options in graphics settings.
  - Adds bilinear, nearest and FSR (1.0) upscaling options in graphics settings.

## 1.1.644 - 2023-02-26
### Fixed:
- Vulkan: Support list topology primitive restart.
  - Fixes broken sand in The Legend of Zelda: Skyward Sword HD on Vulkan. (Will not affect MoltenVK as it does not support this extension.) 

## 1.1.643 - 2023-02-25
### Changed:
- Logging: Redirect StdErr into logging system.
  - Allows for easier MoltenVK/Mesa debugging since StdErr will now show up in log files. No changes in games.

## 1.1.642 - 2023-02-25
### Fixed:
- Add missing DefineConstants definition in Ryujinx.Common.
  - Fixes a project file bug that was preventing FlatPak and nixpkgs releases from building.

## 1.1.641 - 2023-02-25
### Added:
- macos: Add updater support.
  - Adds an external updater script into macOS release packages.
  - No changes for current macOS releases. Will become more useful once upstreaming is complete.

## 1.1.640 - 2023-02-25
### Fixed:
- Update OpenTK to 4.7.7
  - Bumps OpenTK dependency to version 4.7.7. No changes expected in games.

## 1.1.639 - 2023-02-25
### Fixed:
- Move gl_Layer to vertex shader if geometry is not supported.
  - Allows certain UE4 games such as Shin Megami Tensei V to render on macOS.
  - Contributes to the upstreaming of the closed-source macOS branch.

## 1.1.638 - 2023-02-25
### Fixed:
- Perform bounds checking before list indexer to avoid frequent exceptions.
  - Reduces ArgumentOutOfRangeExceptions and performance dips in VS console output and debug builds. No changes in games.

## 1.1.637 - 2023-02-23
### Fixed:
- Account for multisample when calculating render target size hint.
  - Fixes a regression from 1.1.605 that caused graphical and/or upscaling issues in Bubble Bobble, Fate/EXTELLA: The Umbral Star, Pokémon Mystery Dungeon: Rescue Team DX and Rune Factory 5.

## 1.1.636 - 2023-02-22
### Fixed:
- Ava: Fix Title Update Manager not selecting the right update.
  - Fixes an issue on Avalonia that would disable game updates after opening the update manager and not re-selecting an update.

## 1.1.635 - 2023-02-22
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.4.1 to 17.5.0.
  - Updates the Microsoft.NET.Test.Sdk dependency. No changes in games.

## 1.1.634 - 2023-02-22
### Fixed:
- nuget: bump UnicornEngine.Unicorn from 2.0.2-rc1-9c9356d to 2.0.2-rc1-a913199.
  - Updates the UnicornEngine.Unicorn dependency. No changes in games.

## 1.1.633 - 2023-02-22
### Fixed:
- Ava: Fix Linux updater crashing when tarStream is null.
  - Fixes a regression that caused the Avalonia autoupdater to crash on Linux.

## 1.1.632 - 2023-02-21
### Fixed:
- Add copy dependency for some incompatible texture formats.
  - Fixes vertical stripes in Mario + Rabbids Sparks of Hope.

## 1.1.631 - 2023-02-21
### Fixed:
- misc: changes base application directory behaviour.
  - Allows changing base application directory behaviour at build time via FORCE_EXTERNAL_BASE_DIR. Required by nixpkgs and flathub.
  - Contributes towards upstreaming the currently closed-source macOS build.

## 1.1.630 - 2023-02-21
### Fixed:
- Move Ryujinx Folder from ~/.config to ~/Library/Application Support on macOS.
  - Moves the Ryujinx folder to make it more consistent with other apps on macOS. Data will automatically migrate from the old path to the new one. 

## 1.1.629 - 2023-02-21
### Fixed:
- Use SIMD acceleration for audio upsampler.
  - Doubles audio upsampling speed on x64 hardware.

## 1.1.628 - 2023-02-21
### Fixed:
- Memory: Faster Split for NonOverlappingRangeList.
  - Reduces asset streaming stutters in Xenoblade Chronicles 2.

## 1.1.627 - 2023-02-21
### Fixed:
- Mark texture as modified and sync on I2M fast path.
  - Fixes a regression from 1.1.233 that caused graphical issues in Tanuki Justice.

## 1.1.626 - 2023-02-19
### Added:
- Add support for advanced blend (part 1/2).
  - Fixes transparency issues in Mario Party Superstars and ScreamPark selection screen in Luigi's Mansion 3 on Nvidia GPUs.

## 1.1.625 - 2023-02-17
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.26.1 to 6.27.0.
  - Updates the System.IdentityModel.Tokens.Jwt dependency. No changes in games.

## 1.1.624 - 2023-02-16
### Fixed:
- Clear CPU side data on GPU buffer clears.
  - Fixes the black screen in Mario + Rabbids Sparks of Hope.

## 1.1.623 - 2023-02-16
### Fixed:
- Validate dimensions before creating texture.
  - Fixes a regression introduced in 1.1.615 that caused Super Smash Bros. Ultimate to crash when using ARCropolis mods.

## 1.1.622 - 2023-02-15
### Fixed:
- GUI: Small Updater refactor & Set correct permissions on Linux when extracting files.
  - Fixes a Linux issue where the Ryujinx executable wouldn't work after using the autoupdater.

## 1.1.621 - 2023-02-15
### Fixed:
- Vulkan: Respect VK_KHR_portability_subset vertex stride alignment.
  - No known changes in games.

## 1.1.620 - 2023-02-15
### Fixed:
- Vulkan: Clean up MemoryAllocator.
  - Avoid querying GPU memory properties at allocation time. No known changes in games. 

## 1.1.619 - 2023-02-13
### Fixed:
- Vulkan: Enforce Vulkan 1.2+ at instance API level and 1.1+ at device level.
  - In some cases, ensures the emulator won't try to initialize Vulkan on an incompatible graphics device. No changes in games.

## 1.1.618 - 2023-02-13
### Fixed:
- Vulkan: Do not call vkCmdSetViewport when viewportCount is 0.
  - Fixes some Vulkan validation errors. No known changes in games.

## 1.1.617 - 2023-02-12
### Fixed:
- Fix partial updates for textures.
  - Fixes random texture corruption in Tony Hawk's Pro Skater 1 + 2 and possibly other games.

## 1.1.616 - 2023-02-10
### Fixed:
- Better filtering of invalid NPadIds when games request unknown supported players.
  - Allows Kingdom Rush to boot.

## 1.1.615 - 2023-02-10
### Fixed:
- Allow partially mapped textures with unmapped start.
  - Fixes broken lighting and fog in Metroid Prime Remastered.

## 1.1.614 - 2023-02-08
### Fixed:
- Fix SPIR-V when all inputs/outputs are indexed.
  - Fixes a crash in Metroid Prime Remastered on Vulkan.

## 1.1.613 - 2023-02-08
### Fixed:
- ObjectiveC Helper Class.
  - Avalonia code cleanup. No changes to the UI functionality itself.

## 1.1.612 - 2023-02-08
### Added:
- Log shader compile errors with Warning level.
  - Shows OpenGL shader compilation errors as Warning log messages in the logging console. No changes in games.

## 1.1.611 - 2023-02-08
### Fixed:
- Replace unicorn bindings with Nuget package.
  - Improves the code for tests. No changes in games.

## 1.1.610 - 2023-02-08
### Fixed:
- Vulkan: Flush command buffers for queries less aggressively.
  - Improves Vulkan performance in Xenoblade Chronicles 2.

## 1.1.609 - 2023-02-08
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.26.0 to 6.26.1.
  - Updates the System.IdentityModel.Tokens.Jwt dependency. No changes in games.

## 1.1.608 - 2023-02-08
### Fixed:
- Replace BitConverter.ToString(bytes).Replace("-", "") with Convert.ToHexString(bytes).
  - Code cleanup. No changes in games.

## 1.1.607 - 2023-02-08
### Fixed:
- Fix some Vulkan validation errors.
  - Code cleanup. No expected changes in games.

## 1.1.606 - 2023-02-08
### Fixed:
- Limit texture cache based on total texture size.
  - Implements a better solution than 1.1.538 for its problem.
  - Fixes a regression from the aforementioned change that spawned noise graphics on the title menu in River City Girls Zero.

## 1.1.605 - 2023-02-08
### Fixed:
- Handle mismatching texture size with copy dependencies.
  - Fixes rendering in Disgaea 6: Defiance of Destiny, The Longest Five Minutes and void tRrLM().
  - Fixes nuclear rainbow terrain when using extra stamina food or Master Cycle Zero in The Legend of Zelda: Breath of the Wild.
  - Fixes nuclear rainbow tracks on AMD GCN/Polaris graphics cards (such as RX 400/500 series) in Mario Kart 8 Deluxe.
  - Might fix random texture corruption in other games.

## 1.1.604 - 2023-02-07
### Fixed:
- Ava: Add ChangeVSyncMode() call to render loop.
  - Makes render loop behave the same as in GTK.
  - Fixes cases of screen tearing that would occur while running games on the Avalonia builds but wouldn't occur on GTK.
  - Might fix screen tearing on macOS.

## 1.1.603 - 2023-02-07
### Fixed:
- Support safe blit on non-2D textures.
  - Fixes more cases of AMD GPUs crashing on higher resolutions in Fire Emblem Engage.

## 1.1.602 - 2023-02-06
### Fixed:
- Accelerate NVDEC VIC surface read/write and colorspace conversion with Arm64 HW intrinsics.
  - Contributes towards upstreaming the currently closed-source macOS build.

## 1.1.601 - 2023-02-05
### Fixed:
- Implement safe depth-stencil blit using stencil export extension.
  - Fixes AMD graphics cards crashing on non-native resolutions in Fire Emblem Engage, Pokémon Scarlet/Violet, Pokémon Sword/Shield, The Legend of Zelda: Link's Awakening, Splatoon 3, Yu-Gi-Oh! Rush Duel: Dawn of the Battle Royale and possibly other games.
  - Fixes low performance on AMD GPUs in Pokémon Scarlet/Violet, Pokémon Sword/Shield and possibly other games.

## 1.1.600 - 2023-02-05
### Fixed:
- Insert bitcast for assignment of fragment integer outputs on GLSL.
  - Fixes a regression from 1.1.549 that broke rendering on OpenGL in several games, including Kirby and the Forgotten Land and Luigi's Mansion 3.

## 1.1.599 - 2023-02-01
### Fixed:
- Implement Account LoadOpenContext.
  - Fixes some multi-game collections that would crash after launching one of the games, such as Prinny Presents NIS Classics Volume 1: Phantom Brave: The Hermuda Triangle Remastered / Soul Nomad & the World Eaters.

## 1.1.598 - 2023-01-30
### Fixed:
- nuget: bump SharpZipLib from 1.4.1 to 1.4.2.
  - Updates the SharpZipLib package version. No changes to emulator functionality.

## 1.1.597 - 2023-01-29
### Added:
- Initial Apple Hypervisor based CPU emulation.
  - Contributes towards upstreaming the currently closed-source macOS build.
  - Allows games to run ARM code natively on Apple Silicon using the Apple Hypervisor framework. Greatly improves performance on Apple Silicon. 

## 1.1.596 - 2023-01-26
### Fixed:
- Relax Vulkan requirements.
  - Redo of 1.1.551 with fixes for the issues that appeared last time, such as broken lighting in several games.

## 1.1.595 - 2023-01-24
### Fixed:
- Vulkan: Reset queries on same command buffer.
  - Contributes towards upstreaming the currently closed-source macOS build.
  - Fixes some visual issues in A Hat in Time and Super Mario Odyssey on macOS.

## 1.1.594 - 2023-01-23
### Fixed:
- Remove use of GetFunctionPointerForDelegate to get JIT cache function pointer.
  - Optimizes JIT code. No known changes in games.

## 1.1.593 - 2023-01-23
### Fixed:
- SPIR-V: Change BitfieldExtract and BitfieldInsert for SPIRV-Cross.
  - Contributes towards upstreaming the currently closed-source macOS build.

## 1.1.592 - 2023-01-22
### Added:
- Add option to register file types.
  - Under tools > manage file types, added options to associate or disassociate Nintendo Switch file extensions with Ryujinx, so that when an nsp or xci file is double-clicked, it opens that game on Ryujinx. Note that this feature doesn't currently work.

## 1.1.591 - 2023-01-22
### Fixed:
- Handle parsing of corrupt Config.json and prevent crash on launch.
  - Fixes a crash on boot that could occur if the config.json file was corrupted or invalid.

## 1.1.590 - 2023-01-22
### Fixed:
- Arm64: Simplify TryEncodeBitMask and use for constants.
  - Improves performance very slightly on ARM64 systems.

## 1.1.589 - 2023-01-22
### Fixed:
- AvaloniaKeyboardDriver: Swallow TextInput events to avoid bell on macOS.
  - Prevents macOS from spamming the bell sound when playing with a keyboard.  

## 1.1.588 - 2023-01-21
### Fixed:
- Allow setting texture data from 1x to fix some textures resetting randomly.
  - Fixes resolution scaling in Cruis'n Blast, Deltarune, Fire Emblem Engage, Monopoly Madness, My Hero One's Justice 2, Pokémon Brilliant Diamond/Shining Pearl, Pokémon Mystery Dungeon: Rescue Team DX, Rune Factory 5, The Stanley Parable: Ultra Deluxe and possibly other games. 

## 1.1.587 - 2023-01-21
### Fixed:
- Ava UI: Various Fixes. 
  - Fixes screenshot functionality not working on Avalonia since 1.1.532.
  - Fixes an issue where game updates would not be removed properly from the JSON file if an update file was moved or renamed.
  - Cleans up some more Ava code.

## 1.1.586 - 2023-01-21
### Fixed:
- Remove use of reflection on GAL multithreading.
  - Code improvement, required for NativeAOT support. No known changes in games.

## 1.1.585 - 2023-01-21
### Fixed:
- nuget: bump Microsoft.CodeAnalysis.Analyzers from 3.3.3 to 3.3.4.
  - Updates Microsoft.CodeAnalysis.Analyzers. No changes to emulator functionality.

## 1.1.584 - 2023-01-21
### Fixed:
- Use volatile read/writes for GAL threading.
  - Contributes towards upstreaming the currently closed-source macOS build.
  - Fixes some instances of random crashing on ARM64 macOS.

## 1.1.583 - 2023-01-21
### Fixed:
- Implement CSET and CSETP shader instructions.
  - Fixes low detail texture bug in Persona 4 Golden.

## 1.1.582 - 2023-01-21
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.25.1 to 6.26.0.
  - Updates System.IdentityModel.Tokens.Jwt. No changes to emulator functionality.

## 1.1.581 - 2023-01-20
### Added:
- Ava UI: Add Notifications and Cleanup.
  - Adds in-app notifications to the Avalonia UI.
  - Disables "open save directory" options when the given directory does not exist.

## 1.1.580 - 2023-01-20
### Fixed:
- Ava UI: Fix string.Format issues in Locale.
  - Fixes an issue where some windows wouldn't open if UI language was not set to English on Avalonia.

## 1.1.579 - 2023-01-20
### Fixed:
- Catch Profile.json parse to prevent crash on launch.
  - Fixes a crash on boot that could occur if the profiles.json file was corrupted or invalid.

## 1.1.578 - 2023-01-20
### Added:
- Ava UI: Add Control+Cmd+F HotKey for Mac OS.
  - Adds additional hotkey to toggle full screen by pressing ^+⌘+F on macOS.

## 1.1.577 - 2023-01-20
### Added:
- Implement PCM24 output.
  - Allows audio devices that do not support PCM16, PCM32 or float output to have PCM24 and PCM8 as possible output options.

## 1.1.576 - 2023-01-20
### Fixed:
- Ava UI: Fixes and cleanup Updater.
  - Denying an emulator update after startup will no longer disable the option to check for updates on Avalonia.

## 1.1.575 - 2023-01-19
### Fixed:
- Vulkan: Destroy old swapchain on swapchain recreation.
  - Fixes an issue where the image would freeze on AMD graphics cards when exiting fullscreen.
  - Fixes a memory leak on macOS when resizing the window.

## 1.1.574 - 2023-01-18
### Fixed:
- Vulkan: Explicitly enable precise occlusion queries.
  - Contributes towards upstreaming the currently closed-source macOS build.
  - Fixes ink collision in Splatoon 2 and Splatoon 3 on MoltenVK.

## 1.1.573 - 2023-01-18
### Fixed:
- NativeSignalHandler: Fix write flag.
  - Allows Mario Kart 8 Deluxe to use resolution scaling on ARM64 macOS.
  - May improve Linux performance in some situations.

## 1.1.572 - 2023-01-18
### Fixed:
- Optimize string memory usage. Use Spans and StringBuilders where possible.
  - Code cleanup. No changes in games.

## 1.1.571 - 2023-01-18
### Fixed:
- HOS: Load RomFs by pid.
  - Allows separate processes to use separate romfs. No known changes in games.

## 1.1.570 - 2023-01-17
### Fixed:
- CPU: Fix NRE when disposing AddressSpace with 4KB pages support.
  - Fixes an issue from 1.1.568 where the emulator crashed upon stopping emulation. (Note that it can still hang on closing certain games as that's a different issue.)

## 1.1.569 - 2023-01-16
### Fixed:
- ConfigurationState: Default to Vulkan on macOS.
  - Sets graphics backend to Vulkan on macOS as the default setting.

## 1.1.568 - 2023-01-16
### Fixed:
- Implement support for page sizes > 4KB.
  - Contributes towards upstreaming the currently closed-source macOS build.
  - Games can now run without Rosetta on Apple Silicon using a master macOS build.

## 1.1.567 - 2023-01-16
### Fixed:
- Ava UI: Readd some infos to the GameList.
  - Adds missing title ID and file extension information to the games list on the Avalonia UI.

## 1.1.566 - 2023-01-16
### Fixed:
- Add short duration texture cache.
  - Improves performance in Fire Emblem: Three Houses and Hyrule Warriors: Age of Calamity, and possibly other Warriors/Musou games.

## 1.1.565 - 2023-01-15
### Fixed:
- Ava: Fix Linux Vulkan renderer regression.
  - Fixes a regression from 1.1.563 that caused Vulkan to crash on Linux when using Avalonia.

## 1.1.564 - 2023-01-15
### Fixed:
- UI: Fixes GTK sorting regression of #4294.
  - Fixes a regression from 1.1.562 that caused crashing on the GTK UI.

## 1.1.563 - 2023-01-15
### Fixed:
- Ava UI: Renderer refactoring.
  - Refactor and cleanup everything related with Renderer hosting. Should not affect rendering behaviour.

## 1.1.562 - 2023-01-15
### Fixed:
- UI: Fix applications times.
  - Makes play times easier to read.

## 1.1.561 - 2023-01-15
### Fixed:
- Specify image view usage flags on Vulkan.
  - Fixes black screen on Nvidia in games using multisample sRGB textures, such as Pinball FX3 and Sphinx and the Cursed Mummy.

## 1.1.560 - 2023-01-15
### Fixed:
- Implement missing service calls in pm.
  - Allows EdiZon NRO to boot.

## 1.1.559 - 2023-01-15
### Fixed:
- Ava UI: TitleUpdateWindow Refactor.
  - Redesigns the Avalonia title update manager and cleans up its code.

## 1.1.558 - 2023-01-15
### Fixed:
- Audren: Implement polyphase upsampler.
  - Improves accuracy of the upsampler implementation. No known changes in games.

## 1.1.557 - 2023-01-14
### Fixed:
- Ava UI: Fixes "Hide Cursor on Idle" for Windows.
  - Fixes an issue with the cursor not being hidden properly on the Avalonia UI.

## 1.1.556 - 2023-01-14
### Fixed:
- Change GetPageSize to use Environment.SystemPageSize.
  - Allows the emulator to detect the correct page size for the system it's running on.
  - Required for Ryujinx to work on Asahi Linux in the future.

## 1.1.555 - 2023-01-14
### Fixed:
- Fix texture flush from CPU WaitSync regression on OpenGL.
  - Fixes a regression from 1.1.554 that caused Super Mario Odyssey and other games to crash on OpenGL.

## 1.1.554 - 2023-01-13
### Fixed:
- Fix NRE when MemoryUnmappedHandler is called for a destroyed channel.
  - Fixes random crashes in EVE ghost enemies. Possibly a regression from 1.1.361.

## 1.1.553 - 2023-01-13
### Fixed:
- Fix texture modified on CPU from GPU thread after being modified on GPU not being updated.
  - Fixes missing textures in EVE ghost enemies, and possibly other games.

## 1.1.552 - 2023-01-13
### Fixed:
- Revert "Relax Vulkan requirements (#4228)".
  - Reverts the previous change as it caused rendering issues on some games.

## 1.1.551 - 2023-01-13
### Changed:
- Relax Vulkan requirements.
  - The emulator will now request certain graphics features optionally, making it possible to run on more GPUs.
  - Allows some games to work on the Raspberry Pi 4, albeit at non-playable speeds.

## 1.1.550 - 2023-01-12
### Fixed:
- Prepo: Fix SaveSystemReport and SaveSystemReportWithUser IPC definitions.
  - Fixes a small regression from 1.1.519. No known changes in games.
  - Fixes indexes args issue from #4188 (1.1.506).

## 1.1.549 - 2023-01-12
### Added:
- Vulkan: Add workarounds for MoltenVK.
  - Adds MoltenVK configuration class and several workarounds for graphics features not supported by macOS. 
  - Contributes towards upstreaming the currently closed-source macOS build.
  - Allows Mario Kart 8 Deluxe and many other titles to go in-game on Mac. On M1 systems, Rosetta is required for now.

## 1.1.548 - 2023-01-12
### Fixed:
- Ava UI: Reorder settings of Resolution Scaler.
  - "Custom" is now at the bottom of the list.

## 1.1.547 - 2023-01-12
### Fixed:
- Ava UI: Various Fixes.
  - Fixes an error from 1.1.540 that could occur when switching profiles and clicking "Manage saves".
  - Save manager file sizes will be displayed in KB and GB when needed.
  - Other minor adjustments.

## 1.1.546 - 2023-01-12
### Fixed:
- Ava UI: Settings Adjustments.
  - Fixes log settings ordering.
  - Other minor adjustments.

## 1.1.545 - 2023-01-12
### Added:
- PTC: Check process architecture.
  - PPTC files will be disregarded if they are for different CPU architectures (for example, PTC compiled on an x86 PC cannot be transferred to an M1 Mac).

## 1.1.544 - 2023-01-12
### Added:
- ARM64: CPU feature detection.
  - Required to determine what CPU features can be used by the emulator on ARM. No changes in games.

## 1.1.543 - 2023-01-12
### Fixed:
- lm: Handle Tail flag in LogPacket.
  - Fixes logs related to the lm service. No changes in games.

## 1.1.542 - 2023-01-11
### Fixed:
- Ava UI: Move Ava logging to Logger.Debug.
  - Avalonia UI logs (see 1.1.523) will now only show if debug logs are enabled, reducing console spam.

## 1.1.541 - 2023-01-11
### Fixed:
- Ava UI: Fixes PerformanceCheck condition.
  - Fixes "Graphics shader dump path" warning prompt not disabling the setting when the user clicked "Yes" to disable it.

## 1.1.540 - 2023-01-11
### Fixed:
- Ava GUI: User Profile Manager + Other Fixes.
  - Redesigns the user profiles manager on Avalonia and cleans up its code.

## 1.1.539 - 2023-01-10
### Fixed:
- Ava: Fixes Update count in heading.
  - Number of available updates in the title update manager has been corrected.

## 1.1.538 - 2023-01-10
### Fixed:
- Remove textures from cache on unmap if not mapped and modified. 
  - Fixes abnormally high VRAM usage in Eiyuden Chronicle: Rising and Witch's Garden.

## 1.1.537 - 2023-01-10
### Fixed:
- ava: Generate Locale menu automatically.
  - Generates Avalonia UI language selection menu automatically in order to make adding new languages easier. 

## 1.1.536 - 2023-01-10
### Added:
- Implement JIT ARM64 backend.
  - Adds ARM64 target support to the JIT, allowing it to work on ARM CPUs. 
  - Contributes towards upstreaming the currently closed-source macOS build.
  - Paves the way for Ryujinx to run on other ARM devices in the future.

## 1.1.535 - 2023-01-10
### Added:
- Set LSApplicationCategoryType to games.
  - On the next macOS release, automatically adds Ryujinx to the Launchpad games folder. 

## 1.1.534 - 2023-01-10
### Fixed:
- Ava GUI: Fix Context Menu Locales.
  - Corrects device save and BCAT save directory option text. 

## 1.1.533 - 2023-01-10
### Fixed:
- misc: Enforce LF.
  - Improves Ryujinx.sh script handling. 

## 1.1.532 - 2023-01-10
### Fixed:
- ava: Cleanup AppHost.
  - Cleans up AppHost file code. 

## 1.1.531 - 2023-01-10
### Fixed:
- Ava: Add missing null check to ContentDialogHelper.ShowAsync().
  - Fixes a regression that caused the Avalonia UI to crash on the Steam Deck.

## 1.1.530 - 2023-01-09
### Added:
- Add command line arguments to override docked mode.
  - Adds "--docked-mode" and "--handheld-mode" command line arguments for both GUIs to override the state of EnableDockedMode.

## 1.1.529 - 2023-01-09
### Fixed:
- Fix linux packaging step for CI & Add headless build support to Ryujinx.sh.
  - Fixes the previous version not building.

## 1.1.528 - 2023-01-09
### Added:
- Linux: Add Avalonia detection to Ryujinx.sh.
  - Adds a small check to Ryujinx.sh to figure out if Ryujinx or Ryujinx.Ava needs to be executed.

## 1.1.527 - 2023-01-09
### Fixed:
- Replace tabs with spaces across the project. 

## 1.1.526 - 2023-01-09
### Fixed:
- headless: Change window icon size to 48x48.

## 1.1.525 - 2023-01-08
### Added:
- [Headless] Add missing arguments & Fix typos.
  - Adds "--macro-hle", "--hide-cursor", "--profile" and "--root-data-dir" to headless build arguments.
  - Inverts several command line options i.e. EnableInternetAccess is now DisableInternetAccess for the non-default option.
  - Adds a window icon.

## 1.1.524 - 2023-01-08
### Fixed:
- ava: Fixes regressions from refactoring. 
- Fixes the following regressions on the Avalonia GUI:
  - Right click context menu on games now works again.
  - "Stop emulation" menu item now works again.
  - The UI will now overlay properly over games again. 

## 1.1.523 - 2023-01-08
### Fixed:
- Ava: Make Avalonia use our logging system.
  - Avalonia logs will now be logged by Ryujinx.

## 1.1.522 - 2023-01-08
### Fixed:
- Ava GUI: Fix long selection bar broken in #4178 (1.1.520).

## 1.1.521 - 2023-01-08
### Fixed:
- HIPC: Fix reply possibly also receiving one request.
  - Fixes an issue from 1.1.506. No known changes in games.

## 1.1.520 - 2023-01-08
### Fixed:
- Ava GUI: MainWindow Refactor.
  - Refactors the main window code on the Avalonia UI. No notable changes to the window itself.

## 1.1.519 - 2023-01-08
### Fixed:
- Horizon: Impl Prepo, Fixes bugs, Clean things.
  - Ports the prepo service to the new IPC handling from 1.1.506. Not known to affect any games.
  - Moves sm service logs from Info logs to Debug logs.
  - Cleans up sm and lm service code and general fixes.

## 1.1.518 - 2023-01-07
### Fixed:
- MainWindow: Vertically center SearchBox TextPresenter.
  - Centers "Search..." text in the games list search box on the Avalonia GUI.

## 1.1.517 - 2023-01-07
### Fixed:
- ava: Fix regression caused by #4013.
  - Fixes a regression from the previous change that caused the UI to break.

## 1.1.516 - 2023-01-07
### Added:
- Include a start.sh file with correct launch options.
  - Adds a script to run Ryujinx on Linux and simplify its usage.

## 1.1.515 - 2023-01-06
### Fixed:
- Ava GUI: AboutWindow Refactor.
  - Redesigns the "About" window on the Avalonia UI.

## 1.1.514 - 2023-01-06
### Fixed:
- [hipc] Fix 'Unexpected result code Success returned' in Reply().
  - Fixes a crash when minimizing Ryujinx.Headless.SDL2.

## 1.1.513 - 2023-01-06
### Fixed:
- Ava GUI: SettingsWindow Refactor.
  - Refactors the settings window code on the Avalonia UI. No actual changes to the window itself.

## 1.1.512 - 2023-01-06
### Fixed:
- chore: Update Ryujinx.SDL2-CS to 2.26.1.
  - May improve compatibility with certain controllers.

## 1.1.511 - 2023-01-04
### Fixed:
- Misc: Remove duplicated entries and clean locale csproj.
  - Small code cleanup. No changes in games.

## 1.1.508-1.1.510 - 2023-01-04
### Fixed:
- Readd Ryujinx.Ui.LocaleGenerator removed in 1.1.506.
  - Fixes a regression from 1.1.506 where the project couldn't be built with Visual Studio.
- hle: Add safety measure around overflow in ScheduleFutureInvocation.
  - Fixes a regression from 1.1.506 that caused crashing on Linux.

## 1.1.507 - 2023-01-04
### Fixed:
- Make PPTC state non-static.
  - Allows PPTC to be used when the JIT service is loaded, improving loading in Super Mario 64, N64 NSO emulator games and any other games that use this service.

## 1.1.506 - 2023-01-04
### Fixed:
- IPC refactor part 3+4: New server HIPC message processor.
  - Cleans up service implementations and opens the possibility for performance improvements in future changes to services. No known changes in games.

## 1.1.505 - 2023-01-04
### Fixed:
- Update Locale files.
  - Updates Avalonia UI language files with the latest changes from Crowdin.

You can help with missing translations on https://crwd.in/ryujinx

## 1.1.504 - 2023-01-03
### Fixed:
- Avalonia - Add source generator for locale items.
  - Makes it easier to manage localization code for the Avalonia GUI. No changes to actual UI functionality.

## 1.1.503 - 2023-01-02
### Fixed:
- misc: Use official names for NVDEC registers.
  - No changes to emulator functionality.

## 1.1.502 - 2023-01-01
### Fixed:
- chore: Update tests dependencies.
  - No changes to emulator functionality.

## 1.1.501 - 2023-01-01
### Fixed:
- Fix typo in left joycon SL binding.
  - Left SL button was incorrectly labeled as Right SL button.

## 1.1.500 - 2022-12-29
### Fixed:
- Filter hidden game files from the Game List.
  - On the next macOS release, removes "._" files from the games list.

## 1.1.499 - 2022-12-29
### Fixed:
- Use vector outputs for texture operations.
  - May improve performance in certain games on integrated GPUs, and on dedicated GPUs when using resolution scaling.

## 1.1.498 - 2022-12-29
### Fixed:
- HLE: Add basic stubs to get Labo VR booting to title screen.
  - Allows Nintendo Labo Toy-Con 04: VR Kit to boot.

## 1.1.497 - 2022-12-29
### Fixed:
- Vulkan: Don't flush commands when creating most sync.
  - Improves Vulkan performance in Pokémon Scarlet/Violet, The Legend of Zelda: Breath of the Wild, Xenoblade Chronicles Definitive Edition and Xenoblade Chronicles 3.
    - Xenoblade DE and 3 should now perform better on Vulkan than on OpenGL.

## 1.1.496 - 2022-12-29
### Fixed:
- Ava GUI: Restructure Ryujinx.Ava.
  - Code cleanup of the Avalonia project. No changes to emulator functionality.

## 1.1.495 - 2022-12-29
### Fixed:
- Fix for not receiving any SDL events on Linux using Headless build.

## 1.1.494 - 2022-12-28
### Fixed:
- haydn: Add support for PCMFloat, PCM32 and PCM8 conversions.
  - Improves SoundIO compatibility with audio devices that don't expose PCM16.

## 1.1.493 - 2022-12-26
### Fixed:
- Use new ArgumentNullException and ObjectDisposedException throw-helper API.
  - Small .NET code optimizations. No known changes in games.

## 1.1.492 - 2022-12-26
### Fixed:
- GPU: Add fallback when 16-bit formats are not supported.
  - On the next macOS release, fixes "ErrorFormatNotSupported" crashes in Ni no Kuni Wrath of the White Witch, Super Kirby Clash, Vroom in the Night Sky and Ys VIII: Lacrimosa of Dana. Only affects Intel Macs.

## 1.1.491 - 2022-12-26
### Fixed:
- Added Generic Math to BitUtils.
  - Small .NET code optimizations. No known changes in games. 

## 1.1.490 - 2022-12-26
### Fixed:
- bsd::RecvFrom: verify output buffer size before writing socket address.
  - Verifies output buffer size (sockAddrOutSize) is non zero before writing socket address. No known changes in games. 

## 1.1.489 - 2022-12-24
### Fixed:
- Some minor cleanups and optimizations.
  - No known changes to emulator functionality. 

## 1.1.488 - 2022-12-21
### Fixed:
- Implement a software ETC2 texture decoder.
  - Implements a software decoder to decompress ETC2 textures on CPU when the format is not supported on the GPU.
  - Fixes crashes in Infinity Tanks World War 2, Paradigm Paradox, Vegas Party and any other game that presented an ETC2 format error when using Vulkan with Nvidia and AMD GPUs. 

## 1.1.487 - 2022-12-21
### Fixed:
- Fix CPU FCVTN instruction implementation (slow path).
  - Fixes an issue on 2nd gen Intel CPUs and older that caused misaligned text on Two Point Campus and possibly other games.

## 1.1.486 - 2022-12-21
### Fixed:
- GPU: Force rebind when pool changes.
  - Fixes graphical issues on character images in "The New Prince of Tennis: LET'S GO!! ~Daily Life~ from RisingBeat".

## 1.1.485 - 2022-12-21
### Fixed:
- Make UI display correct content in Chinese.
  - Fixes an issue where games wouldn't display the correct title or icon on the games list when system language was set to Chinese.

## 1.1.484 - 2022-12-21
### Fixed:
- hle: Handle GPU profiler and debugger device path correctly.
  - Fixes log warnings in Doukoku Soshite (慟哭そして…).

## 1.1.483 - 2022-12-21
### Fixed:
- Fix DrawArrays vertex buffer size.
  - Fixes vertex explosions in Sphinx and the Cursed Mummy on OpenGL.

## 1.1.482 - 2022-12-20
### Fixed:
- ARMeilleure: Hash _data pointer instead of value for Operand.
  - May slightly improve how long it takes to boot any given game for the first time.

## 1.1.481 - 2022-12-19
### Fixed:
- Avalonia - Fix software keyboard row collision.
  - Fixes an issue where placeholder text would not go away when typing on the software keyboard applet on Avalonia.

## 1.1.480 - 2022-12-19
### Fixed:
- Eliminate zero-extension moves in more cases on 32-bit games.
  - Small code cleanup. May affect 32-bit games, though no changes are known.

## 1.1.479 - 2022-12-18
### Fixed:
- Revert "ARMeilleure: Add initial support for AVX512(EVEX encoding)".
  - Reverted due to a regression causing black screens on CPUs that support AVX-512.

## 1.1.478 - 2022-12-18
### Added:
- ARMeilleure: Add initial support for AVX512(EVEX encoding).
  - Implements enough of the EVEX encoding features to utilize AVX512 instructions for a 128-bit register use-case. Further changes are required for CPUs with AVX-512 support to have a notable performance improvement.

## 1.1.477 - 2022-12-18
### Fixed:
- hle: Fix wrong conversion in UserPresence.ToString.
  - Fixes the bell sound when entering Time Trials in Mario Kart 8 Deluxe.

## 1.1.476 - 2022-12-16
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 17.4.0 to 17.4.1.
  - Updates Microsoft.NET.Test.Sdk. No changes to emulator functionality.

## 1.1.475 - 2022-12-16
### Fixed:
- Implement another non-indexed draw method on GPU.
  - Fixes rendering in Ikaruga.

## 1.1.474 - 2022-12-16
### Fixed:
- GPU: Fix layered attachment write.
  - Fixes a regression introduced in 1.1.418 that broke the crowd rendering in Mario Strikers: Battle League.

## 1.1.473 - 2022-12-15
### Fixed:
- Avalonia: Fix invisible swkbd applet on Linux.
  - The software keyboard applet is now visible again on Avalonia on Linux systems.

## 1.1.472 - 2022-12-15
### Fixed:
- Replace DllImport usage with LibraryImport.
  - Code improvement, required for NativeAOT support. No known changes in games.

## 1.1.471 - 2022-12-15
### Fixed:
- Fix NRE when loading Vulkan shader cache with Vertex A shaders.
  - Fixes a regression that caused Catherine to crash on boot when loading the shader cache.

## 1.1.470 - 2022-12-14
### Fixed:
- Remove Half Conversion.
  - Code improvement. No known changes in games.

## 1.1.469 - 2022-12-14
### Fixed:
- Vulkan: enable VK_EXT_custom_border_color features.
  - Only create a custom border color if the feature is supported and enabled.
  - Fixes Vulkan crashes in Super Smash Bros. Ultimate, Xenoblade Chronicles 2 and other titles when using RADV on Linux.

## 1.1.468 - 2022-12-12
### Added:
- Bsd: Add support for dns_mitm.
  - Allows for simple DNS redirection, which is used by some mods.

## 1.1.467 - 2022-12-12
### Fixed:
- misc: Update to Ryujinx.Graphics.Nvdec.Dependencies 5.0.1-build13.
  - Fixes packaging issues on macOS related to an unsatisfied dependency on libX11.

## 1.1.466 - 2022-12-12
### Fixed:
- Use NuGet Central Package Management to manage package versions solution-wise.
  - Makes version management easier for all dependencies. No changes to emulator functionality. 

## 1.1.465 - 2022-12-12
### Fixed:
- misc: Some fixes to the updaters.
  - Fixes command line being broken when updating on Avalonia.
  - Makes the Avalonia updater fallback to the GTK Ryujinx executable if current name isn't found.
  - Makes permission setter function more generic.
  - Remove direct usage of chmod to use File.SetUnixFileMode.

## 1.1.464 - 2022-12-12
### Fixed:
- Fix "UI" abbreviation being miscapitalized.

## 1.1.463 - 2022-12-12
### Fixed:
- Use method overloads that support trimming. Mark some types to be trimming friendly.
  - Code cleanup. No changes to emulator functionality.

## 1.1.462 - 2022-12-12
### Added:
- Bsd: Implement Select.
  - Allows LAN mode to function in Saints Row: The Third and Saints Row IV.
  - Just Die Already now goes in-game.

## 1.1.461 - 2022-12-10
### Fixed:
- audio: Rewrite SoundIo bindings.
  - Code cleanup. No changes to emulator functionality.

## 1.1.460 - 2022-12-10
### Fixed:
- Fix Lambda Explicit Type Specification Warnings.
  - Code cleanup. No changes to emulator functionality.

## 1.1.459 - 2022-12-10
### Fixed:
- Fix Redundant Qualifer Warnings.
  - Code cleanup. No changes to emulator functionality.

## 1.1.458 - 2022-12-09
### Fixed:
- Fix HasUnalignedStorageBuffers value when buffers are always unaligned.
  - Fixes a regression introduced in 1.1.419 that caused NieR Automata: The End of YoRHa Edition to crash when loading into gameplay.

## 1.1.457 - 2022-12-09
### Fixed:
- Add explicit dependency on System.Drawing.Common on Ryujinx.Ava to workaround trimming bugs.
  - Fixes Avalonia builds crashing on startup since 1.1.456.

## 1.1.456 - 2022-12-09
### Fixed:
- misc: Remove dependency on System.Drawing.Common.
  - Removes System.Drawing.Common, which was used only once for DPI scaling factor, and implements the same behaviour using gdiplus. Reduces emulator size slightly.

## 1.1.455 - 2022-12-09
### Fixed:
- Add concurrency restriction on release workflows.
  - Allows merging multiple pull requests without needing to wait for them to build. 

## 1.1.454 - 2022-12-09
### Fixed:
- misc: Update Ryujinx.Graphics.Nvdec.Dependencies to 5.0.1-build12
  - Updates ffmpeg dependencies to support Linux x64 and macOS.

## 1.1.453 - 2022-12-09
### Fixed:
- ava: Restyle the Status Bar.
  - Some tweaks to the Avalonia status bar where performed (fonts and margin adjustments)
  - Game selector height was increased.

## 1.1.452 - 2022-12-09
### Fixed:
- nuget: bump CommandLineParser from 2.8.0 to 2.9.1.
  - Updates CommandLineParser. No changes to emulator functionality.

## 1.1.451 - 2022-12-08
### Fixed:
- Fix shader FSWZADD instruction.
  - Fixes text rendering in Just Dance 2023, The Stanley Parable: Ultra Deluxe, and possibly other games.
  - Fixes transparency issues in Two Point Campus.
  - Fixes layering issues in OlliOlli World. 

## 1.1.450 - 2022-12-08
### Fixed:
- Shader: Implement PrimitiveID.
  - Fixes overly dark lighting in Dark Souls Remastered.

## 1.1.449 - 2022-12-08
### Fixed:
- Fix inconsistent capitalization.
  - Fixes a typo.

## 1.1.448 - 2022-12-07
### Fixed:
- acc: Stub CheckNetworkServiceAvailabilityAsync.
  - Allows Hulu to boot.

## 1.1.447 - 2022-12-07
### Fixed:
- nuget: bump DynamicData from 7.12.8 to 7.12.11.
  - Updates DynamicData. No changes to emulator functionality.

## 1.1.446 - 2022-12-07
### Fixed:
- nuget: bump NUnit from 3.12.0 to 3.13.3.
  - Updates NUnit. No changes to emulator functionality.

## 1.1.445 - 2022-12-07
### Added:
- Add Ryujinx license to builds.
  - Adds Ryujinx license information file to emulator builds.

## 1.1.444 - 2022-12-07
### Fixed:
- nuget: bump System.Drawing.Common from 6.0.0 to 7.0.0.
  - Updates System.Drawing.Common. No changes to emulator functionality.

## 1.1.443 - 2022-12-07
### Fixed:
- hle: Do not add disabled AoC item to the list.
  - Fixes an issue that caused Mario Kart 8 Deluxe to not work properly when older DLC was disabled but not removed on the DLC list.

## 1.1.442 - 2022-12-06
### Fixed:
- macOS: Fix Struct Layout Packing.
  - Fix struct layout packing so more tests can run on macOS.

## 1.1.441 - 2022-12-06
### Fixed:
- gtk: Fixes warnings about obsolete components.
  - Removes some warnings from the GTK UI.

## 1.1.440 - 2022-12-06
### Fixed:
- Shader: Add fallback for LDG from "ube" buffer ranges.
  - Fixes grass particles in the wind in The Legend of Zelda: Breath of the Wild. May fix similar issues in other games.

## 1.1.439 - 2022-12-06
### Fixed:
- UI: Add Metal surface creation for MoltenVK.
  - Required for basic graphics rendering on macOS.

## 1.1.438 - 2022-12-06
### Fixed:
- nuget: bump XamlNameReferenceGenerator from 1.4.2 to 1.5.1.
  - Updates XamlNameReferenceGenerator. No changes to emulator functionality.

## 1.1.437 - 2022-12-06
### Fixed:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.25.0 to 6.25.1.
  - Updates System.IdentityModel.Tokens.Jwt. No changes to emulator functionality.

## 1.1.436 - 2022-12-06
### Fixed:
- ava: Cleanup Input classes.
  - Code cleanup. No changes to emulator functionality.

## 1.1.435 - 2022-12-06
### Added:
- amadeus: Add missing compressor effect from REV11.
  - No games are known to use this effect for now.

## 1.1.434 - 2022-12-05
### Fixed:
- Fix storage buffer access when match fails.
  - Fixes a regression from 1.1.430 that caused scanlines on save file icons and occasional black textures in Xenoblade Chronicles Definitive Edition.

## 1.1.433 - 2022-12-05
### Fixed:
- Change default Vsync toggle hotkey to F1 instead of Tab.
  - The default "toggle Vsync" (uncap/cap framerate) hotkey is now F1 and not Tab. Should help with users accidentally turning it off when alt-tabbing.

## 1.1.432 - 2022-12-05
### Fixed:
- nuget: bump Microsoft.CodeAnalysis.CSharp from 4.2.0 to 4.4.0.
  - Updates Microsoft.CodeAnalysis.CSharp. No changes to emulator functionality.

## 1.1.431 - 2022-12-05
### Fixed:
- Fix Sorting Regression.
  - Fixes a regression introduced in 1.1.428 that caused the games list to disappear.

## 1.1.430 - 2022-12-05
### Fixed:
- Fix shaders with global memory access from unknown locations.
  - Fixes a regression from 1.1.427 that caused Mario Golf Super Rush (and possibly others) to crash.

## 1.1.429 - 2022-12-05
### Fixed:
- Update 'OpenGL Log Level' to 'Graphics Backend Log Level'.
  - Renames the OpenGL logging option to properly reflect that it relates to both backends.

## 1.1.428 - 2022-12-05
### Fixed:
- Ava GUI: Several UI Fixes.
  - Adjusts styles and UI elements to more closely match WinUI theming.
  - Adjusts how playtime is represented to hours and minutes.
  - Type boxes and other focusable elements now defocus when empty-space is selected.

## 1.1.427 - 2022-12-05
### Fixed:
- Restrict shader storage buffer search when match fails.
  - May help avoid an error on macOS in the future. No known changes in games.

## 1.1.426 - 2022-12-05
### Fixed:
- Make structs readonly when applicable.
  - Code cleanup. No changes to emulator functionality.

## 1.1.425 - 2022-12-05
### Fixed:
- misc: Fix obsolete warnings in Ryujinx.Graphics.Vulkan.
  - No changes to emulator functionality.

## 1.1.424 - 2022-12-05
### Fixed:
- nuget: bump Microsoft.NET.Test.Sdk from 16.8.0 to 17.4.0.
  - Updates Microsoft.NET.Test.Sdk. No changes to emulator functionality.

## 1.1.423 - 2022-12-04
### Added:
- Add InfoType.MesosphereCurrentProcess.
  - Allows exefs replacement mods and homebrew to easily get their own process handle for use with certain SVCs, such as MapProcessMemory.

## 1.1.422 - 2022-12-04
### Fixed:
- ui: Disallow checking for updates while emulation active.
  - Disables "check for updates" option while a game is running.

## 1.1.421 - 2022-12-04
### Fixed:
- Allow SNorm buffer texture formats on Vulkan.
  - Used by most UE4 games, though not known to affect any of them at the moment.

## 1.1.420 - 2022-12-04
### Fixed:
- Implement non-MS to MS copies with draws.
  - Required by Apple Silicon GPUs. No known changes in games.

## 1.1.419 - 2022-12-04
### Fixed:
- GPU: Use lazy checks for specialization state.
  - Improves performance slightly in Super Mario Odyssey and possibly other games.

## 1.1.418 - 2022-12-04
### Fixed:
- GPU: Swap bindings array instead of copying.
  - Improves performance slightly in Super Mario Odyssey and possibly other games.

## 1.1.417 - 2022-12-03
### Fixed:
- Use source generated regular expressions.
  - Code cleanup. No changes in games.

## 1.1.416 - 2022-12-03
### Fixed:
- Support logging available memory on macOS.
  - Available RAM will be shown on macOS logs on the next macOS update.

## 1.1.415 - 2022-12-02
### Fixed:
- Fix using in Ava.
  - Fixes an oversight in the Avalonia code that caused 1.1.414 not to compile.

## 1.1.414 - 2022-12-02
### Fixed:
- SDL2Driver: Invoke dispatcher on main thread.
  - Required for macOS. No expected changes in games.

## 1.1.413 - 2022-12-02
### Fixed:
- Avalonia - Save Manager.
  - Adds a save manager to the Avalonia UI under Options > Manage User Profiles.
  - Allows for easy file deletion and quick save folder opening.
  - Adds an option to restore lost user profiles using existing saves.

## 1.1.412 - 2022-12-02
### Fixed:
- amadeus: Fix wrong SendCommands logic.
  - May help games with audio desyncs.

## 1.1.411 - 2022-12-02
### Fixed:
- Ava GUI: Add back locales removed in #3955.
  - Adds back "SettingsButtonSave" & "SettingsButtonClose" removed in 1.1.410.
  - Fixes "Close" showing as "SettingsButtonClose" on the  Avalonia UI.

## 1.1.410 - 2022-12-01
### Fixed:
- Ava GUI: Make Dialogue More Intuitive.
  - Renames "Save" to "OK" and "Close" to "Cancel".
  - Layout of buttons adjusted to establish a clearer visual hierarchy (adapts to match OS).
  - "OK" is now bound to Enter and the button is highlighted.
  - "Cancel" is now bound to Escape.

## 1.1.409 - 2022-12-01
### Fixed:
- Revert "nuget: bump SixLabors.ImageSharp from 1.0.4 to 2.1.3 (#3976)".
  - Reverts the previous change.

## 1.1.408 - 2022-12-01 ⚠️ This build has been removed.
### Fixed:
- nuget: bump SixLabors.ImageSharp from 1.0.4 to 2.1.3.
  - Updates SixLabors.ImageSharp dependencies. No changes to emulator functionality.

## 1.1.407 - 2022-12-01
### Fixed:
- chore: Update Silk.NET to 2.16.0.
  - Updates Silk.NET dependencies and Vulkan extensions. No changes to emulator functionality.

## 1.1.406 - 2022-12-01
### Fixed:
- Better SDL2 Audio Init Error Logging.
  - Adds an error to the log when SDL2 fails to initialize. 

## 1.1.405 - 2022-12-01
### Fixed:
- GPU: Track buffer migrations and flush source on incomplete copy.
  - Fixes most cases of vertex explosions in Pokémon Scarlet/Violet.
  - Fixes device loss crashes and possibly vertex explosions in Xenoblade Chronicles 3 on Vulkan, ONLY if shader caches from before this change are purged beforehand. 

## 1.1.404 - 2022-12-01
### Fixed:
- infra: Add distribution files for macOS.
  - Upstreams macOS packing and distribution files.

## 1.1.403 - 2022-11-30
### Fixed:
- Avalonia: Clean up leftover RenderTimer & Fix minimum and initial window size.
  - Black bars will no longer show on the default window size on Avalonia.
  - Framerate on the Avalonia UI is no longer limited (does not affect games). 

## 1.1.402 - 2022-11-30
### Fixed:
- nuget: bump DiscordRichPresence from 1.0.175 to 1.1.3.18.
  - Updates DiscordRichPresence dependencies. No changes to emulator functionality.

## 1.1.401 - 2022-11-30
### Fixed:
- Remove shader dependency on SPV_KHR_shader_ballot and SPV_KHR_subgroup_vote extensions.
  - Required for MoltenVK. No changes in games.

## 1.1.400 - 2022-11-30
### Fixed:
- Ensure that vertex attribute buffer index is valid on GPU.
  - Fixes some crashes on Vulkan on Intel and AMD graphics cards. It's currently unknown which games are affected.

## 1.1.399 - 2022-11-29
### Fixed:
- nuget: bump System.Management from 6.0.0 to 7.0.0.
  - Updates System.Management to 7.0.0. No changes to emulator functionality.

## 1.1.398 - 2022-11-29
### Fixed:
- ConcurrentBitmap: Use Interlocked Or/And.
  - Code cleanup. No changes to emulator functionality.

## 1.1.397 - 2022-11-29
### Fixed:
- chore: Update OpenTK to 4.7.5.
  - Updates OpenTK dependencies. No changes to emulator functionality.

## 1.1.396 - 2022-11-29
### Fixed:
- Avalonia: Update FluientAvalonia
  - Make dialogs work on Linux with Avalonia making it usable on it again.

## 1.1.395 - 2022-11-28
### Fixed:
- GPU: Always draw polygon topology as triangle fan.
  - Fixes the stats chart in Pokémon Legends Arceus and Pokémon Scarlet/Violet on Vulkan and on certain OpenGL drivers that don't support GL_POLYGON in compatibility mode.

## 1.1.394 - 2022-11-28
### Fixed:
- amadeus: Fixes and initial 15.0.0 support.
  - Fixes crashes in Ninja Gaiden Sigma 2, Ninja Gaiden 3: Razor's Edge and Paper Mario: The Origami King.
  - Fixes broken audio in Crash Team Racing Nitro-Fueled.
  - Fix Delay effect wrong variable usage for matrix transform on Stereo, Quadraphonic and Surround codepaths.
  - Update Delay effect Surround matrix to support REV11 optimization.
  - Change voice drop logic to use 32 bits integer to be closer to real firmware. Might fix voice drop issues on some games.
  - Add voice drop parameter support that was introduced in 15.0.0.
  - Accurately stub ExecuteAudioRendererRendering.

## 1.1.393 - 2022-11-27
### Fixed:
- sfdnsres: Fix deserializer of AddrInfoSerialized when addresses are empty.
  - Allows the Homebrew App Store to boot.

## 1.1.392 - 2022-11-27
### Fixed:
- bsd: Fix eventfd broken logic.
  - Required for Pokémon Legends Arceus 1.1.1 to boot with Guest Internet Access enabled, though more changes are necessary for this.

## 1.1.391 - 2022-11-27
### Fixed:
- HLE: fix small issue in IPsmSession.
  - Fixes small logic error in the Psm service. No known changes in games.

## 1.1.390 - 2022-11-26
### Fixed:
- Avalonia: Fix OpenGL crashing on Linux.
  - Fixes a regression from 1.1.389 that caused Avalonia to crash on Linux when using OpenGL.

## 1.1.389 - 2022-11-25
### Fixed:
- Avalonia: Fix invisible Vulkan window on Linux.
  - Fixes the Avalonia rendering window being invisible when using Vulkan on Linux.

## 1.1.388 - 2022-11-25
### Fixed:
- ava: Refactor Title Update Manager window.
  - The Title Update Manager on Avalonia is now redone to be consistent with the changes in 1.1.385.
  - Fixes an issue where trying to scroll with a mouse wheel would expand the window instead, this time for real.

## 1.1.387 - 2022-11-25
### Fixed:
- Fix CB0 alignment with addresses used for 8/16-bit LDG/STG.
  - Fixes a regression introduced in 1.1.355 that caused Xenoblade 3 videos to be more pixelated than normal.

## 1.1.386 - 2022-11-25
### Fixed:
- chore: Update Avalonia related dependencies.
  - Updates Avalonia dependencies. No changes to emulator functionality.

## 1.1.385 - 2022-11-25
### Fixed:
- ava: Rework DLC Manager, add various fixes and cleanup.
  - The DLC Manager on Avalonia is completely redone to enhance the user experience.
  - Fixes an issue where trying to scroll with a mouse wheel on a list with multiple DLCs would expand the window instead.
  - The game list will now wait 1 second after a keyword is typed to search for a game. Previously it would refresh on each keystroke, taking too long to load on big lists.
  - Updates are now selected automatically when added to the Title Update Manager.
  - After deleting a game directory in settings, the next one will be automatically selected.

## 1.1.384 - 2022-11-24
### Fixed:
- nuget: bump SharpZipLib from 1.3.3 to 1.4.1.
  - Updates the SharpZipLib package version. No changes to emulator functionality.

## 1.1.383 - 2022-11-24
### Fixed:
- chore: Update Ryujinx.SDL2-CS to 2.24.2.
  - May improve controller compatibility or sound issues in some games.
    - Controller bindings may need to be reconfigured after this update.

## 1.1.382 - 2022-11-24
### Fixed:
- GPU: Don't trigger uploads for redundant buffer updates.
  - Improves performance in Xenoblade Chronicles: Definitive Edition and The Legend of Zelda: Link's Awakening on Vulkan.

## 1.1.381 - 2022-11-24
### Fixed:
- Reduce usage of Marshal.PtrToStructure and Marshal.StructureToPtr.
  - Code cleanup. No expected changes to emulator functionality.

## 1.1.380 - 2022-11-24
### Fixed:
- ui: Fixes disposing on GTK/Avalonia and Firmware Messages on Avalonia.
  - Fixes an issue where opening a game without installed firmware and then selecting the "OK" or "Open the Setup Guide" options would crash Ryujinx.

## 1.1.379 - 2022-11-24
### Fixed:
- Ryujinx.Ava: Add missing redefinition of app name.
  - Fixes an issue where Ryujinx could sometimes report as "Avalonia Application".

## 1.1.378 - 2022-11-24
### Fixed:
- Fix NRE on Avalonia for error applets with unknown error message.
  - Fixes an issue where unknown errors would crash Avalonia.

## 1.1.377 - 2022-11-24
### Fixed:
- GAL: Send all buffer assignments at once rather than individually.
  - Improves performance significantly in Pokémon Scarlet/Violet, Super Mario Odyssey, Super Smash Bros. Ultimate and any other games that tend to bind many constant buffers at once.

## 1.1.376 - 2022-11-23
### Fixed:
- GPU: Access non-prefetch command buffers directly.
  - Improves performance slightly in Pokémon Scarlet/Violet and Super Mario Odyssey.

## 1.1.375 - 2022-11-23
### Fixed:
- GPU: Relax locking on Buffer Cache.
  - Improves performance in Super Mario Odyssey and possibly other games with high FIFO.

## 1.1.374 - 2022-11-23
### Fixed:
- nuget: bump Avalonia from 0.10.15 to 0.10.18.
  - Updates the Avalonia package version. No changes to emulator functionality.

## 1.1.373 - 2022-11-23
### Fixed:
- ava: Fix JsonSerializer warnings.
  - Fixes some warnings in the Avalonia project. No changes to emulator functionality.

## 1.1.372 - 2022-11-23
### Fixed:
- Update to LibHac 0.17.0.
  - Fixes an issue where Ryujinx would delete the Save folder in the sdcard directory after booting.
  - Improves filesystem emulation stability.

## 1.1.371 - 2022-11-23
### Fixed:
- Stub IFriendService: 1 (Cancel).
  - Allows SnowRunner to proceed past the title screen.

## 1.1.370 - 2022-11-23
### Fixed:
- Avalonia - Fix controller insertion crash.
  - Fixes a crash that occurred when connecting a new controller while the settings window was open on Avalonia.

## 1.1.369 - 2022-11-21
### Fixed:
- Do not update shader state for DrawTextures.
  - Fixes a crash in A Hat in Time that occurred in certain places.

## 1.1.368 - 2022-11-20
### Fixed:
- Use upstream unicorn for Ryujinx.Tests.Unicorn.
  - CPU tests can now be executed on Linux. No changes in games.

## 1.1.367 - 2022-11-20
### Fixed:
- Reword the description of the 6GB expand DRAM hack to be less tantalizing.
  - "Expand DRAM Size to 6GiB" is now named "Use alternative memory layout (Developers)". 

## 1.1.366 - 2022-11-19
### Fixed:
- Unsubscribe MemoryUnmappedHandler even when GPU channel is destroyed.
  - Fixes a regression introduced in 1.1.361 that caused World of Light to crash in Super Smash Bros. Ultimate. 

## 1.1.365 - 2022-11-19
### Fixed:
- Fix shader cache on Vulkan when geometry shaders are inserted.
  - Fixes a crash when loading the shader cache on Vulkan on GPUs affected by 1.1.364 in Pokémon Scarlet/Violet. 

## 1.1.364 - 2022-11-18
### Fixed:
- Move gl_Layer from vertex to geometry if GPU does not support it on vertex.
  - Fixes a crash during boot on Vulkan and a black screen on OpenGL when using Maxwell and older Nvidia GPUs in Pokémon Scarlet/Violet. 

## 1.1.363 - 2022-11-18
### Fixed:
- Vulkan: Clear dummy texture to (0,0,0,0) on creation.
  - Fixes an issue with AMD GPUs on Linux that caused colored filters to appear over the screen in Pokémon Scarlet/Violet when using Vulkan. 

## 1.1.362 - 2022-11-18
### Fixed:
- GPU: Fix thread safety of ReregisterRanges.
  - Fixes some crashes in Pokémon Scarlet/Violet. 
  - May fix similar issues in Pokémon Sword/Shield and possibly other games.

## 1.1.361 - 2022-11-18
### Fixed:
- Prune ForceDirty and CheckModified caches on unmap.
  - Fixes a regression that would degrade performance over time in Super Mario Odyssey.
- Vulkan: Don't create preload command buffer outside a render pass.
  - Improves performance in Pokémon Scarlet/Violet.

## 1.1.359 - 2022-11-17
### Fixed:
- am: Stub GetSaveDataSizeMax.
  - Allows Football Manager 2023 Touch to boot.

## 1.1.358 - 2022-11-17
### Added:
- Use ReadOnlySpan<byte> compiler optimization in more places.
  - Small code optimization. No known changes in games.

## 1.1.357 - 2022-11-17
### Fixed:
- Allow _volatile to be set from MultiRegionHandle checks again.
  - Fixes performance regressions from 1.1.335 that affected Pokémon Sword/Shield, Yu-Gi-Oh! Rush Duel: Dawn of the Battle Royale and possibly other games.

## 1.1.356 - 2022-11-17
### Fixed:
- SPIR-V: Fix unscaling helper not being able to find Array textures.
  - Fixes broken ground textures in Pokémon Scarlet/Violet when using resolution scaling.
  - Fixes upscaling on exploding bubbles in Bubble Bobble and spawn spheres and other effects in Rune Factory 5.

## 1.1.355 - 2022-11-17
### Fixed:
- GPU: Eliminate CB0 accesses when storage buffer accesses are resolved.
  - Improves performance significantly in Xenoblade Chronicles Definitive Edition on Vulkan.
  - Improves performance in Xenoblade 2 and Xenoblade 3 on Vulkan, and in Definitive Edition on OpenGL.
  - Improves performance in Pokémon Scarlet/Violet and possibly other games.

## 1.1.354 - 2022-11-17
### Fixed:
- ci: Clean up Actions leftovers.
  - Fixes Avalonia build versions for pull requests.
  - Ensures that the "--self-contained" doesn't warn at build.

## 1.1.353 - 2022-11-17
### Fixed:
- Capitalization to be consistent.
  - Small grammar changes.

## 1.1.352 - 2022-11-17
### Fixed:
- Allow to start Ryujinx in Wayland environment.
  - Allows Ryujinx to start in Wayland environment, ignoring code to retrieve monitor dimensions.

## 1.1.351 - 2022-11-16
### Fixed:
- Fix Fedora support.
  - Fixes Fedora Linux not having a symlink for libX11.so by attempting to import it by version.

## 1.1.350 - 2022-11-16
### Fixed:
- Prevent raw Unicode control codes from showing on software keyboard applet.
  - Fixes some formatting errors on the software keyboard applet.

## 1.1.349 - 2022-11-16
### Fixed:
- Update units of memory from decimal to binary prefixes.
  - Changes "GB" to "GiB" and "MB" to "MiB" on the UI and the rest of the code.

## 1.1.348 - 2022-11-16
### Fixed:
- Use new C# 11 UTF-8 string literals.
  - Small code optimization. No known changes in games.

## 1.1.347 - 2022-11-16
### Fixed:
- Make use of Random.Shared.
  - Small code optimization. No known changes in games.

## 1.1.346 - 2022-11-16
### Fixed:
- Use new LINQ Order() methods.
  - Small code optimization. No known changes in games.

## 1.1.345 - 2022-11-16
### Added:
- Implement HLE macro for DrawElementsIndirect.
  - Adds an "Enable Macro HLE" option to graphics settings, enabled by default. 
  - When enabled, improves performance on Monster Hunter Rise, NieR Automata: The End of YoRHa Edition, Nintendo Switch Sports (not yet playable) and possibly other games.

## 1.1.344 - 2022-11-15
### Fixed:
- GTK: It's REE-YOU-JINX.
  - Corrects the pronunciation guide in the GTK UI's "about" page.

## 1.1.343 - 2022-11-12
### Fixed:
- UI: Allow overriding graphics backend + Move command line parser into a new class.
  - Adds a new command line option "-g/--graphics-backend" which allows to override the previously configured graphics backend value on launch.
  - Command line arguments are now kept when Avalonia restarts (except for the overridden graphics backend).
  - Reduces the amount of duplicate code between GTK and Avalonia.

## 1.1.342 - 2022-11-12
### Fixed:
- Use vector transform feedback outputs if possible.
  - Fixes grass rendering in Xenoblade Chronicles Definitive Edition on Vulkan on Intel GPUs. 
  - May fix similar issues in Pokkén Tournament or the Xenoblade games on AMD and/or Intel GPUs.

## 1.1.341 - 2022-11-11
### Fixed:
- Fix VertexId and InstanceId on Vulkan.
  - Fixes incorrect rendering in Pokémon Mystery Dungeon Rescue Team DX on Vulkan on Intel GPUs.
  - May fix similar issues in Metro 2033 Redux, Sniper Elite 3 and others. 

## 1.1.340 - 2022-11-10
### Fixed:
- Minor improvement to Vulkan pipeline state and bindings management.
  - No known changes in games.

## 1.1.339 - 2022-11-09
### Changed:
- infra: Migrate to .NET 7
  - Update project to .NET 7 and enable TieredPGO.
  - Possible performance improvements up to 15% in .NET runtime limited scenarios.

## 1.1.338 - 2022-11-03
### Fixed:
- Ensure all pending draws are done before compute dispatch.
  - Nights of Azure 2: Bride of the New Moon now works on Vulkan.

## 1.1.337 - 2022-11-02
### Fixed:
- Vulkan: Implement multisample <-> non-multisample copies and depth-stencil resolve.
  - Fate/Extella: The Umbral Star now works on Vulkan on Nvidia and Intel GPUs. 
  - Sonic Colors: Ultimate now works on Vulkan on Intel GPUs.

## 1.1.336 - 2022-11-02
### Fixed:
- fix: Support FFmpeg 5.1.x for decoding.
  - FFmpeg 5.1+ now plays pre-rendered videos properly on Linux.

## 1.1.335 - 2022-10-29
### Fixed:
- GPU: Use a bitmap to track buffer modified flags.
  - Improves performance significantly (up to 500%) on Bayonetta 3, Mario + Rabidds Kingdom Battle, Mario + Rabbids Sparks of Hope, Monster Hunter Rise, Super Mario 3D All-Stars (Sunshine and Galaxy), Zombie Army 4: Dead War and possibly other games. 

## 1.1.334 - 2022-10-29
### Fixed:
- CI: Fix windows builds missing SourceRevisionId.
  - Windows PR builds will now have version IDs again. 

## 1.1.333 - 2022-10-29
### Fixed:
- Vulkan: Replace VK_EXT_debug_report usage with VK_EXT_debug_utils.
  - No expected changes. 

## 1.1.332 - 2022-10-29
### Fixed:
- SPIR-V: Fix tessellation control shader output types.
  - Fixes crashes on AMD GPUs running Vulkan on Windows in Bayonetta 3 (after the chapter 1 cutscene) and Luigi's Mansion 3 (right before the title screen). 

## 1.1.331 - 2022-10-29
### Added:
- nuget: bump System.IdentityModel.Tokens.Jwt from 6.15.0 to 6.25.0
  - Updates the JWT Token .NET dependency to version 6.25.0.
  - No expected changes.

## 1.1.330 - 2022-10-27
### Fixed:
- AppletAE: Stub SetRecordVolumeMuted.
  - Fixes a crash in Bayonetta 3 when entering gameplay in the first mission.

## 1.1.329 - 2022-10-27
### Fixed:
- hid/irs: Stub StopImageProcessorAsync.
  - Stubs the StopImageProcessorAsync service.
  - Prevents a crash in Game Builder Garage when exiting a game using the IR motion camera.
  - Allows Nintendo Labo Toy-Con 03: Vehicle Kit to progress past the "Make" menu.

## 1.1.328 - 2022-10-26
### Fixed:
- Vulkan: Fix indirect buffer barrier.
  - Fixes an ErrorDeviceLost crash that could occur in Monster Hunter Rise on Nvidia drivers v522.25, and possibly some older ones, when running the game on Vulkan. 

## 1.1.327 - 2022-10-25
### Fixed:
- Vulkan: Use dynamic state for blend constants.
  - Reduces memory usage and slightly speeds up Vulkan pipeline compilation in Mario Kart 8 Deluxe.

## 1.1.326 - 2022-10-23
### Added:
- Ryujinx.Tests.Unicorn: Implement IDisposable.
  - Disposes of Unicorn (CPU emulator used to test validity of ARMeilleure instructions) tests when done. No changes to emulator functionality.

## 1.1.325 - 2022-10-23
### Fixed:
- Attempt to fix issues since github-script v6 upgrade.
  - Fixes some issues with GitHub artifact creation. No changes to emulator functionality.

## 1.1.324 - 2022-10-23
### Fixed:
- Avalonia: Use overlay dialog for controller applet.
  - Fixes an issue where the controller applet was not showing properly in the Avalonia UI.

## 1.1.323 - 2022-10-22
### Fixed:
- nuget: bump SPB from 0.0.4-build24 to 0.0.4-build27.

## 1.1.322 - 2022-10-21
### Fixed:
- CI: Update workflows.
  - Updates Github workflows to the latest version.

## 1.1.321 - 2022-10-21
### Fixed:
- Vulkan: Fix vertex position Z conversion with geometry shader passthrough.
  - Fixes black screen in Game Builder Garage on Vulkan.

## 1.1.320 - 2022-10-19
### Fixed:
- Avalonia: update it_IT.json.
  - Updates the Italian localization for the Avalonia UI.

## 1.1.319 - 2022-10-18
### Fixed:
- Do not clear the rejit queue when overlaps count is equal to 0.
  - No known changes in games.

## 1.1.318 - 2022-10-18
### Added:
- Implement the GetSessionCacheMode in SSL service.
  - No known changes in games.

## 1.1.317 - 2022-10-18
### Fixed:
- Manage state of NfcManager.
  - Fixes Amiibo scanning in Hyrule Warriors Definitive Edition.

## 1.1.316 - 2022-10-18
### Fixed:
- Fix mapping leaks caused by UnmapView not working on Linux.
  - Fixes an issue where UnmapView was failing on Linux because the flags combination being passed was invalid. No known changes in games.

## 1.1.315 - 2022-10-18
### Added:
- A32: Implement VCVTT, VCVTB.
  - Radiant Silvergun is now playable.

## 1.1.314 - 2022-10-18
## Added:
- A64: Add fast path for Fcvtas_Gp/S/V, Fcvtau_Gp/S/V and Frinta_S/V instructions.
  - May reduce stuttering and improve performance in Mario Strikers: Battle League, Mario Party Superstars and Super Smash Bros. Ultimate.

## 1.1.313 - 2022-10-18
### Fixed:
- Avalonia: Update Polish Translation.
  - Updates the Polish localization for the Avalonia UI.

## 1.1.312 - 2022-10-18
### Fixed:
- Vulkan: Dispose TextureStorage when views hit 0 instead of immediately.
  - Reduces VRAM usage in Super Mario Odyssey when running on Vulkan with higher resolution scaling values. May improve VRAM usage on other games.

## 1.1.311 - 2022-10-18
### Fixed:
- Fix: Arguments Break when Updating.
  - Command line arguments will no longer break after updating the emulator.

## 1.1.310 - 2022-10-18
### Fixed:
- Avoid allocations in .Parse methods.
  - No changes in games.

## 1.1.309 - 2022-10-18
### Fixed:
- Vulkan: Fix blit levels/layers parameters being inverted.
  - Fixes a crash before the title screen in Mario + Rabbids Kingdom Battle on Nvidia GPUs using Vulkan.

## 1.1.308 - 2022-10-17
### Fixed:
- Fix kernel VA allocation when random allocation fails.
  - Fixes an issue with random allocations that may have affected some 32-bit games, such as DoDonPachi Resurrection, although no discernible changes were observed during gameplay.

## 1.1.307 - 2022-10-17
### Fixed:
- Avalonia - Remove on property changed call in Time Zone validation.
  - Fixes an issue in the Avalonia UI where using arrow keys in an AutoCompleteTextBox (such as the timezone textbox) would select the first entry and remove all other options.

## 1.1.306 - 2022-10-17
### Fixed:
- Implement OpenDataStorageWithProgramIndex partially.
  - Immortals Fenyx Rising now boots, though it doesn't reach gameplay.
  - Bit.Trip Runner, Bit.Trip Void, MLB The Show 22 and RollerCoaster Tycoon 3 now go in-game. 

## 1.1.305 - 2022-10-16
### Fixed:
- TamperMachine: Fix input mask check.
  - Cheats that required pressing buttons to enable/disable them will now work. 

## 1.1.304 - 2022-10-16
### Fixed:
- Fix various issues caused by Vertex/Index buffer conversions.
  - Fixes some bugs introduced in 1.1.254 and 1.1.278. No known changes in games.

## 1.1.303 - 2022-10-16
### Fixed:
- Fix primitive count calculation for topology conversion.
  - Fixes a regression that caused random triangles to appear on the map in Luigi's Mansion 3.

## 1.1.302 - 2022-10-16
### Fixed:
- Fix phantom configured controllers.
  - Fixes controllers not being disabled properly. Controller applet will no longer ask for a single controller when only one is configured.

## 1.1.301 - 2022-10-15
### Fixed:
- Improve shader BRX instruction code generation.
  - Improves the code generated for BRX instructions. No known changes in games.

## 1.1.300 - 2022-10-15
### Fixed:
- bsd: Check if socket is bound before calling RecvFrom().
  - Fixes a crash in Overpass when selecting a career mode.

## 1.1.299 - 2022-10-10
### Fixed:
- Vulkan: Fix sampler custom border color.
  - Fixes shadows in Xenoblade Chronicles 2 cutscenes when using Vulkan.

## 1.1.298 - 2022-10-09
### Fixed:
- Fix disposed textures being updated on TextureBindingsManager.
  - Fixes a fatal error crash in Crash Team Racing Nitro-Fueled when using Vulkan.

## 1.1.297 - 2022-10-08
### Fixed:
- GPU: Pass SpanOrArray for Texture SetData to avoid copy.
  - Improves performance slightly in NieR Automata: The End of Yorha Edition and UE4 games.

## 1.1.296 - 2022-10-08
### Fixed:
- Vulkan: Fix some issues with CacheByRange.
  - Fixes broken or missing geometry in eBaseball Powerful Pro Yakyuu 2022 and potentially other games that had similar issues with quads.

## 1.1.295 - 2022-10-05
### Fixed:
- Change NvMap ID allocation to match nvservices.
  - Fixes Animal Crossing: New Horizons crashing on start-up without a save file.
  - Fixes other miscellaneous crashes and texture corruptions in Animal Crossing: New Horizons. 
  - Fixes random crashing in The Legend of Zelda: Breath of the Wild.
  - Fixes random crashing when entering or exiting Pokémon centres in Pokémon Sword/Shield.

## 1.1.294 - 2022-10-04
### Fixed:
- Fix memory corruption in BCAT and FS Read methods when buffer is larger than needed.
  - Fixes a crash on the title screen of Sword Art Online: Alicization Lycoris, which now goes in-game.

## 1.1.293 - 2022-10-03
### Fixed:
- Fix shader SULD (bindless) instruction using wrong register as handle.
  - Fixes a regression that caused vertex explosions in Sea of Solitude: The Director's Cut.
  - Fixes rendering in Shadowrun Returns.

## 1.1.292 - 2022-10-03
### Fixed:
- Support use of buffer ranges with size 0.
  - Fixes a regression from 1.1.278 that caused a crash in Fire Emblem Warriors: Three Hopes after the Blue Lions prologue ended.

## 1.1.291 - 2022-10-03
### Fixed:
- Vulkan: Fix buffer texture storage not being updated on buffer handle reuse.
  - Fixes an issue where models would randomly swap back to an old animation frame in UE4 games.

## 1.1.290 - 2022-10-03
### Fixed:
- Avalonia - Fixes updater.
  - Autoupdating will now be possible on Avalonia builds.

## 1.1.289 - 2022-10-02
### Fixed:
- Avalonia: Fix About window not displaying translated window titles.
  - "About" window will now have a properly translated title in different languages on the Avalonia GUI.

## 1.1.288 - 2022-10-02
### Fixed:
- Allow Surface Flinger frame enqueue after process has exited.
  - Fixes an exception that could occur in some rare cases while ending emulation.

## 1.1.287 - 2022-10-02
### Added:
- Volume Hotkeys.
  - Adds hotkeys to increase and decrease the volume by steps of 5%.
  - Default is currently unbound. This can be mapped in Avalonia or via the config json file.

## 1.1.286 - 2022-10-02
### Added:
- ARMeilleure: Add gfni acceleration.
  - Implements gfni instructions to accelerate general purpose bit-shuffling.
  - New instructions are useable on Intel (Icelake 2021 & later) and AMD Zen 4 (2022 & later) CPUs.

## 1.1.285 - 2022-10-02
### Fixed:
- Avoid allocating unmanaged string per shader.
  - No known changes in games.

## 1.1.284 - 2022-10-02
### Added:
- fatal: Implement Service.
  - No changes in games.

## 1.1.283 - 2022-10-01
### Fixed:
- Fix incorrect tessellation inputs/outputs.
  - Fixes missing graphics in The Legend of Heroes: Trails from Zero.
  - Corrects ground rendering in The Witcher 3: Wild Hunt when using Vulkan.

## 1.1.282 - 2022-09-29
### Fixed:
- Fix SSL GetCertificates with certificate ID set to All.
  - Fixes a crash on launch in Life is Strange Remastered. The game is now playable.

## 1.1.281 - 2022-09-29
### Fixed:
- Vulkan: Zero blend state when disabled or write mask is 0.
  - May reduce stuttering and slightly improve performance on Intel and AMD graphics cards.

## 1.1.280 - 2022-09-28
### Fixed:
- Fix ListOpenContextStoredUsers and stub LoadOpenContext.
  - Fixes a crash when launching the games in Prinny Presents NIS Classics Volume 3: La Pucelle: Ragnarok / Rhapsody: A Musical Adventure. The games are now playable.

## 1.1.279 - 2022-09-20
### Fixed:
- Fpsr and Fpcr freed.
  - May reduce stuttering in Tony Hawk's Pro Skater 1 + 2.
  - May further improve pre-rendered video playback.

## 1.1.278 - 2022-09-20
### Fixed:
- Convert Quads to Triangles in Vulkan.
  - Improves Vulkan performance in Fast RMX on Intel GPUs.
  - May improve Vulkan performance in The Legend of Zelda: Skyward Sword HD and all 3 Xenoblade games on Intel GPUs. Might improve Vulkan performance in all of the above games on AMD GPUs. Nvidia Vulkan appears to be unaffected.

## 1.1.277 - 2022-09-19
### Fixed:
- OpenGL: Fix blit from non-multisample to multisample texture.
  - Fixes a rendering regression in Fate/EXTELLA.

## 1.1.276 - 2022-09-19
### Fixed:
- Avalonia - Misc changes to UX.
  - Settings navbar is now full-sized.
  - Alignment in a few windows has been fixed.
  - Number of each controller type is now listed instead of ID.
  - Volume widget on status bar is now aligned and localizable.

## 1.1.275 - 2022-09-19
### Fixed:
- Allow bindless textures with handles from unbound constant buffer.
  - Fixes Sniper Elite 3 crashing on startup.

## 1.1.274 - 2022-09-19
### Changed:
- Avalonia - Use embedded window for Avalonia.
  - Improves frame pacing of games when using the Avalonia UI.
  - Fixes unresponsiveness of the Avalonia UI when using Vulkan.
  - Fixes overlays glitching on fullscreen on Avalonia.
  - Fixes an issue where a previous frame would sometimes show up on games played with the new UI.
  - Allows switching graphics backends and preferred GPU on Avalonia without requiring a restart of the emulator.

## 1.1.273 - 2022-09-19
### Added:
- Implemented in IR the managed methods of the ShlReg region of the SoftFallback class.
  - No known changes in games.

## 1.1.272 - 2022-09-14
### Added:
- A32/T32/A64: Implement Hint instructions (CSDB, SEV, SEVL, WFE, WFI, YIELD).
  - Needed by Hanayaka Nari Waga Ichizoku Modern Nostalgie and Meiji Katsugeki Haikara Ryuuseigumi - Seibai Shimaseu, Yonaoshi Kagyou.

## 1.1.271 - 2022-09-14
### Fixed:
- Periodically Flush Commands for Vulkan.
  - Improves performance on Pokémon Sword/Shield and The Legend of Zelda: Breath of the Wild when using Vulkan.

## 1.1.270 - 2022-09-14
### Fixed:
- Fix partial unmap reprotection on Windows.
  - Fixes a regression in Super Smash Bros. Ultimate that caused some punctuation text to be missing.

## 1.1.269 - 2022-09-13
### Added:
- Implement PLD and SUB (imm16) on T32, plus UADD8, SADD8, USUB8 and SSUB8 on both A32 and T32.
  - Allows more applications to boot through Vita2HOS.
  - May increase compatibility with other 32-bit titles and homebrew.

## 1.1.268 - 2022-09-13
### Added:
- T32: Implement Asimd instructions.
  - Allows VITA-8 to boot through Vita2HOS.
  - May increase compatibility with other 32-bit titles and homebrew.

## 1.1.267 - 2022-09-13
### Fixed:
- Fix bindless 1D textures having a buffer type on the shader.
  - Fixes black screen on Prinny: Can I Really Be the Hero? and Prinny 2: Dawn of Operation Panties, Dood!

## 1.1.266 - 2022-09-13
### Fixed:
- Fix increment on Arm32 NEON VLDn/VSTn instructions with regs > 1.
  - No More Heroes and No More Heroes 2: Desperate Struggle now go in-game.
  - Fixes several missing visual effects in Pikmin 3 Deluxe.
  - Fixes bad voice audio quality in Ni no Kuni Wrath of the White Witch and bad sound quality in Double Dragon Neon and Sky Gamblers: Storm Raiders.
  - Fixes a crash in Valkyria Chronicles when attempting to view the "Encounter at Bruhl" episode.

## 1.1.265 - 2022-09-13
### Fixed:
- Fix R4G4B4A4 format on Vulkan.
  - Fixes text rendering in Ni no Kuni Wrath of the White Witch and Ys VIII: Lacrimosa of Dana.
  - Fixes menu icons in Super Kirby Clash and Vroom in the Night Sky.

## 1.1.264 - 2022-09-11
### Fixed:
- Scale SamplesPassed counter by RT scale on report.
  - Fixes gameplay issues caused by resolution scaling in Splatoon 2 and Splatoon 3, namely specials charging up faster and points being multiplied at higher resolutions, and being unable to swim in ink at lower resolutions.

## 1.1.263 - 2022-09-11
### Fixed:
- Implement VRINT (vector) Arm32 NEON instructions.
  - Ni no Kuni Wrath of the White Witch now goes in-game, though it requires a save file.

## 1.1.262 - 2022-09-10
### Fixed:
- T32: Add Vfp instructions.
  - Allows the triangle homebrew to work on Vita2HOS.

## 1.1.261 - 2022-09-10
### Fixed:
- Implement Thumb (32-bit) memory (ordered), multiply, extension and bitfield instructions.
  - Allows Vita2HOS to go a bit further when launching applications.

## 1.1.260 - 2022-09-10
### Fixed:
- Optimize placeholder manager tree lookup.
  - Reduces the time to stop emulation or close the program when running games with a large amount of memory mappings.
  - Affected games include: Shin Megami Tensei V, Triangle Strategy and possibly some UE4 titles.

## 1.1.259 - 2022-09-10
### Fixed:
- Do not output ViewportIndex on SPIR-V if GPU does not support it.
  - Fixes a crash in Super Smash Bros. Ultimate on older GPUs (pre-Maxwell) using Vulkan.

## 1.1.258 - 2022-09-10
### Fixed:
- Rebind textures if format changes or they're buffer textures.
  - Fixes a regression in Mario Party Superstars in the spotlight minigame where the red spotlight would not render.
  - May affect other titles similarly affected by the regression.

## 1.1.257 - 2022-09-09
### Fixed:
- Allocate work buffer for audio renderer instead of using guest supplied memory.
  - Fixes an access violation crash on Urban Trial Tricky that started happening on 1.1.100.
  - Fixes a crash on boot on Mutant Year Zero: Road to Eden.

## 1.1.256 - 2022-09-09
### Added:
- Add ADD (zx imm12), NOP, MOV (rs), LDA, TBB, TBH, MOV (zx imm16) and CLZ thumb instructions.
  - Allows Vita2HOS to launch again.

## 1.1.255 - 2022-09-09
### Fixed:
- Implement VRSRA, VRSHRN, VQSHRUN, VQMOVN, VQMOVUN, VQADD, VQSUB, VRHADD, VPADDL, VSUBL, VQDMULH and VMLAL Arm32 NEON instructions.
  - Allows Baldur's Gate/ Baldur's Gate II Enhanced Editions, Dies irae -Amantes amentes-, Planescape: Torment/ Icewind Dale Enhanced Editions and Star Wars: Republic Commando to go in-game, possibly fixes other 32-bit games.

## 1.1.254 - 2022-09-08
### Fixed:
- Restride vertex buffer when stride causes attributes to misalign in Vulkan.
  - Fixes vertex explosions in Splatoon 3: Splatfest World Premiere on AMD graphics on Windows, and a crash on Mesa drivers. May improve other games that suffered from vertex explosions on AMD GPUs. 

## 1.1.253 - 2022-09-08
### Fixed:
- Clean up rejit queue.
  - Code cleanup. No expected changes in games. 

## 1.1.252 - 2022-09-08
### Fixed:
- Implemented in IR the managed methods of the Saturating region of the SoftFallback class (the SatQ ones).
  - Greatly improves performance of pre-rendered cutscenes in Astral Chain, Crash Team Racing Nitro-Fueled, Mario + Rabbids: Kingdom Battle, Tony Hawk's Pro Skater 1 + 2, and possibly other games. 

## 1.1.251 - 2022-09-07
### Fixed:
- Transform shader LDC into constant buffer access if offset is constant.
  - Fixes intermittent black screen in Ys VIII: Lacrimosa of DANA.

## 1.1.250 - 2022-09-07
### Fixed:
- bsd: improve socket poll.
  - No expected changes in games.

## 1.1.249 - 2022-09-07
### Added:
- bsd: implement SendMMsg and RecvMMsg.
  - No expected changes in games.

## 1.1.248 - 2022-09-01
### Fixed:
- Bsd: Fix NullReferenceException in BsdSockAddr.FromIPEndPoint().
  - Fixes a crash with Guest Internet Access enabled in Victor Vran Overkill Edition.

## 1.1.247 - 2022-09-01
### Fixed:
- Change vsync signal to happen at 60hz, regardless of swap interval.
  - Fixes voice lines becoming delayed during cutscenes in Tokyo Mirage Sessions #FE Encore.
  - Might fix some game speed issues in The Legend of Zelda: Link's Awakening and Breath of the Wild.

## 1.1.246 - 2022-09-01
### Fixed:
- bsd: Fix Poll(0) returning ETIMEDOUT instead of SUCCESS.
  - No expected changes in games.

## 1.1.245 - 2022-09-01
### Fixed:
- sfdsnres: fix endianess issue for port serialisation.
  - No expected changes in games.

## 1.1.244 - 2022-08-31
### Added:
- account: Implement LoadNetworkServiceLicenseKindAsync.
  - Required for Pokémon Legends: Arceus v1.1.1 to run with Guest Internet Access enabled. The game does not yet boot with this option on, as it requires another change as well.

## 1.1.243 - 2022-08-28
### Fixed:
- Bsd: Fix ArgumentOutOfRangeException in SetSocketOption.
  - Allows Minecraft to boot with "enable guest internet access" DISABLED. Enabling will still cause a crash.

## 1.1.242 - 2022-08-28
### Changed:
- Replace image format magic numbers with enums.
  - Refactors GPU texture format tables to match official NVIDIA open-source headers.
  - Vertex attribute formats are now represented with their own enum.
  - No expected changes in games.

## 1.1.241 - 2022-08-27
### Fixed:
- Avalonia - Update Japanese translation.
  - Brings the Japanese locale up to date for the Avalonia UI.

## 1.1.240 - 2022-08-26
### Fixed:
- Optimize kernel memory block lookup and consolidate RBTree implementations.
  - No known changes, though it might positively affect UE4 games.

## 1.1.239 - 2022-08-26
### Fixed:
- Avalonia - Update Turkish Translation.
  - Updates Turkish localization for the Avalonia UI.

## 1.1.238 - 2022-08-26
### Fixed:
- Update de_DE.json.
  - Updates German localization for the Avalonia UI.

## 1.1.237 - 2022-08-26
### Fixed:
- Update zh_CN.json.
  - Updates simplified Chinese localization for the Avalonia UI.

## 1.1.236 - 2022-08-26
### Fixed:
- Avalonia - Add Polish Translation.
  - Adds a Polish localization for the Avalonia UI.

## 1.1.235 - 2022-08-26
### Fixed:
- Avalonia - Display language names in their corresponding language under "Change Language".
  - Changes language names to their native ones in the Avalonia UI. Before, they were all in English.

## 1.1.234 - 2022-08-26
### Fixed:
- bsd: Fix Poll writting in input buffer.
  - Fixes an oversight in the code. No expected changes to emulator functionality.

## 1.1.233 - 2022-08-26
### Fixed:
- Fast path for Inline-to-Memory texture data transfers.
  - Fixes texture corruption on games that use OpenGL on the Switch, such as Blossom Tales II, Digimon Story Cyber Sleuth, Layton's Mystery Journey, River City Girls Zero, Super Perils of Baking, and more.

## 1.1.232 - 2022-08-25
### Fixed:
- pctl: Implement EndFreeCommunication.
  - Fixes a parental controls service crash in Among Us, Colors Live, Game Builder Garage and Splatoon 3: Splatfest World Premiere.

## 1.1.231 - 2022-08-25
### Fixed:
- misc: Fix missing null terminator for strings with pchtxt.
  - No changes in games.

## 1.1.230 - 2022-08-25
### Added:
- ARMeilleure: Hardware accelerate SHA256.
  - No known changes in games.

## 1.1.229 - 2022-08-25
### Added:
- Implement some 32-bit Thumb instructions.
  - Implements LDM/STM, LDAEX/STLEX, LDR/STR (with register offset shifted by immediate) and LDRD/STRD instructions.
  - No known changes in games.

## 1.1.228 - 2022-08-24
### Fixed:
- Update PPTC dialog text to match label and tooltip.
  - The warning box now properly states that it's queuing a PPTC rebuild and not deleting it.

## 1.1.227 - 2022-08-21
### Fixed:
- Check if game directories have been updated before refreshing GUI.
  - Prevents both UIs from reloading the games list every time settings are updated, and refreshes only if the game folder changes.

## 1.1.226 - 2022-08-20
### Fixed:
- Use RGBA16 vertex format if RGB16 is not supported on Vulkan.
  - Xenoblade Chronicles 3 now boots on AMD graphics cards.

## 1.1.225 - 2022-08-19
### Fixed:
- Change 'Purge PPTC Cache' label & tooltip to reflect function behavior.
  - It is now named "Queue PPTC Rebuild" as the option doesn't purge it completely.

## 1.1.224 - 2022-08-19
### Fixed:
- A few minor documentation fixes.
  - Small code cleanup. No changes in games.

## 1.1.223 - 2022-08-18
### Removed:
- Removed unused usings.
  - Small code cleanup. No changes in games.

## 1.1.222 - 2022-08-17
### Fixed:
- Skipped over the last "Count" key explicitly.
  - Small code cleanup. No changes in games.

## 1.1.221 - 2022-08-17
### Fixed:
- Fix SpirV parse failure.
  - Fixes the Mii editor applet on Vulkan.

## 1.1.220 - 2022-08-17
### Removed:
- Removed extra semicolons.
  - Minor code cleanup. No expected changes.

## 1.1.219 - 2022-08-16
### Fixed:
- Avalonia - Couple fixes and improvements to Vulkan.
  - Fixes a crash that occurred when toggling fullscreen.
  - Adds fallback to OpenGL if Vulkan is not available.
  - Adds swapchain present mode control to GTK.
  - Fixes screenshot feature on Avalonia Vulkan.
  - Fixes favorites not being saved on Avalonia.

## 1.1.218 - 2022-08-16
### Added:
- Vulkan: Add ETC2 texture formats.
  - Adds missing texture formats to Vulkan. On supported Intel and AMD GPUs, games that use these formats, such as Radiation Island or Vegas Party, should now work on Vulkan.

## 1.1.217 - 2022-08-15
### Added:
- am: Stub SetWirelessPriorityMode, SaveCurrentScreenshot and GetHdcpAuthenticationState.
  - Fixes Xenoblade Chronicles 3 photo gallery crash.
  - Fixes Hulu app crashing on startup.

## 1.1.216 - 2022-08-15
### Fixed:
- ControllerApplet: Override player counts when SingleMode is set
  - Reduces controller applet spam in certain titles such as Splatoon 2, Xenoblade Chronicles 3 and other titles that don't like multiple connections.
  - Any title that had significant controller applet log spam may be helped.

## 1.1.215 - 2022-08-14
### Fixed:
- PreAllocator: Check if instruction supports a Vex prefix in IsVexSameOperandDestSrc1.
  - No changes expected in games.

## 1.1.214 - 2022-08-14
### Fixed:
- Fix texture bindings using wrong sampler pool in some cases.
  - Fixes a regression that caused flickering in Animal Crossing: New Horizons, Atelier Ryza (only with Vsync disabled) and No More Heroes 3.

## 1.1.213 - 2022-08-11
### Fixed:
- OpenGL: Limit vertex buffer range for non-indexed draws.
  - Fixes the triangle glitch on fog/smoke in Super Mario Odyssey.
  - Fixes a TDR/driver crash in Xenoblade Chronicles 3 (only on OpenGL).

## 1.1.212 - 2022-08-11
### Fixed:
- Fix blend with RGBX color formats.
  - Fixes broken blending in La-Mulana.

## 1.1.211 - 2022-08-11
### Fixed:
- Rename ToSpan to AsSpan.
  - Small code cleanup.

## 1.1.210 - 2022-08-11
### Added:
- Add Japanese translation to Avalonia UI.

## 1.1.209 - 2022-08-08
### Fixed:
- OpenGL: Fix clear of unbound color targets.
  - Fixes a regression that caused New Super Mario Bros U Deluxe to crash with a NullReferenceException. May fix other games with the same problem.

## 1.1.208 - 2022-08-05
### Fixed:
- Implement Arm32 Sha256 and MRS Rd, CPSR instructions.
  - Mario Kart 8 Deluxe with update 2.1.0 is now playable.

## 1.1.207 - 2022-08-04
### Added:
- Implement HLE macros for render target clears.
  - Adds a HLE macro for render target clears (color and depth-stencil).
  - May result in a minor performance improvement on games that render to array or 3D textures.

## 1.1.206 - 2022-08-03
### Fixed:
- Fix Multithreaded Compilation of Shader Cache on OpenGL.
  - Fixes a regression from 1.1.200 that caused OpenGL to build caches at boot on a single thread. Now it's properly multithreaded again.

## 1.1.205 - 2022-08-02
### Fixed:
- Sfdnsres: Stub ResolverSetOptionRequest.
  - Fixes a crash on boot in Ark: Survival Evolved when Guest Internet Access is enabled.
  - Allows Danger Mouse to go in-game.

## 1.1.204 - 2022-08-02
### Fixed:
- Fix resolution scale values not being updated.
  - Fixes a regression that caused graphical glitches on Xenoblade games when using resolution scaling.

## 1.1.203 - 2022-08-02
### Fixed:
- Fix geometry shader passthrough fallback being used when feature is supported.
  - Fixes a regression on Marvel Ultimate Alliance 3 on Maxwell and newer NVIDIA GPUs when using OpenGL.

## 1.1.202 - 2022-08-02
### Fixed:
- SPIR-V: Initialize undefined variables with 0.
  - Fixes tilt shift blur effect in The Legend of Zelda: Link's Awakening on NVIDIA GPUs.
  - Fixes block flickering in Splatoon 2 on newer NVIDIA GPUs.

## 1.1.201 - 2022-08-01
### Fixed:
- Fix a crash occurring when trying to launch a game with Vulkan on FlatHub releases.

## 1.1.200 - 2022-07-31
### Added:
- Vulkan backend.
- Implemented a Vulkan graphics backend. You can now switch between OpenGL and Vulkan in Settings > Graphics > Graphics Backend.
  - Implemented a GPU selector in the same menu, labeled "Preferred GPU", for systems with more than one graphics card. Keep in mind you can only select the GPU that Vulkan will use, not the one OpenGL will use. 
  - Added a "Texture Recompression" option in graphics settings which, when enabled, will reduce VRAM usage in exchange for slightly worse texture quality (affects both Vulkan and OpenGL). We recommend this for graphics cards that have less than 4GB VRAM.
  - When using Vulkan, AMD and Intel GPUs will see large improvements in compatibility and performance across the board. Use latest graphics drivers for the best experience.
  - Implemented SPIR-V shader backend. Reduces shader compilation times considerably for all GPU vendors, compared to OpenGL's GLSL backend. This results in much less stuttering on first runs.
  - Vulkan supports supersampling at higher than 2x the display resolution, which acts as antialiasing when the rendering resolution is higher than the display's, whereas OpenGL only supports it up to 2x the screen resolution.
  - Vulkan may not have graphical glitches that OpenGL has, such as the co-op player 2 screen in Fire Emblem Warriors: Three Hopes.
  - Certain games, such as Pokémon Legends: Arceus, Pokkén Tournament, Super Mario Odyssey and The Legend of Zelda: Breath of the Wild, have shown slightly better performance on Nvidia Vulkan than on Nvidia OpenGL. 
  - Shader caches from before this change will be deleted, and new shader caches will be starting from zero. This is due to Vulkan and OpenGL caches now being shared.
  - Shader caches built with Vulkan will now be usable with OpenGL and vice versa.
  - Vulkan shaders do not require to be rebuilt after driver updates; however, OpenGL shaders still do.

## 1.1.199 - 2022-07-29
### Fixed:
- Move partial unmap handler to the native signal handler.
  - Greatly improves performance and reduces stuttering on Windows 11.

## 1.1.198 - 2022-07-29
### Fixed:
- Minor Avalonia UI verbiage/case fixes Across languages.

## 1.1.197 - 2022-07-28
### Fixed:
- Avalonia: Another Cleanup.
  - Fixes crashes in the Avalonia UI's DLC Manager that would occur when managing DLC for a game that already had DLC added to it, when selecting 1 or more DLC files and adding them, or when clicking "Remove", "Remove All" or "Save" for titles with no DLC.

## 1.1.196 - 2022-07-28
### Fixed:
- Avalonia: Cleanup UserEditor a bit.
  - Small code cleanup for the user profile editor in the Avalonia UI.

## 1.1.195 - 2022-07-28
### Fixed:
- Fix DMA linear texture copy fast path.
  - Fixes a crash in SD Gundam Battle Alliance Demo.

## 1.1.194 - 2022-07-27
### Added:
- Add a sampler pool cache and improve texture pool cache.
  - Improves performance on Super Zangyura.

## 1.1.193 - 2022-07-25
### Added:
- Backport Avalonia menu/settings tooltips to GTK where possible.
  - New tooltips from the Avalonia UI are now on the current UI as well.

## 1.1.192 - 2022-07-25
### Changed:
- misc: Reformat Ryujinx.Audio with dotnet-format.

## 1.1.191 - 2022-07-24
### Added:
- Resolution scaling hotkeys.
  - Adds hotkeys for changing resolution scaling while a game is running. One increases resolution by a factor of 1 up to 4x; the other decreases resolution by a factor of 1 up to 1x. The hotkeys aren't configured by default, requiring the user to set them up on Avalonia.

## 1.1.190 - 2022-07-24
### Fixed:
- Add support for conditional (with CC) shader Exit instructions.
  - Fixes bloom in Tokyo Mirage Sessions. 

## 1.1.189 - 2022-07-24
### Added:
- feat: add traditional chinese translate (Avalonia).
  - Adds a traditional Chinese localization for the Avalonia UI. 

## 1.1.188 - 2022-07-24
### Fixed:
- Avalonia - Make menuitems toggleable on textclick.
  - Makes it so checkboxes can be enabled/disabled when pressing on their corresponding text on the Avalonia GUI. 

## 1.1.187 - 2022-07-24
### Changed:
- Avalonia - Use content dialog for user profile manager.
  - Moves the user profile window and related windows to a single content dialog on the Avalonia GUI. 

## 1.1.186 - 2022-07-24
### Fixed:
- fix: Ensure to load latest version of ffmpeg libraries first.
  - Fixes a crash related to loading an older version of ffmpeg, instead of the one shipped with the emulator. 

## 1.1.185 - 2022-07-23
### Fixed:
- Minor GTK & Avalonia UI verbiage/case fixes.
  - Small text adjustments in the UIs. 

## 1.1.184 - 2022-07-23
### Fixed:
- Fix decoding of block after shader BRA.CC instructions without predicate.
  - Fixes green lights in Jump Force. 

## 1.1.183 - 2022-07-23
### Fixed:
- Avoid adding shader buffer descriptors for constant buffers that are not used.
  - May slightly improve performance in some games. 

## 1.1.182 - 2022-07-15
### Fixed:
- Avoid scaling 2d textures that could be used as “3d“.
  - Fixes red-tinted textures when upscaling in Agatha Christie: Hercule Poirot - The First Cases, A Hat in Time, Cruis'n Blast, Demon Gaze Extra, Far: Changing Tides, Lost in Random, Pascal's Wager: Definitive Edition, Sherlock Holmes: Devil's Daughter, World's End Club, possibly more. 

## 1.1.181 - 2022-07-14
### Fixed:
- Reduce some unnecessary allocations in DMA handler.
  - Reduces load times slightly and reduces stutters during pre-recorded videos. 

## 1.1.180 - 2022-07-14
### Fixed:
- Remove dependency for FFmpeg.AutoGen and Update FFmpeg to 5.0.1 for Windows.
  - Fixes games crashing on Linux whenever an mpeg pre-rendered video would play. 

## 1.1.179 - 2022-07-14
### Added:
- BSD: Allow use of DontWait flag in Receive.
  - No known changes in games. 

## 1.1.178 - 2022-07-12
### Changed:
- Ava/MainWindow: Do not show Show Console menu item on non-Windows.
  - Hides "Show Console" on Linux in the Avalonia UI. 

## 1.1.177 - 2022-07-11
### Fixed:
- Handle the case where byte size option values are sent to BSD.
  - Fixes a crash in the Super Mario Odyssey online mod when connecting to the server. Note that the SMO online mod still may not work properly.

## 1.1.176 - 2022-07-11
### Fixed:
- Avalonia - Add border to Flyouts.
  - Adds a border to flyouts (menus, dropdowns, etc) to easily tell them from the background. 

## 1.1.175 - 2022-07-11
### Fixed:
- Propagate Shader phi nodes with the same source value from all blocks.
  - Fixes flickering in Monster Hunter Rise: Sunbreak (still requires further changes to get in-game). 

## 1.1.174 - 2022-07-11
### Fixed:
- Avalonia - Make tooltips more useful and descriptive, update Spanish localization.
  - Expands several tooltips to better explain what their respective settings do, and updates the Spanish localization accordingly. 

## 1.1.173 - 2022-07-11
### Fixed:
- Avalonia - Couple fixes and improvements.
  - Fixes a crash in the Avalonia UI when bringing up the autoupdater.
  - Reduces size of cheat window.
  - Enables Tiered Compilation (speeds up Avalonia UI startup time).
  - Removes compiler warnings from the Avalonia project.

## 1.1.172 - 2022-07-11
### Fixed:
- Avalonia - Further Optimize Chinese Translation.
  - Updates the Simplified Chinese localization for Avalonia.

## 1.1.171 - 2022-07-08
### Added:
- UI - Avalonia Part 3.
  - Adds the remaining Avalonia windows. The UI is now at parity with the current GTK UI.

## 1.1.170 - 2022-07-08
### Fixed:
- Avalonia - Use loaded config when assigning controller input.
  - Fixes a crash in the upcoming Avalonia UI that occurs when mapping controller input while a config hasn't been saved for that controller.

## 1.1.169 - 2022-07-08
### Fixed:
- Avalonia - Ensure mouse cursor is only hidden when mouse is in renderer.
  - Fixes a bug in the upcoming Avalonia UI where the mouse cursor wouldn't be hidden properly. 

## 1.1.168 - 2022-07-08
### Changed:
- Relicense Ryujinx.Audio under the terms of the MIT license
  - Adjusts the licence of Amadeus from LGPLv3 to MIT.

## 1.1.167 - 2022-07-08
### Fixed:
- Fix deadlock in mouse input on Avalonia.
  - Fixes a deadlock in the upcoming Avalonia UI that occurs if you open any window while direct mouse input is enabled.

## 1.1.166 - 2022-07-06
### Fixed:
- Fix Vi managed and stray layers open/close/destroy.
  - Portal and Portal 2 are now playable.

## 1.1.165 - 2022-07-06
### Added:
- Implement CPU FCVT Half <-> Double conversion variants.
  - Required by Portal and Portal 2 (however they still require further changes to get in-game). 

## 1.1.164 - 2022-07-05
### Fixed:
- Add support for alpha to coverage dithering.
  - Fixes missing dithering (semi-transparency) effect on objects close to the camera and at the edges of the draw distance in Pokémon Legends: Arceus. 

## 1.1.163 - 2022-07-05
### Added:
- UI - Avalonia Part 2.
  - Adds settings window and subsequent windows and controls to the upcoming Avalonia-based user interface. 

## 1.1.162 - 2022-07-03
### Added:
- ptm: Stub GetTemperature.
  - Stubs GetTemperature service needed by the latest version of nx-hbmenu (Homebrew menu).

## 1.1.161 - 2022-07-02
### Fixed:
- Bindless elimination for constant sampler handle.
  - Allows the Monster Hunter Rise: Sunbreak update to render (still requires further changes to get in-game). 

## 1.1.160 - 2022-06-29
### Fixed:
- ui: Fix timezone abbreviation since #3361.
  - Fixes timezone abbreviation text in system settings. 

## 1.1.159 - 2022-06-25
### Added:
- Add Simplified Chinese to Avalonia (V2).
  - Adds a Chinese localization to the upcoming Avalonia UI. 

## 1.1.158 - 2022-06-25
### Fixed:
- Account for pool change on texture bindings cache.
  - Fixes a regression from 1.1.149 that caused garbled textures on Super Zangyura. 

## 1.1.157 - 2022-06-24
### Fixed:
- timezone: Fix regression caused by #3361.
  - Fixes games that were crashing due to the change in 1.1.156. 

## 1.1.156 - 2022-06-24 [Unpublished]
### Fixed:
- time: Make TimeZoneRule blittable and avoid copies.
  - No known changes in games. 

## 1.1.155 - 2022-06-24
### Fixed:
- Fix ThreadingLock deadlock on invalid access and ExitProcess.
  - Fixes a specific case of the emulator freezing when closing. Does not fix all instances where this happens, however. 

## 1.1.154 - 2022-06-24
### Fixed:
- Ensure texture ID is valid before getting texture descriptor.
  - Fixes a crash in A Hat in Time that would occur after progressing past a certain point in the game.

## 1.1.153 - 2022-06-23
### Changed:
- UI: Some Avalonia cleanup.
  - Cleans up some of the new GUI code. No changes to emulator functionality.

## 1.1.152 - 2022-06-22
### Changed:
- Rewrite kernel memory allocator.
  - Cleans up the kernel memory allocator code. No changes expected in games.

## 1.1.151 - 2022-06-20
### Fixed:
- Fix doubling of detected gamepads on program start.
  - May fix some instances of controller duplicates appearing on the Input Device dropdown.

## 1.1.150 - 2022-06-17
### Fixed:
- Account for res scale changes when updating bindings.
  - Fixes graphical regression when scaling certain games (XCDE/XC2).

## 1.1.149 - 2022-06-17
### Changed:
- Optimize Texture Binding and Shader Specialization Checks.
  - Improves performance in Super Mario Odyssey, The Legend of Zelda: Breath of the Wild, Xenoblade Chronicles Definitive Edition, and possibly others.

## 1.1.148 - 2022-06-17
### Fixed:
- Fix VIC out of bounds copy.
  - Fixes a video crash in LOOPERS.

## 1.1.147 - 2022-06-14
### Fixed:
- Support Array/3D depth-stencil render target, and single layer clears.
  - Fixes missing crowd in Mario Strikers: Battle League.

## 1.1.146 - 2022-06-12
### Fixed:
- Less invasive fix for EventFd blocking operations.
  - Return to single-thread approach for handling sockets.
  - Fixes issues in some games (Pokémon Sword/Shield) where a Hipc response error would crash early into launching.

## 1.1.145 - 2022-06-11
### Fixed:
- Allow concurrent BSD EventFd read/write.
  - Fixes a regression in Diablo II: Resurrected where the game would just hang on a black screen on boot.

## 1.1.144 - 2022-06-11
### Fixed:
- Ignore ClipControl on draw texture fallback.
  - Fixes some games rendering upside-down on AMD and Intel graphics cards, such as Moero Chronicle Hyper. Nvidia is unaffected.

## 1.1.143 - 2022-06-10 
### Fixed:
- Fix instanced indexed inline draw index count.
  - Fixes index count used on the draw passing the count for a single instance. 
  - Fixes performance issues in the 3D sections on Genkai Tokki Moero Crystal H. 

## 1.1.142 - 2022-06-06
### Fixed:
- Fix instanced indexed inline draws.
  - Fixes remaining issues with 3D sections in Genkai Tokki Moero Crystal H. Also fixes performance drops in the game.

## 1.1.141 - 2022-06-05
### Fixed:
- Remove freed memory range from tree on memory block disposal.
  - Fixes an issue where the emulator could crash after stopping emulation and starting another game afterwards.

## 1.1.140 - 2022-06-05
### Fixed:
- Extend uses count from ushort to uint on Operand Data structure.
  - Taiko Risshiden V DX now goes in-game.

## 1.1.139 - 2022-06-05
### Fixed:
- Copy dependency for multisample and non-multisample textures.
  - Fixes black screen in Perky Little Things. 
  - Partially fixes 3D sections in Genkai Tokki Moero Crystal H.

## 1.1.138 - 2022-06-04
### Fixed:
- Fix a potential GPFIFO submission race.
  - No expected changes in games.

## 1.1.137 - 2022-06-02
### Fixed:
- Fix 3D semaphore counter type 0 handling.
  - Fixes a bug where 0 would be released from counter instead of a semaphore payload.
  - The Elder Scrolls V: Skyrim now goes in-game.

## 1.1.136 - 2022-06-01
### Changed:
- infra: Switch to win10-x64 RID and fix PR comment for Avalonia and SDL2 artifact rename.
  - Windows Ryujinx builds now target Windows 10/11.
  - Windows 7, 8 and 8.1 are no longer supported.
  - Avalonia builds posted on PRs by the GitHub bot will be hidden under an "Experimental GUI (Avalonia)" tab. 
  - Headless builds will move back under the "GUI-less (SDL2)" tab. 

## 1.1.135 - 2022-05-31
### Changed:
- Rewrite SVC handler using source generators rather than IL emit.
  - Replace all instances of Reflection.Emit from the codebase with new source generators for runtime code generation.
  - Ryujinx codebase should now be eligible for .NET Ahead-of-Time compilation.
  - Fixes black screen deadlock on boot in Genkai Tokki Moero Crystal H.

## 1.1.134 - 2022-05-31
### Changed:
- Refactor CPU interface to allow the implementation of other CPU emulators.
  - Refactors the existing CPU related interfaces (and also adds new ones) to allow other CPU emulators to be implemented. This includes not only JIT-based emulators, but also hypervisors (for example, Apple Hypervisor).
  - No expected changes in games.

## 1.1.133 - 2022-05-31
### Fixed:
- Allow loading NSPs without a NCA inside.
  - Homebrew applications that are packed as NSP files can now boot.

## 1.1.132 - 2022-05-21
### Fixed:
- Don't force DPI aware on Avalonia.
  - Fixes an issue where per-monitor DPI was not working on the new UI. Does not affect the current UI.

## 1.1.131 - 2022-05-18
### Fixed:
- Fix audio renderer error message result code base.
  - Changes how this specific error is displayed on the console. No changes to emulator functionality.

## 1.1.130 - 2022-05-16
### Fixed:
- UI - Scale end framebuffer blit.
  - Fixes rendering when desktop scaling is over 150%.

## 1.1.129 - 2022-05-15 
### Fixed:
- Fixes the Avalonia updater.
  - Updates the auto-updater code to include the Avalonia paths.


## 1.1.128 - 2022-05-15 
### Fixed:
- Fix Amiibo image path.
  - Fixes a regression that caused crashing when an Amiibo was scanned.

## 1.1.127 - 2022-05-15 
### Fixed:
- gh-actions: Prefix Avalonia builds with test- and disable pre-release.
  - Fixes the updater downloading the wrong Ryujinx build. 

## 1.1.126 - 2022-05-15 
### MISC:
- Pre-release build.

## 1.1.125 - 2022-05-15 
### Added:
- Add Avalonia builds to release.
  - Avalonia builds will now be downloadable on GitHub PR artifacts.

## 1.1.124 - 2022-05-15 
### Fixed:
- misc: Clean up of CS project after Avalonia merge.
  - No expected changes in emulator functionality.

## 1.1.123 - 2022-05-15 
### Fixed:
- sdl2: Update to Ryujinx.SDL2-CS 2.0.22.
  - Fixes G-Shark gamepads.
  - Fixes wired PowerA GameCube controllers.
  - Fixes broken motion controls on Linux.
  - Likely fixes compatibility with more unofficial controllers.

## 1.1.122 - 2022-05-15
### Added:
- Avalonia UI: Part 1
  - Implements the foundations for the UI update to Avalonia.
  - Further parts will be merged before the UI is active.

## 1.1.121 - 2022-05-14
### Fixed:
- Prefetch capabilities before spawning translation threads.
  - Fixes a race condition that could cause games to crash when recompiling shaders.

## 1.1.120 - 2022-05-12
### Fixed:
- Implement Viewport Transform Disable.
  - Fixes the interface in Dragon Quest Builders.
  - Fixes the title screen in River City Girls Zero.
  - Fixes a regression that caused broken menus in Zombies Ate My Neighbors and Ghoul Patrol.
  - Fixes save slot thumbnails and screen copies in the NSO N64 emulator (Mario Kart 64 monitors), screen copies in the Citra RetroArch core, icons in RetroArch, possibly other similar bugs.

## 1.1.119 - 2022-05-07
### Added:
- hid: Various fixes and cleanup.
  - Implements and cleans up various hid functions and services. 
  - RetroArch and likely other similar homebrew are now bootable.

## 1.1.118 - 2022-05-05
### Fixed:
- Add alternative "GL" enum values for StencilOp.
  - Fixes some broken graphics in the Citra RetroArch core, possibly fixes graphics in other homebrew applications.

## 1.1.117 - 2022-05-05 
### Added:
- Enable JIT service LLE.
  - Enables the JIT service, required by the NSO Nintendo 64 emulator and Super Mario 3D All-Stars (Super Mario 64), allowing them to run. It is not an actual service implementation, rather it runs the service on the firmware, so this is an "LLE" approach as opposed to the usual HLE approach where the service is re-implemented on the emulator.
  - Requires firmware version 10.0.0 minimum.

## 1.1.116 - 2022-05-05 
### Fixed:
- Fix shared memory leak on Windows.
  - Fixes a memory leak that would occur when stopping and restarting emulation. 

## 1.1.115 - 2022-05-04 
### Added:
- infra: Warn about support drop of old Windows versions.
  - Shows a warning message to users on Windows 7, 8, 8.1 and older Windows 10 versions stating that Ryujinx support for these versions will be dropped starting June 1st, 2022.

## 1.1.114 - 2022-05-04 
### Fixed:
- Remove AddProtection count > 0 assert.
  - Small code correction. This change only affects debug builds. 

## 1.1.113 - 2022-05-03 
### Changed:
- Change github build workflow to not use hardcoded versioning
  - Fixes an oversight that caused a few PR builds to display an incorrect version number. 

## 1.1.112 - 2022-05-03 
### Added:
- Implement PM GetProcessInfo Atmosphere extension (partially).
  - Adds support for Skyline + ARCropolis mods. Super Smash Bros Ultimate mods that rely on ARCropolis are now usable on Ryujinx. 

## 1.1.111 - 2022-05-02 
### Added:
- Implement code memory syscalls.
  - Implements code memory related syscalls, used by applications that generate and/or modify code at runtime.
  - Required by emulators that use a JIT (NSO N64) and mods that patch for function hooking game code on-the-fly (Skyline/ARCropolis). Note that neither will work with these changes alone.

## 1.1.110 - 2022-05-02
### Added:
- Support memory aliasing.
  - Increases accuracy of fast memory manager modes, allowing for things like IPC, shared memory, transfer memory and code memory to be implemented properly.
  - Paves the way for running sysmodules with fast memory manager modes enabled, as well as running the NSO Nintendo 64 emulator and the Skyline mod manager for Super Smash Bros Ultimate in the future.
  - Fast memory manager modes will no longer work on Windows 7 and Windows 8.

## 1.1.109 - 2022-05-02 
### Fixed:
- Fix flush action from multiple threads regression.
  - Fixes graphical issues in Catherine: Full Body and Pokémon Legends: Arceus due to a regression introduced in 1.1.107.

## 1.1.108 - 2022-05-01 
### Fixed:
- Restrict cases where vertex buffer size from index buffer type is used.
  - Fixes a regression introduced in 1.1.95 that caused visual glitches on certain particle effects in Xenoblade 2 (visible for instance in Godfrey's awakening).

## 1.1.107 - 2022-04-29
### Fixed:
- Fix various issues with texture sync.
  - Fixes a regression in Xenoblade titles where visuals would randomly flash.
  - May fix random bugs in Breath of the Wild such as "air swimming" or other texture streaming bugs.

## 1.1.106 - 2022-04-20 
### Added:
- T32: Implement load/store single (immediate).
  - No changes expected in games.

## 1.1.105 - 2022-04-20
### Fixed:
- Fix broken motion controls when using SDL2.
  - Fixes motion controls on multiple games such as Mario Kart 8 Deluxe; The Legend Of Zelda: Breath of the Wild; Kirby And The Forgotten Land and many others when enabled using the default SDL2 option.

## 1.1.104 - 2022-04-15
### Added:
- Implement HwOpus multistream functions.
  -  Implements multistream related Opus decoding functions. 
  -  Required by MLB The Show 22 and potentially others.


## 1.1.103 - 2022-04-15
### Fixed:
- ReactiveObject: Handle case when oldValue is null.
  - Fixes a possible null exception in the future Avalonia UI.

## 1.1.102 - 2022-04-10
### Fixed:
- ForceDpiAware: X11 implementation.
  - Makes Ryujinx DPI aware on X11 for Linux.

## 1.1.101 - 2022-04-10 
### Added:
- New shader cache implementation.
  - Rewrites both the memory shader cache and the disk shader cache.
  - Old shaders will automatically be converted to the new format when you first boot a game with an existing shader cache.
  - Fixes a slight performance degradation that could occur over time as more shaders were cached.
  - Closing a game will now be slightly faster as the shader cache no longer needs to be recompressed (since shader caches no longer use .zip archives).
  - It is now possible to close the emulator while shaders are loading. 
  - Fixes crashing due to corrupted shaders. The emulator will now rebuild the broken shaders and boot normally.
  - Bindless textures, used by Mario Party Superstars, Pokémon Brilliant Diamond/Shining Pearl, and the vast majority of UE4 games (No More Heroes 3, Shin Megami Tensei V), can now be cached by the emulator's shader cache. These games will be a lot smoother as a result.
  - Completely fixes long boot times on Pokémon BDSP after 2nd run.
  - Fixes graphical glitches in Yokai Watch 1, possibly other games.
  - Fixes a freeze in the Near Forest in Atelier Sophie 2: The Alchemist of the Mysterious Dream.

## 1.1.100 - 2022-04-09 
### Fixed: 
- Fix tail merge from block with conditional jump to multiple returns
  - Fixes audio and visual slowdowns after scanning an Amiibo in games like Animal Crossing: New Horizons. May help similar issues in other titles.

## 1.1.99 - 2022-04-08 
### Added: 
- Implement VMAD shader instruction and improve InvocationInfo and ISBERD handling.
  - Fixes homebrew that uses Nouveau OpenGL and geometry or tessellation shaders. No known changes in commercial games.

## 1.1.98 - 2022-04-08 
### Fixed:
- Allow copy texture views to have mismatching multisample state.
  - Fixes black screen in Pinball FX3.

## 1.1.97 - 2022-04-08
### Fixed:
- Lop3Expression: Optimize expressions.
  - No changes expected in games.

## 1.1.96 - 2022-04-08
### Changed:
- Remove save data creation prompt.
  - Save data directories will now be created automatically and logged in the console.

## 1.1.95 - 2022-04-08
### Fixed:
- Calculate vertex buffer size from index buffer type.
  - Prevents out of memory errors and crashes on Super Mario 64 (SM3DAS) and Perky Little Things. Note that these games need more fixes to work.

## 1.1.94 - 2022-04-08
### Fixed:
- amadeus: Improve and fix delay effect processing.
  - Reworks the sound delay effect processing and cleans up the code.
  - Fixes a bug in the surround sound code. No known changes in games.

## 1.1.93 - 2022-04-07 
### Fixed:
- HID: Signal event on AcquireNpadStyleSetUpdateEventHandle.
  - Fixes random controller disconnects on Flip Wars.

## 1.1.92 - 2022-04-07 
### Fixed:
- LibHac: Update to 0.16.1.
  - Don't fail when EnsureApplicationSaveData tries to create a temporary storage that already exists. Should allow NSO titles to boot again.
  - Support reading XCI files that contain the initial data/key area.
  - Add key sources for system version 14.0.0

## 1.1.91 - 2022-04-06 
### Added:
- amadeus: Update to REV11.
  - This implements all of the ABI changes from REV11 from the new 14.0.0 firmware update. 
  - To our knowledge no games on the Nintendo Switch use these new features at the current moment, but future games likely will. 

## 1.1.90 - 2022-04-05 
### Fixed:
- Do not clamp SNorm outputs to the [0, 1] range on OpenGL.
  - Fixes reflections and lighting on LEGO Star Wars: The Skywalker Saga.
  - Fixes white geometry in Fast RMX.

## 1.1.89 - 2022-04-04 
### Fixed:
- Implement primitive restart draw arrays properly on OpenGL.
  - Fixes white lines in the sky on some Hatsune Miku: Project DIVA Mega Mix clips.

## 1.1.88 - 2022-04-04 
### Fixed:
- Do not force scissor on clear if scissor is disabled.
  - Fixes menu and text glitches on Kirby and the Forgotten Land, and maybe other titles with similar problems.

## 1.1.87 - 2022-04-04 
### Fixed:
- Small graphics abstraction layer cleanup.
  - No known changes in games.

## 1.1.86 - 2022-04-04 
### Fixed:
- Fix shader textureSize with multisample and buffer textures.
  - Fixes graphical issues in Rune Factory 5 and Bubble Bobble 4 Friends.

## 1.1.85 - 2022-03-26 
### Changed:
- infra: Put SDL2 headless release inside a GUI-less block in PR.
  - Download links of PR builds without an user interface will now be hidden. This avoids people downloading them unknowingly.

## 1.1.84 - 2022-03-23 
### Fixed:
- Support NVDEC H264 interlaced video decoding and VIC deinterlacing.
  - Fixes videos in non-Japanese versions of Layton's Mystery Journey. The game now plays them instead of crashing.
  - Fixes every video in Star Wars Episode I: Racer.

## 1.1.83 - 2022-03-22 
### Fixed:
- hle: Some cleanup.
  - Cleaned up the HLE and VirtualFileSystem folders in the code. No changes expected in games.

## 1.1.82 - 2022-03-21 
### Fixed:
- Memory.Tests: Make Multithreading test.
  - Makes the intermittent test failure more explicit so it’s not confused with other errors. 

## 1.1.81 - 2022-03-20 
### Fixed:
- Don't restore Viewport 0 if it hasn't been set yet.
  - Fixes a driver crash when starting some games, introduced in 1.1.79.
  - Games that were black screening with a GPU syncpoint error should now boot correctly (Triangle Strategy, DBFZ etc.)

## 1.1.80 - 2022-03-20 
### Fixed:
- De-tile GOB when DMA copying from block linear to pitch kind memory regions.
  - Fixes texture corruption on games that use OpenGL on the Switch, including Cartoon Network: Battle Crashers, Digimon Story Cyber Sleuth: Complete Edition, Ghoul Patrol (partially), Professor Layton's Mystery Journey, Snack World: The Dungeon Crawl, Zombies Ate My Neighbours (partially), among others.

## 1.1.79 - 2022-03-20 
### Fixed:
- Fix OpenGL issues with RTSS overlays and OBS Game Capture
  - RTSS and overlays that use it should no longer cause certain textures to load incorrectly (Mario Kart 8, Pokémon Legends Arceus).
  - OBS Game Capture should no longer crop the game output incorrectly, flicker randomly, or capture with incorrect gamma.

## 1.1.78 - 2022-03-20 
### Fixed:
- oslc: Fix condition in GetSaveDataBackupSetting.
  - Fixes a regression introduced in 1.1.69 where Animal Crossing: New Horizons would not boot anymore without a save file. Note that the game still crashes most of the time without one.

## 1.1.77 - 2022-03-19 
### Fixed:
- InstEmitMemoryEx: Barrier after write on ordered store.
  - No changes expected in games.

## 1.1.76 - 2022-03-14
### Added:
- ntc: Implement IEnsureNetworkClockAvailabilityService.
  - Needed by Splatoon 2 with Guest Internet Access enabled. The game is now playable with this setting.

## 1.1.75 - 2022-03-14
### Fixed:
- Caching local network info and using an event handler to invalidate as needed.
  - Improves slowdown in calendar menu in Fire Emblem: Three Houses.

## 1.1.74 - 2022-03-14
### Added:
- Implement S8D24 texture format.
  - Fixes starbits interaction in Super Mario Galaxy, now allowing the game to be progressed through.
  - Fixes fog/depth of field/depth particles in SuperTuxKart.

## 1.1.73 - 2022-03-14
### Fixed:
- Dynamically increase buffer size when resizing.
  - Reduces the boot time (from black screen to Game Freak logo) on Pokémon Brilliant Diamond/Shining Pearl by almost half with PPTC enabled.
  - Greatly improves title screen animation and overall performance on Super Mario Galaxy.

## 1.1.72 - 2022-03-14 
### Added:
- Ui: Add option to show/hide console window (Windows-only).
  - Windows users can now toggle the console under Options > Show Log Console.

## 1.1.71 - 2022-03-14 
### Fixed:
- Initialize indexed inputs used on next shader stage.
  - Fixes another regression introduced in 1.1.61 that would cause shaders to fail to compile on WarioWare: Get It Together! and probably other games using indexed attributes.

## 1.1.70 - 2022-03-14 
### Fixed:
- Do not initialize geometry shader passthrough attributes.
  - Fixes a regression introduced in 1.1.61 that caused solid black/ transparent characters (again) on Game Builder Garage. 

## 1.1.69 - 2022-03-12 
### Added:
- Implement `GetSaveDataBackupSetting` of OLSC service.
  - Allows ACNH from 2.0.5 onwards to boot.

## 1.1.68 - 2022-03-12 
### Added:
- Implement setting to rotate stick axis by 90 degrees.
  - Allows the stick to be rotated in all possible orientations (in conjunction with inversion).
  - Games that use sideways joycons (Super Mario Party etc.) will be able to take advantage of this.

## 1.1.67 - 2022-03-12 
### Fixed:
- Fix GetUserDisableCount NRE.
  - Fixes a "NullReferenceException" that could happen when closing the emulator or stopping emulation, reported on Splatoon 2. 
  - Does NOT fix most instances of emulator crashing when quitting.

## 1.1.66 - 2022-03-12 
### Fixed:
- Limit number of events that can be retrieved from GetDisplayVSyncEvent.
  - Fixes "WaitSynchronization InvalidHandle" error spam on .hack//G.U. Last Recode, making the game playable with logs enabled.

## 1.1.65 - 2022-03-11 
### Fixed:
- KThread: Fix GetPsr mask
  - No changes expected in games.

## 1.1.64 - 2022-03-07 
### Fixed:
- amadeus: Fix wrong Span usage in CopyHistories.
  - Fixes a crash in Mononoke Slashdown, which now goes in-game.
  - Fixes a crash in Paper Mario: The Origami King during a cutscene in Shangri-Spa. 

## 1.1.63 - 2022-03-06 
### Added:
- T32: Implement Data Processing (Modified Immediate) instructions.
  - No changes expected in games.

## 1.1.62 - 2022-03-06 
### Added:
- Mod loading from atmosphere SD directories.
  - Implements addition of SD card paths into the ModLoader so that standard Atmosphere/hardware directory set up mods can be used semi-seamlessly. You can now right click a game and click "Open Atmosphere Mods Directory" to access the folder.

## 1.1.61 - 2022-03-06 
### Fixed:
- Only initialize shader outputs that are actually used on the next stage.
  - Fixes models not rendering in Pokémon Legends: Arceus on AMD OpenGL (Windows).
  - May improve performance in select games e.g. PLA and Link's Awakening for Intel iGPUs (Mesa).

## 1.1.60 - 2022-03-05 
### Fixed:
- A32: Fix ALU immediate instructions.
  - No changes expected in games.

## 1.1.59 - 2022-03-05 
### Fixed:
- Decoders: Fix instruction lengths for 16-bit branch instructions.
  - No changes expected in games.

## 1.1.58 - 2022-03-04 
### Changed:
- Decoder: Exit on trapping instructions, and resume execution at trapping instruction.
  - No changes expected in games.

## 1.1.57 - 2022-03-04 
### Added:
- T32: Implement B, B.cond, BL, BLX.
  - Implements remaining thumb CPU instructions. No changes expected in games.

## 1.1.56 - 2022-03-04 
### Added:
- Preparation for initial Flatpack and FlatHub integration.
  - Initial changes required to publish Ryujinx on FlatHub, a Linux app store which is also used by the Steam Deck.

## 1.1.55 - 2022-03-02 
### Added:
- Implement -p or --profile command line argument.
  - Implements a command line argument for specifying which profile to load, overriding the default behavior of loading the most recently used profile. This is useful for people with shared computers, who can now set up 2 (or more) different Ryujinx desktop shortcuts by adding -p and the profile name in shortcut properties > target.

## 1.1.54 - 2022-02-26 
### Fixed:
- Update LibHac to v0.16.0.
  - Adds support for reading NCAs with compressed sections. Iridium and Gunvolt Chronicles: Luminous Avenger iX 2 can now boot.
  - The emulator will now be able to recover from situations where external things mess with extra data files in the save data file system, instead of just erroring.

## 1.1.53 - 2022-02-22 
### Added:
- T32: Implement ALU (shifted register) instructions.
  - No expected changes to emulator functionality.

## 1.1.52 - 2022-02-22
### Fixed:
- Allow textures to have their data partially mapped.
  - Fixes Miitopia crashing in the underground maze.
  - Fixes crashing in Star Ocean First Departure R.

## 1.1.51 - 2022-02-22 
### Fixed:
- Perform unscaled 2d engine copy on CPU if source texture isn't in cache.
  - Reduces stuttering and fixes texture problems in A Hat in Time.
  - Improves stuttering in UE4 games that use texture streaming, such as Yoshi's Crafted World.
  - Fixes the water in Fatal Frame: Maiden of Black Water.

## 1.1.50 - 2022-02-22 
### Added:
- ARMeilleure: Implement single stepping.
  - No expected changes to emulator functionality.

## 1.1.49 - 2022-02-22 
### Fixed:
- gui: Fixes the games icon when there is an update.
  - Updated games will now also display the updated icon on the games list.

## 1.1.48 - 2022-02-22 
### Fixed:
- ARMeilleure: Fix BLX and BXWritePC.
  - Ensures PC is appropriately masked in BXWritePC and BLX (reg) uses BXWritePC.

## 1.1.47 - 2022-02-22 
### Changed:
- Collapse AsSpan().Slice(..) calls into AsSpan(..).
  - No changes to emulator functionality.

## 1.1.46 - 2022-02-19 
### Fixed:
- Add dedicated ServerBase for FileSystem services.
  - Improves menu performance in Super Smash Bros Ultimate.
  - Reduces stuttering on some button advanced cutscenes in Xenoblade Chronicles: Definitive Edition.
  - May improve other instances of stuttering while streaming assets or loading anything.

## 1.1.45 - 2022-02-17 
### Fixed:
- PPTC version increment.
  - Fixes games getting stuck during boot, right after loading shaders.

## 1.1.44 - 2022-02-17 
### Added:
- Enable CPU JIT cache invalidation.
  - This change will be required in the future to make applications that load code dynamically (NROs, mainly Super Smash Bros Ultimate) or that have self-modifying code (certain Skyline/ARCropolis mods) function properly.

## 1.1.43 - 2022-02-17 
### Fixed:
- Prefer texture over textureSize for sampler type.
  - Fixes shaders failing to compile on some games, however, there seems to be no visible differences.

## 1.1.42 - 2022-02-17 
### Changed:
- Use BitOperations methods and delete now unused BitUtils methods.
  - Replaces BitUtils.CountTrailingZeros/CountLeadingZeros/IsPowerOfTwo with BitOperations methods.
  - No changes expected in games.

## 1.1.41 - 2022-02-17 
### Changed:
- Move kernel syscall logs to new trace log level.

## 1.1.40 - 2022-02-17 
### Added:
- Implement/Stub mnpp:app service and some hid calls.
  - Required by SNES v3.0.0 games (NSO collection), however, these are not playable yet.
  - Allows Nintendo Switch Sports Online Play Test to boot.

## 1.1.39 - 2022-02-17 
### Fixed:
- Decoders: Add IOpCode32HasSetFlags.
  - Fixes "Unhandled exception caught: System.InvalidCastException: Specified cast is not valid" error on boot introduced in 1.1.36.

## 1.1.38 - 2022-02-17
### Added:
- Added trace log level.
  - Adds a "trace" log level in developer logs.

## 1.1.37 - 2022-02-17
### Changed:
- Change ServiceNv map creation logs to the Debug level.
  - Removes quite a bit of unneeded log spam.

## 1.1.36 - 2022-02-17 
### Added:
- ARMeilleure: Thumb support (all T16 instructions).
  - Implements all 16-bit thumb CPU instructions.
  - No changes expected in games.

## 1.1.35 - 2022-02-17 
### Fixed:
- misc: Update GtkSharp.Dependencies and speed up initial Windows build.
  - Fixes flickering tooltips.
  - Windows versions will now build faster on GitHub.

## 1.1.34 - 2022-02-17 
### Added:
- Use ReadOnlySpan<byte> compiler optimization for static data.
  - No changes expected in games.

## 1.1.33 - 2022-02-17 
### Fixed:
- Use a basic cubic interpolation for the audren upsampler.
  - Improves audio in The Legend of Zelda: Skyward Sword HD.

## 1.1.32 - 2022-02-16 
### Fixed:
- amadeus: Fix PCMFloat datasource command v1.
  - Small code correction. No changes expected in games.

## 1.1.31 - 2022-02-16
### Fixed:
- Do not allow render targets not explicitly written by the fragment shader to be modified.
  - Fixes cave rendering in Pokémon Legends: Arceus.
  - Fixes weird lines in Pokémon Sword/Shield.
  - Fixes black water in Paper Mario: The Origami King.
  - Fixes blue emblems on ships in Monster Hunter Rise.
  - Fixes overbright jellyfish in NEO: The World Ends with You.

## 1.1.30 - 2022-02-16 
### Fixed:
- amadeus: Fix limiter correctness.
  - Fixes missing audio on Nintendo Switch Sports Online Play Test.

## 1.1.29 - 2022-02-16 
### Fixed:
- When copying linear textures, DMA should ignore region X/Y.
  - Allows River City Girls Zero to get ingame.

## 1.1.28 - 2022-02-16 
### Changed:
- Adjustmentments to controller deadzone calculation.
  - Improves small movements at cardinal directions.
  - Removes "8-axis" effect at high deadzone values.

## 1.1.27 - 2022-02-13 
### Changed:
- Use Enum and Delegate.CreateDelegate generic overloads.
  - Remove unused EnumExtensions.cs.
  - No changes to emulator functionality.

## 1.1.26 - 2022-02-11 
### Fixed:
- InstEmitMemory32: Literal loads always have word-aligned PC.
  - No changes to emulator functionality.

## 1.1.25 - 2022-02-11 
### Fixed:
- Fix missing geometry shader passthrough inputs.
  - Fixes a regression introduced in 1.0.6988 that caused solid black/ transparent characters on Game Builder Garage.

## 1.1.24 - 2022-02-10 
### Fixed:
- Ship SoundIO library only for the specified runtime.
  - Ensures that the SoundIO project gets "RuntimeIdentifiers" property when built as a subproject, so that the correct platform-specific files are provided.

## 1.1.23 - 2022-02-09 
### Fixed:
- Add a limit on the number of uses a constant may have.
  - Deathsmiles II now works.

## 1.1.22 - 2022-02-09 
### Changed:
- misc: Make PID unsigned long instead of long.
  - Code cleanup. No changes in games.

## 1.1.21 - 2022-02-08
### Added:
- ARMeilleure: A32: Implement SHSUB8 and UHSUB8.
  - Implements missing CPU instructions. No known changes in games.

## 1.1.20 - 2022-02-07
### Fixed:
- Fix headless sdl2 option string.

## 1.1.19 - 2022-02-06
### Fixed:
- Make sure mesa_glthread gets a lowercase string on Linux.
  - Fixes a bug in which MESA was provided with an incorrect environment variable controlling backend threading.

## 1.1.18 - 2022-02-06
### Added:
- ARMeilleure: A32: Implement SHADD8.
  - Implements SHADD8 CPU instruction. No expected changes in games.

## 1.1.17 - 2022-02-06
### Added:
- ARMeilleure: OpCodeTable: Add CMN (RsReg).
  - Implements missing variant of CMN instruction.

## 1.1.16 - 2022-02-02
### Fixed:
- Try to ensure save data always has a valid owner ID.
  - Fixes "ResultFsPermissionDenied (2002-6400)" error that would cause games to close during boot with lots of "ThreadTerminating" errors.

## 1.1.15 - 2022-01-31
### Fixed:
- Fix bug that could cause depth buffer to be missing after clear.
  - Fixes a regression introduced in 1.0.7168 that caused models not to render in Sonic Colors: Ultimate.

## 1.1.14 - 2022-01-30
### Changed:
- Remove Appveyor from Readme and SLN.

## 1.1.13 - 2022-01-29
### Fixed:
- Fix small precision error on CPU reciprocal estimate instructions.
  - Fixes some twitching animations in Pokémon Legends: Arceus, for example on the main character's left arm.

## 1.1.12 - 2022-01-29
### Fixed:
- kernel: A bit of refactoring and fix GetThreadContext3 correctness.
  - Code cleanup for the kernel. No changes expected in games.

## 1.1.11 - 2022-01-27
### Fixed:
- Add timestamp to 16-byte/4-word semaphore releases.
  - Fixes 20fps cap in The Legend of Zelda: Breath of the Wild.
  - Fixes Pokémon Legends: Arceus being more pixelated than normal.

## 1.1.10 - 2022-01-27
### Fixed:
- Fix res scale parameters not being updated in vertex shader.
  - Pokémon Legends: Arceus no longer breaks graphics at higher resolutions when Gastly is spawned. 

## 1.1.9 - 2022-01-25
### Fixed:
- Convert Octal-Mode to Decimal.
  - Fixes autoupdater not setting the correct permissions on Linux/Unix. This would sometimes cause the Ryujinx file to lose its "executable" attribute after an autoupdate. 

## 1.1.8 - 2022-01-24
### Fixed:
- Fix regression on PR builds version number since new release system.
  - Fixes PR builds showing as "dirty" builds.

## 1.1.7 - 2022-01-24
### Fixed:
- Fix calls passing V128 values on Linux.
  - Fixes a regression introduced in 1.0.7000 where in some cases, during the pre-allocation stage, the new register operations would not be added to the call operation node. This would cause the register allocator to not keep track of the fixed registers (possibly overwriting the register values), and also to do register allocation for the operands passed on the call (which it should not do).
  - Fixes Pokémon Sword/Shield saves becoming corrupted on Linux.
  - Fixes a crash when booting Splatoon 2 v5.5.0 on Linux.
  - Fixes software memory manager mode not working on Linux.

NOTE: existing saves created on any version after 1.0.7000 are most likely actually corrupted, so you'll need to delete them. Saves created before the bug was introduced (or created on Windows) should be fine.

## 1.1.6 - 2022-01-23
### Fixed:
- amadeus: Fix possible device sink input out of bound.
  - Fixes a crash in Death Coming. Game now boots to menus, however, it will still crash when attempting to contact online servers.

## 1.1.5 - 2022-01-23
### Fixed:
- Set _vibrationPermitted to return True.
  - Games which respect IHidServer::IsVibrationPermitted should now allow vibration to function, for example, Catherine: Full Body.

## 1.1.4 - 2022-01-22
### Fixed:
- Add support for BC1/2/3 decompression (for 3D textures).
  - Fixes garbled text in Tales of Vesperia.
  - Fixes blocky explosions in Xenoblade Chronicles 2.
  - Fixes rain in Ghosts 'n Goblins Resurrection.

## 1.1.3 - 2022-01-22
**WARNING**: This version requires a manual update by redownloading it from <https://ryujinx.org/download> as a result of AppVeyor takedown of the project.
### Changed:
- Release system was switched to GitHub Release as a result of AppVeyor takedown of the project.

</details>