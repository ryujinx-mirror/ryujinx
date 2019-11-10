# Keys

Keys are required for decrypting most of the file formats used by the Nintendo Switch.

 Keysets are stored as text files. These 2 filenames are automatically read:  
* `prod.keys` - Contains common keys used by all Nintendo Switch devices.
* `title.keys` - Contains game-specific keys.

Ryujinx will first look for keys in `RyuFS/system`, and if it doesn't find any there it will look in `$HOME/.switch`.
To dump your `prod.keys` and `title.keys` please follow these following steps.
1.	First off learn how to boot into RCM mode and inject payloads if you haven't already. This can be done [here](https://nh-server.github.io/switch-guide/).
2.	Make sure you have an SD card with the latest release of [Atmosphere](https://github.com/Atmosphere-NX/Atmosphere/releases) inserted into your Nintendo Switch.
3.	Download the latest release of [Lockpick_RCM](https://github.com/shchmue/Lockpick_RCM/releases).
4.	Boot into RCM mode.
5.	Inject the `Lockpick_RCM.bin` that you have downloaded at `Step 3.` using your preferred payload injector. We recommend [TegraRCMGUI](https://github.com/eliboa/TegraRcmGUI/releases) as it is easy to use and has a decent feature set.
6.	Using the `Vol+/-` buttons to navigate and the `Power` button to select, select `Dump from SysNAND | Key generation: X` ("X" depends on your Nintendo Switch's firmware version)
7.	The dumping process may take a while depending on how many titles you have installed.
8.	After its completion press any button to return to the main menu of Lockpick_RCM.
9.	Navigate to and select `Power off` if you have an SD card reader. Or you could Navigate and select `Reboot (RCM)` if you want to mount your SD card using `TegraRCMGUI > Tools > Memloader V3 > MMC - SD Card`.
10.	You can find your keys in `sd:/switch/prod.keys` and `sd:/switch/title.keys` respectively.
11. Copy these files and paste them in `RyuFS/system`.
And you're done!

## Title keys

These are only used for games that are not dumped from cartridges but from games downloaded from the Nintendo eShop, these are also only used if the eShop dump does *not* have a `ticket`. If the game does have a ticket, Ryujinx will read the key directly from that ticket.

Title keys are stored in the format `rights_id = key`.

For example:

```
01000000000100000000000000000003 = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
01000000000108000000000000000003 = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
01000000000108000000000000000004 = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

## Prod keys

These are typically used to decrypt system files and encrypted game files. These keys get changed in about every major system update, so make sure to keep your keys up-to-date if you want to play newer games!