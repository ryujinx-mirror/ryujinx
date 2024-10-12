## Ryujinx offers multiplayer support over the internet via LDN/Local Wireless emulation in a special preview build. Furthermore, all versions of Ryujinx now also support LDN connectivity with CFW Nintendo Switch consoles via ldn_mitm, and LAN Mode connectivity with Switch consoles on games with LAN functionality!
## Download LDN 3.1.3 here!
### Windows: https://www.patreon.com/file?h=74910544&i=13368552
### Windows (Avalonia UI): https://www.patreon.com/file?h=74910544&i=13368547
### Linux: https://www.patreon.com/file?h=74910544&i=13368529
### Linux (Avalonia UI): https://www.patreon.com/file?h=74910544&i=13368553
### macOS (3.0.1, Avalonia UI): https://github.com/Ryujinx/release-channel-macos/releases/download/1.1.0-macos1/Ryujinx-1.1.0-macos1-macos_universal.app.tar.gz

## Table of Contents

[LDN/Local Wireless](#ldn)

[LDN Games & How to Play](#how-to-play-ldn)

[LAN Mode](#lan-mode) 
 
[LAN Mode Game List & Cheat Sheet](#lan-mode-games)  

LDN
===

This feature emulates the "Local Play" or "Local Wireless" multiplayer mode in a particular game, but connects you with other Ryujinx players anywhere in the world with internet access. 

Note: this feature does _**not**_ work with "Online Mode", which uses the Nintendo Switch Online service. 

### Requirements:
- An active high-speed internet connection.
- The Ryujinx application must be enabled for inbound traffic in your firewall if you are the game host. Windows Firewall, if enabled, will pop-up prompting you to allow access. Make sure to allow it!
- UPNP enabled in your router/firewall for P2P mode (most routers already have this enabled). Otherwise the game can still work (albeit with higher latency) using the proxy host.
- The same game version as those you are playing with e.g. Animal Crossing: New Horizons 2.0.5; mixing game versions will _**not**_ work!

### Configuration Options:

Multiplayer support via P2P network hosting is already enabled by default in this preview build. In most setups, it will simply work without any configuration.

**Specific Options:**

We recommend creating a user profile and choosing your username in **Options > Manage User Profiles** before going online, as the default profile is known to cause connection issues in some games.

![image](assets/185624289-ab7c877a-f1ef-4c16-84c9-14b847e02b7a.png)

You will have to transfer your saves if you had any on the default profile. To access them, switch to the old profile then right click your game and click on `Open User Save Directory`.

_**Mode:**_  
Set to _Ryujinx Ldn_ by default. There are 3 modes:
- Disabled - Multiplayer functionality is disabled. If you're planning on using this, that's a sign you should just use our [latest regular build](https://ryujinx.org/download) instead.
- Ryujinx LDN - This enables standard connectivity with Ryujinx users over the internet ONLY.
- ldn_mitm - Enables connectivity with a CFW (hacked) Nintendo Switch AND other Ryujinx users on the same network.

_**Disable P2P Network Hosting:**_  
Unchecked by default. If you are experiencing connectivity issues and cannot get P2P to work, you may check the box labeled "Disable P2P Network Hosting (may increase latency)" to instead use the Ryujinx proxy host server.

_**Network Passphrase:**_  
Empty by default. When empty, you will be able to connect to anyone else playing the same game version as yourself. To play with friends, either create a passphrase yourself or click the "Generate Random" button and share it with them, then have them enter the same passphrase. This will prevent unwanted parties from joining your game, as well as limit the number of potential rooms to join for popular games like Mario Kart 8. We want to make sure you get to the right Grand Prix!

![multiplayer settings](assets/185694966-ae17eabc-6f5e-4731-804e-d9433d97cfb9.png)

How to play LDN
======
While LDN is available on a vast amount of games, here are some of the most popular ones and how to access Ryujinx LDN on them.
 
Before playing LDN multiplayer, go to Options > Settings > System and ensure "Enable Guest Internet Access" is OFF and "Enable Vsync" is ON, then head to Multiplayer settings and ensure the mode is set to "Ryujinx Ldn" if you want to play online, or "ldn_mitm" if you want to play with others in the same local network. Once the emulator enters local wireless, a Windows firewall warning will appear. Make sure to allow it to connect.

If you're looking for people to play with, we have dedicated LDN channels in our [Discord server](https://discord.gg/ryujinx).

Animal Crossing: New Horizons
==========  
Head to Dodo Airlines, talk to Orville, choose either "I wanna fly!" or "I want visitors.", and then select "Via local play."

![image](assets/185639984-f29ab392-839a-4ed1-ad23-144b8304d9bb.png)

Fast RMX
==========  
On the main menu, select "Multiplayer" and then "Local communication".

![image](assets/185659376-dd965ffe-38a9-49f4-a68f-7daed0e40ce3.png)

Mario Kart 8 Deluxe
==========  
**This game requires a built shader cache to play LDN.** The game will disconnect if any given player cannot maintain 60fps, and shader compilation stuttering will cause connection errors. If you are experiencing issues, run through every course in single-player first. On fast CPUs, shader compilation may not be a problem when running Vulkan.

On the main menu, select "Wireless Play" and then "1p". "2p" is only for playing LDN with splitscreen, for when you have someone else with you, either in person or on Parsec.

![image](assets/185641338-225c109d-96a1-42fd-a3a3-4e678bb04525.png)

Mario Party Superstars
==========  
**This game requires version 1.0.0 of the game to work on LDN.** It's also prone to disconnecting if no shader cache has been built, though less so than Mario Kart 8.

On the main menu, select "Local Play". 

![image](assets/185641629-db09227e-eb93-4a42-9f42-88e9d2a084fc.png)

Mario Strikers: Battle League
==========  
**This game requires [a mod to bypass the intro crash](https://github.com/ryujinx-mirror/ryujinx/files/12786472/cutscene.skip.-.piplup.zip).**
Right click the game > Open mods directory, and extract on the folder.

On the main menu, select "Quick battle", then select "Local wireless".

![image](assets/185654446-373d3035-a454-49e2-813e-55bb859ac5a9.png)

Monster Hunter Generations Ultimate
==========  
Press `Y`, make sure "Hunter Search" is on. 

![image](assets/185648696-dce23485-2168-4e61-8ad0-ed32e0933ff9.png)

Monster Hunter Rise
==========  
Speak to Senri the Mailman and select "Play Locally". 

![image](assets/185634138-0e83b1db-0210-4f5a-a4e6-12fd75bdbcd3.png)
![image](assets/185634201-0ba2be55-3c67-4f94-b57a-b021ce873e63.png)

Pokkén Tournament DX
==========  
**Use Vulkan for this game.**

On the main menu, select "Wireless Battle".

![image](assets/185653219-32739384-e82f-4e0d-8f1e-981fa2b9b8f7.png)

Pokémon Brilliant Diamond and Shining Pearl
==========  
In a Pokémon Center, head upstairs and talk to the lady in the middle if you want to trade, or talk to the lady in the right if you want to battle. For the latter, select "Local communication".

![image](assets/185644561-76326b69-af4a-4b1b-a65d-bb6c94dab504.png)
![image](assets/185644623-7ebbc375-333b-4ea8-8198-5ee6416f9ec7.png)

Pokémon Scarlet and Violet
==========  
Press `X` to bring up the menu, select "Poké Portal" (available after the first Pokémon Center) and choose the preferred option. 

![image](assets/203631868-3a7edde9-c089-4118-846a-7d2da179fde7.png)
![image](assets/203632109-0dc6544e-dadb-46ce-8a54-36c8f10f674a.png)
"Surprise Trade", "Battle Stadium" and "Mystery Gift" **will not work** due to requiring Nintendo Switch Online connections. This also applies to Poké Portal News.

Pokémon Sword and Shield
==========  
Press `Y` to bring up the comms menu and pick an option.

![image](assets/185661668-36aa1bd7-1304-436f-9ebd-bf05ae05eaf1.png)

Pokémon Legends: Arceus
==========  
In the village, next to the Galaxy building, talk to Simona, select "I want to trade Pokémon", and then "Someone nearby".

![image](assets/185646569-3af4e17c-309e-4ba2-aa14-23367712307b.png)

Splatoon 2
==========  
On the main plaza, go into the building on the right.

![image](assets/185651879-55f45130-040c-4c37-8630-a549ae93812f.png)
![image](assets/185651986-d9776dcd-ca51-4b67-b9de-c57bb91b717f.png)

Splatoon 3
==========  
Press `X` to bring up the menu, go to The Shoal and talk to the receptionist.

![image](assets/189258596-e3968ff9-78d4-4753-857f-5207a60bb049.png)
![image](assets/189258828-d30e4a5d-d9dc-4d88-83ec-febcd83c0c2e.png)

Super Mario 3D World
==========  
Press `R` on the overworld map and select "Local Wireless Play".

![image](assets/206545452-920d90ac-fa52-440f-838d-dc8846becd5b.png)
![image](assets/206545125-0e6d3264-77f8-4b00-84ae-9def333519c8.png)


Super Smash Bros. Ultimate
==========  
On the main menu, press `ZR` (or press `right` twice) and select "Local Wireless".

![image](assets/185651167-62bbcd51-d34b-4a31-b391-4a531a790e8b.png)


LAN Mode
========

`Enable Guest Internet Access` allows you to connect with a Switch console on the same network as your PC running Ryujinx, as long as the game supports LAN as well. This feature is available on mainline Ryujinx too. See below for a list of games with a LAN mode.

![LAN mode](assets/185664722-02acf437-bdac-4f4e-ac14-38bdf30902eb.png)


LAN Mode Games
=======================================
[ARMS](#arms-lan-mode)  
[Bayonetta 2](#bayonetta-2-lan-mode)  
[Duke Nukem 3D: 20th Anniversary World Tour](duke-nukem-3d-20th-anniversary-world-tour-lan-mode)                                        
[Mario & Sonic at the Olympic Games Tokyo 2020](#mario-and-sonic-at-the-olympic-games-tokyo-2020-lan-mode)    
[Mario Kart 8 Deluxe](#mario-kart-8-deluxe-lan-mode)  
[Mario Tennis Aces](#mario-tennis-aces-lan-mode)  
[Pokkén Tournament DX](#pokkén-tournament-dx-lan-mode)  
[Pokémon Sword and Pokémon Shield](#pokémon-sword-and-pokémon-shield-lan-mode)  
[Saints Row: the Third - The Full Package](#saints-row-the-third-the-full-package-lan-mode)  
[Saints Row IV](#saints-row-iv-lan-mode)  
[Splatoon 2](#splatoon-2-lan-mode)  
[Splatoon 3](#splatoon-3-lan-mode)    
[Titan Quest](#titan-quest-lan-mode)

ARMS LAN Mode
====
Hold `Left Analog stick button` and press `L` + `R` on the main menu screen. The "Local" option will change to "LAN Play".  

![2021-02-01 14_14_47-Ryujinx 1 0 0-ldn2 1 - ARMS v5 4 0 (01009B500007C000) (64-bit)](assets/106519052-f3ea7400-6497-11eb-84e1-fd2f086c8e33.png)

Bayonetta 2 LAN Mode
===========  
Enter tag climax and highlight local play. Hold `Left Analog stick button` and press `L` + `R`.  

![image](assets/106519536-ac181c80-6498-11eb-87bc-260d9da6be75.png)

Duke Nukem 3D: 20th Anniversary World Tour LAN Mode
===========  
On the main menu, choose "Multiplayer" then select "LAN Play".

![image](assets/185713561-da196c78-de81-41ff-842a-a6f3de053d10.png)

Mario and Sonic at the Olympic Games Tokyo 2020 LAN Mode
===========================================  
Press `L` + `R` + `Left Analog Stick Button` on the main menu screen. The "Local Play" option will change to "LAN Play".  

![image](assets/106519747-01ecc480-6499-11eb-83b4-5b7487e00baf.png)

Mario Kart 8 Deluxe LAN Mode
===================
Press `L` + `R` + `Left Analog Stick Button` on the main menu screen. The "Wireless Play" option will change to "LAN Play".  

![image](assets/106518403-1203a480-6497-11eb-8f9d-99063198d6ed.png)

Mario Tennis Aces LAN Mode
=================
Select Free Play from the Main Menu. Hold `Left Analog stick button` and press `L` + `R`. The "Local Play" option will change to "LAN Play".  

![image](assets/106520124-84758400-6499-11eb-8790-7001fc63eee5.png)

Pokkén Tournament DX LAN Mode 
====================  
You must use game version **1.3.3**. From the main screen select a game. Press `B` + `X` + `Dpad-Down` and press `L` + `R`. A new screen asking if you want to enter Event Mode will appear.  

![image](assets/106525027-9870b400-64a0-11eb-9625-d65a5383b3ae.png)

Pokémon Sword and Pokémon Shield LAN Mode
==================== 
Note: **LAN Mode in these games became inconsistent** due to a game update. If you're unable to connect, there is probably no solution other than resorting to ldn_mitm.

Press `L` + `R` + `Left Analog stick button` in the options menu. The following screen/prompt will pop up. You must have gotten far enough in the game to unlock multiplayer in order to use LAN Mode!  

![image](assets/106521445-7c1e4880-649b-11eb-91b5-4cb25b343fa6.png)

SAINTS ROW THE THIRD THE FULL PACKAGE LAN Mode
=====================================  
At the main menu, select "CO-OP CAMPAIGN" and then Select "LAN GAME".  

![image](assets/106521866-1e3e3080-649c-11eb-9f79-6cbb3e9b4245.png)

Saints Row IV LAN Mode
==========  
On the main menu, select "Co-op Campaign" then "LAN Play".

![image](assets/185718836-15a7f140-10a9-42ad-8c5f-01af9a0168f8.png)

Splatoon 2 LAN Mode
==========  
Hold `L` + `R` + `Left Analog Stick Button` at the local play option for 4-5 seconds until LAN mode is activated. You will see the following prompt:  

![image](assets/106522652-44180500-649d-11eb-994c-081e851b08ff.png)

Splatoon 3 LAN Mode
==========  
Press `X` to bring up the menu, go to The Shoal, enable Guest Internet Access (enabling it before booting the game will cause it to get stuck trying to connect to online). Hold `ZL` + `ZR` + `Left Stick Button` for 5 seconds at The Shoal and you'll see the prompt in the image below. LAN mode will be enabled afterwards.

![image](assets/189263164-951f31d9-5416-4fd1-9037-6cac9dc10e7b.png)

Titan Quest LAN Mode
===========  
Select "Multiplayer - Local" in the main menu.

![image](assets/185712303-0ec50a9d-7673-453a-870c-a92c94838db2.png)

