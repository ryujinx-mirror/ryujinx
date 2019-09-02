using OpenTK.Input;
using Ryujinx.HLE.Input;

namespace Ryujinx.UI.Input
{
    public struct NpadKeyboardLeft
    {
        public Key StickUp;
        public Key StickDown;
        public Key StickLeft;
        public Key StickRight;
        public Key StickButton;
        public Key DPadUp;
        public Key DPadDown;
        public Key DPadLeft;
        public Key DPadRight;
        public Key ButtonMinus;
        public Key ButtonL;
        public Key ButtonZl;
    }

    public struct NpadKeyboardRight
    {
        public Key StickUp;
        public Key StickDown;
        public Key StickLeft;
        public Key StickRight;
        public Key StickButton;
        public Key ButtonA;
        public Key ButtonB;
        public Key ButtonX;
        public Key ButtonY;
        public Key ButtonPlus;
        public Key ButtonR;
        public Key ButtonZr;
    }

    public struct KeyboardHotkeys
    {
        public Key ToggleVsync;
    }

    public class NpadKeyboard
    {
        /// <summary>
        /// Left JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardLeft LeftJoycon { get; set; }

        /// <summary>
        /// Right JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardRight RightJoycon { get; set; }

        /// <summary>
        /// Hotkey Keyboard Bindings
        /// </summary>
        public KeyboardHotkeys Hotkeys { get; private set; }

        public ControllerButtons GetButtons(KeyboardState keyboard)
        {
            ControllerButtons buttons = 0;

            if (keyboard[(Key)LeftJoycon.StickButton]) buttons |= ControllerButtons.StickLeft;
            if (keyboard[(Key)LeftJoycon.DPadUp])      buttons |= ControllerButtons.DpadUp;
            if (keyboard[(Key)LeftJoycon.DPadDown])    buttons |= ControllerButtons.DpadDown;
            if (keyboard[(Key)LeftJoycon.DPadLeft])    buttons |= ControllerButtons.DpadLeft;
            if (keyboard[(Key)LeftJoycon.DPadRight])   buttons |= ControllerButtons.DPadRight;
            if (keyboard[(Key)LeftJoycon.ButtonMinus]) buttons |= ControllerButtons.Minus;
            if (keyboard[(Key)LeftJoycon.ButtonL])     buttons |= ControllerButtons.L;
            if (keyboard[(Key)LeftJoycon.ButtonZl])    buttons |= ControllerButtons.Zl;
            
            if (keyboard[(Key)RightJoycon.StickButton]) buttons |= ControllerButtons.StickRight;
            if (keyboard[(Key)RightJoycon.ButtonA])     buttons |= ControllerButtons.A;
            if (keyboard[(Key)RightJoycon.ButtonB])     buttons |= ControllerButtons.B;
            if (keyboard[(Key)RightJoycon.ButtonX])     buttons |= ControllerButtons.X;
            if (keyboard[(Key)RightJoycon.ButtonY])     buttons |= ControllerButtons.Y;
            if (keyboard[(Key)RightJoycon.ButtonPlus])  buttons |= ControllerButtons.Plus;
            if (keyboard[(Key)RightJoycon.ButtonR])     buttons |= ControllerButtons.R;
            if (keyboard[(Key)RightJoycon.ButtonZr])    buttons |= ControllerButtons.Zr;

            return buttons;
        }

        public (short, short) GetLeftStick(KeyboardState keyboard)
        {
            short dx = 0;
            short dy = 0;
            
            if (keyboard[(Key)LeftJoycon.StickUp])    dy =  short.MaxValue;
            if (keyboard[(Key)LeftJoycon.StickDown])  dy = -short.MaxValue;
            if (keyboard[(Key)LeftJoycon.StickLeft])  dx = -short.MaxValue;
            if (keyboard[(Key)LeftJoycon.StickRight]) dx =  short.MaxValue;

            return (dx, dy);
        }

        public (short, short) GetRightStick(KeyboardState keyboard)
        {
            short dx = 0;
            short dy = 0;

            if (keyboard[(Key)RightJoycon.StickUp])    dy =  short.MaxValue;
            if (keyboard[(Key)RightJoycon.StickDown])  dy = -short.MaxValue;
            if (keyboard[(Key)RightJoycon.StickLeft])  dx = -short.MaxValue;
            if (keyboard[(Key)RightJoycon.StickRight]) dx =  short.MaxValue;

            return (dx, dy);
        }

