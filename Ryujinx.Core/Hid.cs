using Ryujinx.Core.OsHle;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Core
{
    public class Hid
    {
        /*
        Thanks to:
        https://github.com/reswitched/libtransistor/blob/development/lib/hid.c
        https://github.com/reswitched/libtransistor/blob/development/include/libtransistor/hid.h
        https://github.com/switchbrew/libnx/blob/master/nx/source/services/hid.c
        https://github.com/switchbrew/libnx/blob/master/nx/include/switch/services/hid.h
        
        struct HidSharedMemory
        {
            header[0x400];
            touchscreen[0x3000];
            mouse[0x400];
            keyboard[0x400];
            unkSection1[0x400];
            unkSection2[0x400];
            unkSection3[0x400];
            unkSection4[0x400];
            unkSection5[0x200];
            unkSection6[0x200];
            unkSection7[0x200];
            unkSection8[0x800];
            controllerSerials[0x4000];
            controllers[0x5000 * 10]; 
            unkSection9[0x4600];
        }
        */

        private const int Hid_Num_Entries = 17;
        private Switch Ns;
        private long SharedMemOffset;

        public Hid(Switch Ns)
        {
            this.Ns = Ns;
        }

        public void Init(long HidOffset)
        {            
            unsafe
            {
                if (HidOffset == 0 || HidOffset + Horizon.HidSize > uint.MaxValue)
                {
                    return;
                }

                SharedMemOffset = HidOffset;

                uint InnerOffset = (uint)Marshal.SizeOf(typeof(HidSharedMemHeader));

                IntPtr HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset);

                HidTouchScreen TouchScreen = new HidTouchScreen();
                TouchScreen.Header.TimestampTicks = (ulong)Environment.TickCount;
                TouchScreen.Header.NumEntries = (ulong)Hid_Num_Entries;
                TouchScreen.Header.LatestEntry = 0;
                TouchScreen.Header.MaxEntryIndex = (ulong)Hid_Num_Entries - 1;
                TouchScreen.Header.Timestamp = (ulong)Environment.TickCount;
                
                Marshal.StructureToPtr(TouchScreen, HidPtr, false);

                InnerOffset += (uint)Marshal.SizeOf(typeof(HidTouchScreen));
                HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset);

                HidMouse Mouse = new HidMouse();
                Mouse.Header.TimestampTicks = (ulong)Environment.TickCount;
                Mouse.Header.NumEntries = (ulong)Hid_Num_Entries;
                Mouse.Header.LatestEntry = 0;
                Mouse.Header.MaxEntryIndex = (ulong)Hid_Num_Entries - 1;

                //TODO: Write this structure when the input is implemented
                //Marshal.StructureToPtr(Mouse, HidPtr, false);

                InnerOffset += (uint)Marshal.SizeOf(typeof(HidMouse));
                HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset);

                HidKeyboard Keyboard = new HidKeyboard();
                Keyboard.Header.TimestampTicks = (ulong)Environment.TickCount;
                Keyboard.Header.NumEntries = (ulong)Hid_Num_Entries;
                Keyboard.Header.LatestEntry = 0;
                Keyboard.Header.MaxEntryIndex = (ulong)Hid_Num_Entries - 1;

                //TODO: Write this structure when the input is implemented
                //Marshal.StructureToPtr(Keyboard, HidPtr, false);

                InnerOffset += (uint)Marshal.SizeOf(typeof(HidKeyboard)) +
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection1)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection2)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection3)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection4)) +
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection5)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection6)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection7)) +
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection8)) + 
                               (uint)Marshal.SizeOf(typeof(HidControllerSerials));

                //Increase the loop to initialize more controller.
                for (int i = 8; i < Enum.GetNames(typeof(HidControllerID)).Length - 1; i++)
                {
                    HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset + (uint)(Marshal.SizeOf(typeof(HidController)) * i));

                    HidController Controller = new HidController();
                    Controller.Header.Type = (uint)(HidControllerType.ControllerType_Handheld | HidControllerType.ControllerType_JoyconPair);
                    Controller.Header.IsHalf = 0;
                    Controller.Header.SingleColorsDescriptor = (uint)(HidControllerColorDescription.ColorDesc_ColorsNonexistent);
                    Controller.Header.SingleColorBody = 0;
                    Controller.Header.SingleColorButtons = 0;
                    Controller.Header.SplitColorsDescriptor = 0;
                    Controller.Header.LeftColorBody = (uint)JoyConColor.Body_Neon_Red;
                    Controller.Header.LeftColorButtons = (uint)JoyConColor.Buttons_Neon_Red;
                    Controller.Header.RightColorBody = (uint)JoyConColor.Body_Neon_Blue;
                    Controller.Header.RightColorButtons = (uint)JoyConColor.Buttons_Neon_Blue;

                    Controller.Layouts = new HidControllerLayout[Enum.GetNames(typeof(HidControllerLayouts)).Length];
                    Controller.Layouts[(int)HidControllerLayouts.Main] = new HidControllerLayout();
                    Controller.Layouts[(int)HidControllerLayouts.Main].Header.LatestEntry = (ulong)Hid_Num_Entries;

                    Marshal.StructureToPtr(Controller, HidPtr, false);
                }

                Logging.Info("HID Initialized!");
            }
        }

        public void SendControllerButtons(HidControllerID ControllerId, 
                                          HidControllerLayouts Layout, 
                                          HidControllerKeys Buttons, 
                                          JoystickPosition LeftJoystick, 
                                          JoystickPosition RightJoystick)
        {
            uint InnerOffset = (uint)Marshal.SizeOf(typeof(HidSharedMemHeader)) +
                               (uint)Marshal.SizeOf(typeof(HidTouchScreen)) +
                               (uint)Marshal.SizeOf(typeof(HidMouse)) +
                               (uint)Marshal.SizeOf(typeof(HidKeyboard)) +
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection1)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection2)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection3)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection4)) +
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection5)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection6)) + 
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection7)) +
                               (uint)Marshal.SizeOf(typeof(HidUnknownSection8)) + 
                               (uint)Marshal.SizeOf(typeof(HidControllerSerials)) +
                               ((uint)(Marshal.SizeOf(typeof(HidController)) * (int)ControllerId)) +
                               (uint)Marshal.SizeOf(typeof(HidControllerHeader)) +
                               (uint)Layout * (uint)Marshal.SizeOf(typeof(HidControllerLayout));

            IntPtr HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset);

            HidControllerLayoutHeader OldControllerHeaderLayout = (HidControllerLayoutHeader)Marshal.PtrToStructure(HidPtr, typeof(HidControllerLayoutHeader));

            HidControllerLayoutHeader ControllerLayoutHeader = new HidControllerLayoutHeader
            {
                TimestampTicks = (ulong)Environment.TickCount,
                NumEntries = (ulong)Hid_Num_Entries,
                MaxEntryIndex = (ulong)Hid_Num_Entries - 1,
                LatestEntry = (OldControllerHeaderLayout.LatestEntry < (ulong)Hid_Num_Entries ? OldControllerHeaderLayout.LatestEntry + 1 : 0)
            };

            Marshal.StructureToPtr(ControllerLayoutHeader, HidPtr, false);

            InnerOffset += (uint)Marshal.SizeOf(typeof(HidControllerLayoutHeader)) + (uint)((uint)(ControllerLayoutHeader.LatestEntry) * Marshal.SizeOf(typeof(HidControllerInputEntry)));
            HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset);

            HidControllerInputEntry ControllerInputEntry = new HidControllerInputEntry
            {
                Timestamp = (ulong)Environment.TickCount,
                Timestamp_2 = (ulong)Environment.TickCount,
                Buttons = (ulong)Buttons,
                Joysticks = new JoystickPosition[(int)HidControllerJoystick.Joystick_Num_Sticks]
            };
            ControllerInputEntry.Joysticks[(int)HidControllerJoystick.Joystick_Left] = LeftJoystick;
            ControllerInputEntry.Joysticks[(int)HidControllerJoystick.Joystick_Right] = RightJoystick;
            ControllerInputEntry.ConnectionState = (ulong)(HidControllerConnectionState.Controller_State_Connected | HidControllerConnectionState.Controller_State_Wired);

            Marshal.StructureToPtr(ControllerInputEntry, HidPtr, false);
        }

        public void SendTouchPoint(HidTouchScreenEntryTouch TouchPoint)
        {
            uint InnerOffset = (uint)Marshal.SizeOf(typeof(HidSharedMemHeader));

            IntPtr HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset);

            HidTouchScreenHeader OldTouchScreenHeader = (HidTouchScreenHeader)Marshal.PtrToStructure(HidPtr,typeof(HidTouchScreenHeader));

            HidTouchScreenHeader TouchScreenHeader = new HidTouchScreenHeader()
            {
                TimestampTicks = (ulong)Environment.TickCount,
                NumEntries = (ulong)Hid_Num_Entries,
                MaxEntryIndex = (ulong)Hid_Num_Entries - 1,
                Timestamp = (ulong)Environment.TickCount,
                LatestEntry = OldTouchScreenHeader.LatestEntry < Hid_Num_Entries-1 ? OldTouchScreenHeader.LatestEntry + 1 : 0
            };

            Marshal.StructureToPtr(TouchScreenHeader, HidPtr, false);

            InnerOffset += (uint)Marshal.SizeOf(typeof(HidTouchScreenHeader))
                + (uint)((uint)(OldTouchScreenHeader.LatestEntry) * Marshal.SizeOf(typeof(HidTouchScreenEntry)));
            HidPtr = new IntPtr(Ns.Ram.ToInt64() + (uint)SharedMemOffset + InnerOffset);            

            HidTouchScreenEntry hidTouchScreenEntry = new HidTouchScreenEntry()
            {
                Header = new HidTouchScreenEntryHeader()
                {
                    Timestamp = (ulong)Environment.TickCount,
                    NumTouches = 1
                },
                Touches = new HidTouchScreenEntryTouch[16]
            };

            //Only supports single touch
            hidTouchScreenEntry.Touches[0] = TouchPoint;

            Marshal.StructureToPtr(hidTouchScreenEntry, HidPtr, false);
        }
    }
}