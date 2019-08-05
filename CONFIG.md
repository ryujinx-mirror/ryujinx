## Config File

`Config.jsonc` should be present in executable folder. The available settings follow:

- `graphics_shaders_dump_path` *(string)*

  Dump shaders in local directory (e.g. `C:\ShaderDumps`)

- `logging_enable_debug` *(bool)*

  Enable the Debug Logging.

- `logging_enable_stub` *(bool)*

  Enable the Trace Logging.

- `logging_enable_info` *(bool)*

  Enable the Informations Logging.

- `logging_enable_warn` *(bool)*

  Enable the Warning Logging.

- `logging_enable_error` *(bool)*

  Enable the Error Logging.

- `enable_file_log` *(bool)*

  Enable writing the logging inside a Ryujinx.log file.

- `system_language` *(string)*

  Change System Language, [System Language list](https://gist.github.com/HorrorTroll/b6e4a88d774c3c9b3bdf54d79a7ca43b)

- `docked_mode` *(bool)*

  Enable or Disable Docked Mode

- `enable_vsync` *(bool)*

  Enable or Disable Game Vsync

- `enable_multicore_scheduling` *(bool)*

  Enable or Disable Multi-core scheduling of threads

- `enable_fs_integrity_checks` *(bool)*

  Enable integrity checks on Switch content files

- `controller_type` *(string)*

  The primary controller's type.
  Supported Values: `Handheld`, `ProController`, `NpadPair`, `NpadLeft`, `NpadRight`

- `keyboard_controls` *(object)* :
  - `left_joycon` *(object)* :
    Left JoyCon Keyboard Bindings
    - `stick_up` *(string)*
    - `stick_down` *(string)*
    - `stick_left` *(string)*
    - `stick_right` *(string)*
    - `stick_button` *(string)*
    - `dpad_up` *(string)*
    - `dpad_down` *(string)*
    - `dpad_left` *(string)*
    - `dpad_right` *(string)*
    - `button_minus` *(string)*
    - `button_l` *(string)*
    - `button_zl` *(string)*
  - `right_joycon` *(object)* :
    Right JoyCon Keyboard Bindings
    - `stick_up` *(string)*
    - `stick_down` *(string)*
    - `stick_left` *(string)*
    - `stick_right` *(string)*
    - `stick_button` *(string)*
    - `button_a` *(string)*
    - `button_b` *(string)*
    - `button_x` *(string)*
    - `button_y` *(string)*
    - `button_plus` *(string)*
    - `button_r` *(string)*
    - `button_zr` *(string)*

- `joystick_controls` *(object)* :
  - `enabled` *(bool)*
    Whether or not to enable Controller Support.
  - `index` *(int)*
    The index of the Controller Device.
  - `deadzone` *(number)*
    The deadzone of both analog sticks on the Controller.
  - `trigger_threshold` *(number)*
    The value of how pressed down each trigger has to be in order to register a button press
  - `left_joycon` *(object)* :
    Left JoyCon Controller Bindings
    - `stick` *(string)*
    - `stick_button` *(string)*
    - `dpad_up` *(string)*
    - `dpad_down` *(string)*
    - `dpad_left` *(string)*
    - `dpad_right` *(string)*
    - `button_minus` *(string)*
    - `button_l` *(string)*
    - `button_zl` *(string)*
  - `right_joycon` *(object)* :
  Right JoyCon Controller Bindings
    - `stick` *(string)*
    - `stick_button` *(string)*
    - `button_a` *(string)*
    - `button_b` *(string)*
    - `button_x` *(string)*
    - `button_y` *(string)*
    - `button_plus` *(string)*
    - `button_r` *(string)*
    - `button_zr` *(string)*
  
### Default Mapping.
   #### Controller
     - Left Joycon:
       - Analog Stick = Axis 0
	   - DPad Up = DPad Up #Hat0 Up
	   - DPad Down = DPad Down #Hat0 Down
	   - DPad Left = DPad Left #Hat0 Left
	   - DPad Right = DPad Right #Hat0 Right
	   - Minus = Button 10
	   - L = Button 6
	   - ZL = Button 8
	 
     - Right Joycon:
	   - Analog Stick = Axis 2
	   - A = Button 0
	   - B = Button 1
	   - X = Button 3
	   - Y = Button 4
	   - Plus = Button 11
	   - R = Button 7
	   - ZR = Button 9

   #### Keyboard
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
  
### Valid Button Mappings.
  - Button# = A button on the controller. # should not exceed the max # of buttons detected on your controller.
  - Axis# = An analog axis on the controller. It can be a stick control, or a motion control axis.
  - Hat# = A Point of View (POV), Hat or Directional Pad control on the controller.

  Button configuration and controller capabilities differ from one controller to another. Please use a
  configuration tool to find out the actual button configuration of your controller.