        public HotkeyButtons GetHotkeyButtons(KeyboardState keyboard)
        {
            HotkeyButtons buttons = 0;

            if (keyboard[(Key)Hotkeys.ToggleVsync]) buttons |= HotkeyButtons.ToggleVSync;

            return buttons;
        }

        class KeyMappingEntry
        {
            public Key TargetKey;
            public byte Target;
        }

        private static readonly KeyMappingEntry[] KeyMapping = new KeyMappingEntry[]
        {
            new KeyMappingEntry { TargetKey = Key.A, Target = 0x4  },
            new KeyMappingEntry { TargetKey = Key.B, Target = 0x5  },
            new KeyMappingEntry { TargetKey = Key.C, Target = 0x6  },
            new KeyMappingEntry { TargetKey = Key.D, Target = 0x7  },
            new KeyMappingEntry { TargetKey = Key.E, Target = 0x8  },
            new KeyMappingEntry { TargetKey = Key.F, Target = 0x9  },
            new KeyMappingEntry { TargetKey = Key.G, Target = 0xA  },
            new KeyMappingEntry { TargetKey = Key.H, Target = 0xB  },
            new KeyMappingEntry { TargetKey = Key.I, Target = 0xC  },
            new KeyMappingEntry { TargetKey = Key.J, Target = 0xD  },
            new KeyMappingEntry { TargetKey = Key.K, Target = 0xE  },
            new KeyMappingEntry { TargetKey = Key.L, Target = 0xF  },
            new KeyMappingEntry { TargetKey = Key.M, Target = 0x10 },
            new KeyMappingEntry { TargetKey = Key.N, Target = 0x11 },
            new KeyMappingEntry { TargetKey = Key.O, Target = 0x12 },
            new KeyMappingEntry { TargetKey = Key.P, Target = 0x13 },
            new KeyMappingEntry { TargetKey = Key.Q, Target = 0x14 },
            new KeyMappingEntry { TargetKey = Key.R, Target = 0x15 },
            new KeyMappingEntry { TargetKey = Key.S, Target = 0x16 },
            new KeyMappingEntry { TargetKey = Key.T, Target = 0x17 },
            new KeyMappingEntry { TargetKey = Key.U, Target = 0x18 },
            new KeyMappingEntry { TargetKey = Key.V, Target = 0x19 },
            new KeyMappingEntry { TargetKey = Key.W, Target = 0x1A },
            new KeyMappingEntry { TargetKey = Key.X, Target = 0x1B },
            new KeyMappingEntry { TargetKey = Key.Y, Target = 0x1C },
            new KeyMappingEntry { TargetKey = Key.Z, Target = 0x1D },

            new KeyMappingEntry { TargetKey = Key.Number1, Target = 0x1E },
            new KeyMappingEntry { TargetKey = Key.Number2, Target = 0x1F },
            new KeyMappingEntry { TargetKey = Key.Number3, Target = 0x20 },
            new KeyMappingEntry { TargetKey = Key.Number4, Target = 0x21 },
            new KeyMappingEntry { TargetKey = Key.Number5, Target = 0x22 },
            new KeyMappingEntry { TargetKey = Key.Number6, Target = 0x23 },
            new KeyMappingEntry { TargetKey = Key.Number7, Target = 0x24 },
            new KeyMappingEntry { TargetKey = Key.Number8, Target = 0x25 },
            new KeyMappingEntry { TargetKey = Key.Number9, Target = 0x26 },
            new KeyMappingEntry { TargetKey = Key.Number0, Target = 0x27 },

            new KeyMappingEntry { TargetKey = Key.Enter,        Target = 0x28 },
            new KeyMappingEntry { TargetKey = Key.Escape,       Target = 0x29 },
            new KeyMappingEntry { TargetKey = Key.BackSpace,    Target = 0x2A },
            new KeyMappingEntry { TargetKey = Key.Tab,          Target = 0x2B },
            new KeyMappingEntry { TargetKey = Key.Space,        Target = 0x2C },
            new KeyMappingEntry { TargetKey = Key.Minus,        Target = 0x2D },
            new KeyMappingEntry { TargetKey = Key.Plus,         Target = 0x2E },
            new KeyMappingEntry { TargetKey = Key.BracketLeft,  Target = 0x2F },
            new KeyMappingEntry { TargetKey = Key.BracketRight, Target = 0x30 },
            new KeyMappingEntry { TargetKey = Key.BackSlash,    Target = 0x31 },
            new KeyMappingEntry { TargetKey = Key.Tilde,        Target = 0x32 },
            new KeyMappingEntry { TargetKey = Key.Semicolon,    Target = 0x33 },
            new KeyMappingEntry { TargetKey = Key.Quote,        Target = 0x34 },
            new KeyMappingEntry { TargetKey = Key.Grave,        Target = 0x35 },
            new KeyMappingEntry { TargetKey = Key.Comma,        Target = 0x36 },
            new KeyMappingEntry { TargetKey = Key.Period,       Target = 0x37 },
            new KeyMappingEntry { TargetKey = Key.Slash,        Target = 0x38 },
            new KeyMappingEntry { TargetKey = Key.CapsLock,     Target = 0x39 },

            new KeyMappingEntry { TargetKey = Key.F1,  Target = 0x3a },
            new KeyMappingEntry { TargetKey = Key.F2,  Target = 0x3b },
            new KeyMappingEntry { TargetKey = Key.F3,  Target = 0x3c },
            new KeyMappingEntry { TargetKey = Key.F4,  Target = 0x3d },
            new KeyMappingEntry { TargetKey = Key.F5,  Target = 0x3e },
            new KeyMappingEntry { TargetKey = Key.F6,  Target = 0x3f },
            new KeyMappingEntry { TargetKey = Key.F7,  Target = 0x40 },
            new KeyMappingEntry { TargetKey = Key.F8,  Target = 0x41 },
            new KeyMappingEntry { TargetKey = Key.F9,  Target = 0x42 },
            new KeyMappingEntry { TargetKey = Key.F10, Target = 0x43 },
            new KeyMappingEntry { TargetKey = Key.F11, Target = 0x44 },
            new KeyMappingEntry { TargetKey = Key.F12, Target = 0x45 },

            new KeyMappingEntry { TargetKey = Key.PrintScreen, Target = 0x46 },
            new KeyMappingEntry { TargetKey = Key.ScrollLock,  Target = 0x47 },
            new KeyMappingEntry { TargetKey = Key.Pause,       Target = 0x48 },
            new KeyMappingEntry { TargetKey = Key.Insert,      Target = 0x49 },
            new KeyMappingEntry { TargetKey = Key.Home,        Target = 0x4A },
            new KeyMappingEntry { TargetKey = Key.PageUp,      Target = 0x4B },
            new KeyMappingEntry { TargetKey = Key.Delete,      Target = 0x4C },
            new KeyMappingEntry { TargetKey = Key.End,         Target = 0x4D },
            new KeyMappingEntry { TargetKey = Key.PageDown,    Target = 0x4E },
            new KeyMappingEntry { TargetKey = Key.Right,       Target = 0x4F },
            new KeyMappingEntry { TargetKey = Key.Left,        Target = 0x50 },
            new KeyMappingEntry { TargetKey = Key.Down,        Target = 0x51 },
            new KeyMappingEntry { TargetKey = Key.Up,          Target = 0x52 },

            new KeyMappingEntry { TargetKey = Key.NumLock,        Target = 0x53 },
            new KeyMappingEntry { TargetKey = Key.KeypadDivide,   Target = 0x54 },
            new KeyMappingEntry { TargetKey = Key.KeypadMultiply, Target = 0x55 },
            new KeyMappingEntry { TargetKey = Key.KeypadMinus,    Target = 0x56 },
            new KeyMappingEntry { TargetKey = Key.KeypadPlus,     Target = 0x57 },
            new KeyMappingEntry { TargetKey = Key.KeypadEnter,    Target = 0x58 },
            new KeyMappingEntry { TargetKey = Key.Keypad1,        Target = 0x59 },
            new KeyMappingEntry { TargetKey = Key.Keypad2,        Target = 0x5A },
            new KeyMappingEntry { TargetKey = Key.Keypad3,        Target = 0x5B },
            new KeyMappingEntry { TargetKey = Key.Keypad4,        Target = 0x5C },
            new KeyMappingEntry { TargetKey = Key.Keypad5,        Target = 0x5D },
            new KeyMappingEntry { TargetKey = Key.Keypad6,        Target = 0x5E },
            new KeyMappingEntry { TargetKey = Key.Keypad7,        Target = 0x5F },
            new KeyMappingEntry { TargetKey = Key.Keypad8,        Target = 0x60 },
            new KeyMappingEntry { TargetKey = Key.Keypad9,        Target = 0x61 },
            new KeyMappingEntry { TargetKey = Key.Keypad0,        Target = 0x62 },
            new KeyMappingEntry { TargetKey = Key.KeypadPeriod,   Target = 0x63 },

            new KeyMappingEntry { TargetKey = Key.NonUSBackSlash, Target = 0x64 },

            new KeyMappingEntry { TargetKey = Key.F13, Target = 0x68 },
            new KeyMappingEntry { TargetKey = Key.F14, Target = 0x69 },
            new KeyMappingEntry { TargetKey = Key.F15, Target = 0x6A },
            new KeyMappingEntry { TargetKey = Key.F16, Target = 0x6B },
            new KeyMappingEntry { TargetKey = Key.F17, Target = 0x6C },
            new KeyMappingEntry { TargetKey = Key.F18, Target = 0x6D },
            new KeyMappingEntry { TargetKey = Key.F19, Target = 0x6E },
            new KeyMappingEntry { TargetKey = Key.F20, Target = 0x6F },
            new KeyMappingEntry { TargetKey = Key.F21, Target = 0x70 },
            new KeyMappingEntry { TargetKey = Key.F22, Target = 0x71 },
            new KeyMappingEntry { TargetKey = Key.F23, Target = 0x72 },
            new KeyMappingEntry { TargetKey = Key.F24, Target = 0x73 },

            new KeyMappingEntry { TargetKey = Key.ControlLeft,  Target = 0xE0 },
            new KeyMappingEntry { TargetKey = Key.ShiftLeft,    Target = 0xE1 },
            new KeyMappingEntry { TargetKey = Key.AltLeft,      Target = 0xE2 },
            new KeyMappingEntry { TargetKey = Key.WinLeft,      Target = 0xE3 },
            new KeyMappingEntry { TargetKey = Key.ControlRight, Target = 0xE4 },
            new KeyMappingEntry { TargetKey = Key.ShiftRight,   Target = 0xE5 },
            new KeyMappingEntry { TargetKey = Key.AltRight,     Target = 0xE6 },
            new KeyMappingEntry { TargetKey = Key.WinRight,     Target = 0xE7 },
        };

