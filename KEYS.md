# Keys

Keys are required for decrypting most of the file formats used by the Nintendo Switch.

Keysets are stored as text files. These 3 filenames are automatically read:  
`prod.keys` - Contains common keys usedy by all Switch devices.  
`console.keys` - Contains console-unique keys.  
`title.keys` - Contains game-specific keys.

Ryujinx will first look for keys in `RyuFS/system`, and if it doesn't find any there it will look in `$HOME/.switch`.

A guide to assist with dumping your own keys can be found [here](https://gist.github.com/roblabla/d8358ab058bbe3b00614740dcba4f208).

## Common keys

Here is a template for a key file containing the main keys Ryujinx uses to read content files.  
Both `prod.keys` and `console.keys` use this format.

```
master_key_00                         = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
master_key_01                         = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
master_key_02                         = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
master_key_03                         = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
master_key_04                         = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
master_key_05                         = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

titlekek_source                       = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
key_area_key_application_source       = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
key_area_key_ocean_source             = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
key_area_key_system_source            = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
aes_kek_generation_source             = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
aes_key_generation_source             = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
header_kek_source                     = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
header_key_source                     = XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

## Title keys

Title keys are stored in the format `rights_id,key`.

For example:

```
01000000000100000000000000000003,XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
01000000000108000000000000000003,XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
01000000000108000000000000000004,XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

## Complete key list
Below is a complete list of keys that are currently recognized.  
\## represents a hexadecimal number between 00 and 1F  
@@ represents a hexadecimal number between 00 and 03

### Common keys

```
master_key_source
keyblob_mac_key_source
package2_key_source
aes_kek_generation_source
aes_key_generation_source
key_area_key_application_source
key_area_key_ocean_source
key_area_key_system_source
titlekek_source
header_kek_source
header_key_source
sd_card_kek_source
sd_card_nca_key_source
sd_card_save_key_source
retail_specific_aes_key_source
per_console_key_source
bis_kek_source
bis_key_source_@@

header_key
xci_header_key
eticket_rsa_kek

master_key_##
package1_key_##
package2_key_##
titlekek_##
key_area_key_application_##
key_area_key_ocean_##
key_area_key_system_##
keyblob_key_source_##
keyblob_##
```

### Console-unique keys

```
secure_boot_key
tsec_key
device_key
bis_key_@@

keyblob_key_##
keyblob_mac_key_##
encrypted_keyblob_##

sd_seed
```
