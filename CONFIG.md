## Config File

`Ryujinx.conf` should be present in executable folder (It's an *.ini file) following this format:

- `Logging_Enable_Info` *(bool)*

  Enable the Informations Logging.

- `Logging_Enable_Trace` *(bool)*

  Enable the Trace Logging (Enabled in Debug recommended).
  
- `Logging_Enable_Debug` *(bool)*

   Enable the Debug Logging (Enabled in Debug recommended).

- `Logging_Enable_Warn` *(bool)*

  Enable the Warning Logging (Enabled in Debug recommended).

- `Logging_Enable_Error` *(bool)*

  Enable the Error Logging (Enabled in Debug recommended).

- `Logging_Enable_Fatal` *(bool)*

  Enable the Fatal Logging (Enabled in Debug recommended).

- `Logging_Enable_Ipc` *(bool)*

  Enable the Ipc Message Logging.

- `Logging_Enable_LogFile` *(bool)*

  Enable writing the logging inside a Ryujinx.log file.
  
- `GamePad_Index` *(int)*

  The index of the Controller Device.
  
- `GamePad_Deadzone` *(float)*

  The deadzone of both analog sticks on the Controller.

- `GamePad_Enable` *(bool)*
  
  Whether or not to enable Controller Support.
  
- `Controls_Left_JoyConKeyboard_XX` *(int)*
  ```
  Controls_Left_JoyConKeyboard_Stick_Up (int)
  Controls_Left_JoyConKeyboard_Stick_Down (int)
  Controls_Left_JoyConKeyboard_Stick_Left (int)
  Controls_Left_JoyConKeyboard_Stick_Right (int)
  Controls_Left_JoyConKeyboard_Stick_Button (int)
  Controls_Left_JoyConKeyboard_DPad_Up (int)
  Controls_Left_JoyConKeyboard_DPad_Down (int)
  Controls_Left_JoyConKeyboard_DPad_Left (int)
  Controls_Left_JoyConKeyboard_DPad_Right (int)
  Controls_Left_JoyConKeyboard_Button_Minus (int)
  Controls_Left_JoyConKeyboard_Button_L (int)
  Controls_Left_JoyConKeyboard_Button_ZL (int)
  ```
  
  Keys of the Left Emulated Joycon, the values depend of the [OpenTK Enum Keys](https://github.com/opentk/opentk/blob/develop/src/OpenTK/Input/Key.cs).
  
  OpenTK use a QWERTY layout, so pay attention if you use another Keyboard Layout.
  
  Ex: `Controls_Left_JoyConKeyboard_Button_Minus = 52` > Tab key (All Layout).

- `Controls_Right_JoyConKeyboard_XX` *(int)*
  ```
  Controls_Right_JoyConKeyboard_Stick_Up (int)
  Controls_Right_JoyConKeyboard_Stick_Down (int)
  Controls_Right_JoyConKeyboard_Stick_Left (int)
  Controls_Right_JoyConKeyboard_Stick_Right (int)
  Controls_Right_JoyConKeyboard_Stick_Button (int)
  Controls_Right_JoyConKeyboard_Button_A (int)
  Controls_Right_JoyConKeyboard_Button_B (int)
  Controls_Right_JoyConKeyboard_Button_X (int)
  Controls_Right_JoyConKeyboard_Button_Y (int)
  Controls_Right_JoyConKeyboard_Button_Plus (int)
  Controls_Right_JoyConKeyboard_Button_R (int)
  Controls_Right_JoyConKeyboard_Button_ZR (int)
  ```

  Keys of the right Emulated Joycon, the values depend of the [OpenTK Enum Keys](https://github.com/opentk/opentk/blob/develop/src/OpenTK/Input/Key.cs).
  
  OpenTK use a QWERTY layout, so pay attention if you use another Keyboard Layout.
  
  Ex: `Controls_Right_JoyConKeyboard_Button_A = 83` > A key (QWERTY Layout) / Q key (AZERTY Layout).
  
- `Controls_Left_JoyConController_XX` *(String)*
  ```
  Controls_Left_JoyConController_Stick (String)
  Controls_Left_JoyConController_Stick_Button (String)
  Controls_Left_JoyConController_DPad_Up (String)
  Controls_Left_JoyConController_DPad_Down (String)
  Controls_Left_JoyConController_DPad_Left (String)
  Controls_Left_JoyConController_DPad_Right (String)
  Controls_Left_JoyConController_Button_Minus (String)
  Controls_Left_JoyConController_Button_L (String)
  Controls_Left_JoyConController_Button_ZL (String)
  ```
  
- `Controls_Right_JoyConController_XX` *(String)*
  ```
  Controls_Right_JoyConController_Stick (String)
  Controls_Right_JoyConController_Stick_Button (String)
  Controls_Right_JoyConController_Button_A (String)
  Controls_Right_JoyConController_Button_B (String)
  Controls_Right_JoyConController_Button_X (String)
  Controls_Right_JoyConController_Button_Y (String)
  Controls_Right_JoyConController_Button_Plus (String)
  Controls_Right_JoyConController_Button_R (String)
  Controls_Right_JoyConController_Button_ZR (String)
  ```

- Default Mapping
   - Controller
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
   - Keyboard
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
  
- Valid Button Mappings
  - A = The A / Cross Button
  - B = The B / Circle Button
  - X = The X / Square Button
  - Y = The Y / Triangle Button
  - LStick = The Left Analog Stick when Pressed Down
  - RStick = The Right Analog Stick when Pressed Down
  - Start = The Start / Options Button
  - Back = The Select / Back / Share Button
  - RShoulder = The Right Shoulder Button
  - LShoulder = The Left Shoulder Button
  - RTrigger = The Right Trigger
  - LTrigger = The Left Trigger
  - DPadUp = Up on the DPad
  - DPadDown = Down on the DPad
  - DPadLeft = Left on the DPad
  - DpadRight = Right on the DPad
- Valid Joystick Mappings
  - LJoystick = The Left Analog Stick
  - RJoystick = The Right Analog Stick

  On more obscure / weird controllers this can vary, so if this list doesn't work, trial and error will.