        private static readonly KeyMappingEntry[] KeyModifierMapping = new KeyMappingEntry[]
        {
            new KeyMappingEntry { TargetKey = Key.ControlLeft,  Target = 0 },
            new KeyMappingEntry { TargetKey = Key.ShiftLeft,    Target = 1 },
            new KeyMappingEntry { TargetKey = Key.AltLeft,      Target = 2 },
            new KeyMappingEntry { TargetKey = Key.WinLeft,      Target = 3 },
            new KeyMappingEntry { TargetKey = Key.ControlRight, Target = 4 },
            new KeyMappingEntry { TargetKey = Key.ShiftRight,   Target = 5 },
            new KeyMappingEntry { TargetKey = Key.AltRight,     Target = 6 },
            new KeyMappingEntry { TargetKey = Key.WinRight,     Target = 7 },
            new KeyMappingEntry { TargetKey = Key.CapsLock,     Target = 8 },
            new KeyMappingEntry { TargetKey = Key.ScrollLock,   Target = 9 },
            new KeyMappingEntry { TargetKey = Key.NumLock,      Target = 10 },
        };

        public HLE.Input.Keyboard GetKeysDown(KeyboardState keyboard)
        {
            HLE.Input.Keyboard hidKeyboard = new HLE.Input.Keyboard
            {
                    Modifier = 0,
                    Keys     = new int[0x8]
            };

            foreach (KeyMappingEntry entry in KeyMapping)
            {
                int value = keyboard[entry.TargetKey] ? 1 : 0;

                hidKeyboard.Keys[entry.Target / 0x20] |= (value << (entry.Target % 0x20));
            }

            foreach (KeyMappingEntry entry in KeyModifierMapping)
            {
                int value = keyboard[entry.TargetKey] ? 1 : 0;

                hidKeyboard.Modifier |= value << entry.Target;
            }

            return hidKeyboard;
        }
    }
}
