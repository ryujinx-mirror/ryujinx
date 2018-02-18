## Config File

`Ryujinx.conf` should be present in executable folder (It's an *.ini file) following this format:

- `Logging_Enable_Info` *(bool)*

  Enable the Informations Logging.

- `Logging_Enable_Trace` *(bool)*

  Enable the Trace Logging (Enabled in Debug recommanded).
  
- `Logging_Enable_Debug` *(bool)*

   Enable the Debug Logging (Enabled in Debug recommanded).

- `Logging_Enable_Warn` *(bool)*

  Enable the Warning Logging (Enabled in Debug recommanded).

- `Logging_Enable_Error` *(bool)*

  Enable the Error Logging (Enabled in Debug recommanded).

- `Logging_Enable_Fatal` *(bool)*

  Enable the Fatal Logging (Enabled in Debug recommanded).

- `Logging_Enable_LogFile` *(bool)*

  Enable writing the logging inside a Ryujinx.log file.
  
- `Controls_Left_FakeJoycon_XX` *(int)*
  ```
  Controls_Left_FakeJoycon_Stick_Up (int)
  Controls_Left_FakeJoycon_Stick_Down (int)
  Controls_Left_FakeJoycon_Stick_Left (int)
  Controls_Left_FakeJoycon_Stick_Right (int)
  Controls_Left_FakeJoycon_Stick_Button (int)
  Controls_Left_FakeJoycon_DPad_Up (int)
  Controls_Left_FakeJoycon_DPad_Down (int)
  Controls_Left_FakeJoycon_DPad_Left (int)
  Controls_Left_FakeJoycon_DPad_Right (int)
  Controls_Left_FakeJoycon_Button_Minus (int)
  Controls_Left_FakeJoycon_Button_L (int)
  Controls_Left_FakeJoycon_Button_ZL (int)
  ```
  
  Keys of the Left Emulated Joycon, the values depend of the [OpenTK Enum Keys](https://github.com/opentk/opentk/blob/develop/src/OpenTK/Input/Key.cs).
  
  OpenTK use a QWERTY layout, so pay attention if you use another Keyboard Layout.
  
  Ex: `Controls_Left_FakeJoycon_Button_Minus = 52` > Tab key (All Layout).

- `Controls_Right_FakeJoycon_XX` *(int)*
  ```
  Controls_Right_FakeJoycon_Stick_Up (int)
  Controls_Right_FakeJoycon_Stick_Down (int)
  Controls_Right_FakeJoycon_Stick_Left (int)
  Controls_Right_FakeJoycon_Stick_Right (int)
  Controls_Right_FakeJoycon_Stick_Button (int)
  Controls_Right_FakeJoycon_Button_A (int)
  Controls_Right_FakeJoycon_Button_B (int)
  Controls_Right_FakeJoycon_Button_X (int)
  Controls_Right_FakeJoycon_Button_Y (int)
  Controls_Right_FakeJoycon_Button_Plus (int)
  Controls_Right_FakeJoycon_Button_R (int)
  Controls_Right_FakeJoycon_Button_ZR (int)
  ```

  Keys of the right Emulated Joycon, the values depend of the [OpenTK Enum Keys](https://github.com/opentk/opentk/blob/develop/src/OpenTK/Input/Key.cs).
  
  OpenTK use a QWERTY layout, so pay attention if you use another Keyboard Layout.
  
  Ex: `Controls_Right_FakeJoycon_Button_A = 83` > A key (QWERTY Layout) / Q key (AZERTY Layout).
