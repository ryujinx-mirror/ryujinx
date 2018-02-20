using System;
using System.Runtime.InteropServices;
 
namespace Ryujinx
{
    [Flags]
    public enum HidControllerKeys
    {
        KEY_A            = (1 << 0),
        KEY_B            = (1 << 1),
        KEY_X            = (1 << 2),
        KEY_Y            = (1 << 3),
        KEY_LSTICK       = (1 << 4),
        KEY_RSTICK       = (1 << 5),
        KEY_L            = (1 << 6),
        KEY_R            = (1 << 7),
        KEY_ZL           = (1 << 8),
        KEY_ZR           = (1 << 9),
        KEY_PLUS         = (1 << 10),
        KEY_MINUS        = (1 << 11),
        KEY_DLEFT        = (1 << 12),
        KEY_DUP          = (1 << 13),
        KEY_DRIGHT       = (1 << 14),
        KEY_DDOWN        = (1 << 15),
        KEY_LSTICK_LEFT  = (1 << 16),
        KEY_LSTICK_UP    = (1 << 17),
        KEY_LSTICK_RIGHT = (1 << 18),
        KEY_LSTICK_DOWN  = (1 << 19),
        KEY_RSTICK_LEFT  = (1 << 20),
        KEY_RSTICK_UP    = (1 << 21),
        KEY_RSTICK_RIGHT = (1 << 22),
        KEY_RSTICK_DOWN  = (1 << 23),
        KEY_SL           = (1 << 24),
        KEY_SR           = (1 << 25),

        // Pseudo-key for at least one finger on the touch screen
        KEY_TOUCH        = (1 << 26),

        // Buttons by orientation (for single Joy-Con), also works with Joy-Con pairs, Pro Controller
        KEY_JOYCON_RIGHT = (1 << 0),
        KEY_JOYCON_DOWN  = (1 << 1),
        KEY_JOYCON_UP    = (1 << 2),
        KEY_JOYCON_LEFT  = (1 << 3),

        // Generic catch-all directions, also works for single Joy-Con
        KEY_UP           = KEY_DUP | KEY_LSTICK_UP | KEY_RSTICK_UP,
        KEY_DOWN         = KEY_DDOWN | KEY_LSTICK_DOWN | KEY_RSTICK_DOWN,
        KEY_LEFT         = KEY_DLEFT | KEY_LSTICK_LEFT | KEY_RSTICK_LEFT,
        KEY_RIGHT        = KEY_DRIGHT | KEY_LSTICK_RIGHT | KEY_RSTICK_RIGHT,
    }

    public enum HidControllerID
    {
        CONTROLLER_PLAYER_1 = 0,
        CONTROLLER_PLAYER_2 = 1,
        CONTROLLER_PLAYER_3 = 2,
        CONTROLLER_PLAYER_4 = 3,
        CONTROLLER_PLAYER_5 = 4,
        CONTROLLER_PLAYER_6 = 5,
        CONTROLLER_PLAYER_7 = 6,
        CONTROLLER_PLAYER_8 = 7,
        CONTROLLER_HANDHELD = 8,
        CONTROLLER_UNKNOWN  = 9
    }

    public enum HidControllerJoystick
    {
        Joystick_Left       = 0,
        Joystick_Right      = 1,
        Joystick_Num_Sticks = 2
    }

    public enum HidControllerLayouts
    {
        Pro_Controller,
        Handheld_Joined,
        Joined,
        Left,
        Right,
        Main_No_Analog,
        Main
    }

    [Flags]
    public enum HidControllerConnectionState
    {
        Controller_State_Connected = (1 << 0),
        Controller_State_Wired     = (1 << 1)
    }

    [Flags]
    public enum HidControllerType
    {
        ControllerType_ProController = (1 << 0),
        ControllerType_Handheld      = (1 << 1),
        ControllerType_JoyconPair    = (1 << 2),
        ControllerType_JoyconLeft    = (1 << 3),
        ControllerType_JoyconRight   = (1 << 4)
    }

    public enum HidControllerColorDescription
    {
        ColorDesc_ColorsNonexistent = (1 << 1),
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x8)]
    public struct JoystickPosition
    {
        public int DX;
        public int DY;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public struct HidControllerMAC
    {
        public ulong Timestamp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] MAC;
        public ulong Unknown;
        public ulong Timestamp_2;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x28)]
    public struct HidControllerHeader
    {
        public uint Type;
        public uint IsHalf;
        public uint SingleColorsDescriptor;
        public uint SingleColorBody;
        public uint SingleColorButtons;
        public uint SplitColorsDescriptor;
        public uint LeftColorBody;
        public uint LeftColorButtons;
        public uint RightColorBody;
        public uint RightColorButtons;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public struct HidControllerLayoutHeader
    {
        public ulong TimestampTicks;
        public ulong NumEntries;
        public ulong LatestEntry;
        public ulong MaxEntryIndex;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x30)]
    public struct HidControllerInputEntry
    {
        public ulong Timestamp;
        public ulong Timestamp_2;
        public ulong Buttons;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)HidControllerJoystick.Joystick_Num_Sticks)]
        public JoystickPosition[] Joysticks;
        public ulong ConnectionState;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x350)]
    public struct HidControllerLayout
    {
        public HidControllerLayoutHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public HidControllerInputEntry[] Entries;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x5000)]
    public struct HidController
    {
        public HidControllerHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public HidControllerLayout[] Layouts;
        /*
            pro_controller
            handheld_joined
            joined
            left
            right
            main_no_analog
            main
        */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x2A70)]
        public byte[] Unknown_1;
        public HidControllerMAC MacLeft;
        public HidControllerMAC MacRight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xDF8)]
        public byte[] Unknown_2;
    }
}
