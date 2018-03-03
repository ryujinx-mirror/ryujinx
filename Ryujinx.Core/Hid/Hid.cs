using ChocolArm64.Memory;
using System.Diagnostics;

namespace Ryujinx.Core.Input
{
    public class Hid
    {
        /*
         * Reference:
         * https://github.com/reswitched/libtransistor/blob/development/lib/hid.c
         * https://github.com/reswitched/libtransistor/blob/development/include/libtransistor/hid.h
         * https://github.com/switchbrew/libnx/blob/master/nx/source/services/hid.c
         * https://github.com/switchbrew/libnx/blob/master/nx/include/switch/services/hid.h
         */

        private const int HidHeaderSize            = 0x400;
        private const int HidTouchScreenSize       = 0x3000;
        private const int HidMouseSize             = 0x400;
        private const int HidKeyboardSize          = 0x400;
        private const int HidUnkSection1Size       = 0x400;
        private const int HidUnkSection2Size       = 0x400;
        private const int HidUnkSection3Size       = 0x400;
        private const int HidUnkSection4Size       = 0x400;
        private const int HidUnkSection5Size       = 0x200;
        private const int HidUnkSection6Size       = 0x200;
        private const int HidUnkSection7Size       = 0x200;
        private const int HidUnkSection8Size       = 0x800;
        private const int HidControllerSerialsSize = 0x4000;
        private const int HidControllersSize       = 0x32000;
        private const int HidUnkSection9Size       = 0x800;

        private const int HidTouchHeaderSize = 0x28;
        private const int HidTouchEntrySize  = 0x298;

        private const int HidTouchEntryHeaderSize = 0x10;
        private const int HidTouchEntryTouchSize  = 0x28;

        private const int HidControllerSize        = 0x5000;
        private const int HidControllerHeaderSize  = 0x28;
        private const int HidControllerLayoutsSize = 0x350;

        private const int HidControllersLayoutHeaderSize = 0x20;
        private const int HidControllersInputEntrySize   = 0x30;

        private const int HidHeaderOffset            = 0;
        private const int HidTouchScreenOffset       = HidHeaderOffset            + HidHeaderSize;
        private const int HidMouseOffset             = HidTouchScreenOffset       + HidTouchScreenSize;
        private const int HidKeyboardOffset          = HidMouseOffset             + HidMouseSize;
        private const int HidUnkSection1Offset       = HidKeyboardOffset          + HidKeyboardSize;
        private const int HidUnkSection2Offset       = HidUnkSection1Offset       + HidUnkSection1Size;
        private const int HidUnkSection3Offset       = HidUnkSection2Offset       + HidUnkSection2Size;
        private const int HidUnkSection4Offset       = HidUnkSection3Offset       + HidUnkSection3Size;
        private const int HidUnkSection5Offset       = HidUnkSection4Offset       + HidUnkSection4Size;
        private const int HidUnkSection6Offset       = HidUnkSection5Offset       + HidUnkSection5Size;
        private const int HidUnkSection7Offset       = HidUnkSection6Offset       + HidUnkSection6Size;
        private const int HidUnkSection8Offset       = HidUnkSection7Offset       + HidUnkSection7Size;
        private const int HidControllerSerialsOffset = HidUnkSection8Offset       + HidUnkSection8Size;
        private const int HidControllersOffset       = HidControllerSerialsOffset + HidControllerSerialsSize;
        private const int HidUnkSection9Offset       = HidControllersOffset       + HidControllersSize;

        private const int HidEntryCount = 17;

        private long SharedMemOffset;

        private Switch Ns;

        public Hid(Switch Ns)
        {
            this.Ns = Ns;
        }

        public void Init(long HidOffset)
        {
            SharedMemOffset = HidOffset;

            InitializeJoyconPair(
                JoyConColor.Body_Neon_Red,
                JoyConColor.Buttons_Neon_Red,
                JoyConColor.Body_Neon_Blue,
                JoyConColor.Buttons_Neon_Blue);
        }

        public void InitializeJoyconPair(
            JoyConColor LeftColorBody,
            JoyConColor LeftColorButtons,
            JoyConColor RightColorBody,
            JoyConColor RightColorButtons)
        {
            long BaseControllerOffset = HidControllersOffset + 8 * HidControllerSize;

            HidControllerType Type =
                HidControllerType.ControllerType_Handheld |
                HidControllerType.ControllerType_JoyconPair;

            bool IsHalf = false;

            HidControllerColorDesc SingleColorDesc =
                HidControllerColorDesc.ColorDesc_ColorsNonexistent;

            JoyConColor SingleColorBody    = JoyConColor.Black;
            JoyConColor SingleColorButtons = JoyConColor.Black;

            HidControllerColorDesc SplitColorDesc = 0;

            WriteInt32(BaseControllerOffset + 0x0,  (int)Type);

            WriteInt32(BaseControllerOffset + 0x4,  IsHalf ? 1 : 0);

            WriteInt32(BaseControllerOffset + 0x8,  (int)SingleColorDesc);
            WriteInt32(BaseControllerOffset + 0xc,  (int)SingleColorBody);
            WriteInt32(BaseControllerOffset + 0x10, (int)SingleColorButtons);
            WriteInt32(BaseControllerOffset + 0x14, (int)SplitColorDesc);

            WriteInt32(BaseControllerOffset + 0x18, (int)LeftColorBody);
            WriteInt32(BaseControllerOffset + 0x1c, (int)LeftColorButtons);

            WriteInt32(BaseControllerOffset + 0x20, (int)RightColorBody);
            WriteInt32(BaseControllerOffset + 0x24, (int)RightColorButtons);
        }

        public void SetJoyconButton(
            HidControllerId      ControllerId,
            HidControllerLayouts ControllerLayout,
            HidControllerButtons Buttons,
            HidJoystickPosition  LeftStick,
            HidJoystickPosition  RightStick)
        {
            long ControllerOffset = HidControllersOffset + (int)ControllerId * HidControllerSize;

            ControllerOffset += HidControllerHeaderSize;

            ControllerOffset += (int)ControllerLayout * HidControllerLayoutsSize;

            long LastEntry = ReadInt64(ControllerOffset + 0x10);

            long CurrEntry = (LastEntry + 1) % HidEntryCount;

            long Timestamp = Stopwatch.GetTimestamp();

            WriteInt64(ControllerOffset + 0x0,  Timestamp);
            WriteInt64(ControllerOffset + 0x8,  HidEntryCount);
            WriteInt64(ControllerOffset + 0x10, CurrEntry);
            WriteInt64(ControllerOffset + 0x18, HidEntryCount - 1);

            ControllerOffset += HidControllersLayoutHeaderSize;

            ControllerOffset += CurrEntry * HidControllersInputEntrySize;

            WriteInt64(ControllerOffset + 0x0,  Timestamp);
            WriteInt64(ControllerOffset + 0x8,  Timestamp);

            WriteInt64(ControllerOffset + 0x10, (uint)Buttons);

            WriteInt32(ControllerOffset + 0x18, LeftStick.DX);
            WriteInt32(ControllerOffset + 0x1c, LeftStick.DY);

            WriteInt64(ControllerOffset + 0x20, RightStick.DX);
            WriteInt64(ControllerOffset + 0x24, RightStick.DY);

            WriteInt64(ControllerOffset + 0x28,
                (uint)HidControllerConnState.Controller_State_Connected |
                (uint)HidControllerConnState.Controller_State_Wired);
        }

        public void SetTouchPoints(params HidTouchPoint[] Points)
        {
            long LastEntry = ReadInt64(HidTouchScreenOffset + 0x10);

            long CurrEntry = (LastEntry + 1) % HidEntryCount;

            long Timestamp = Stopwatch.GetTimestamp();

            WriteInt64(HidTouchScreenOffset + 0x0,  Timestamp);
            WriteInt64(HidTouchScreenOffset + 0x8,  HidEntryCount);
            WriteInt64(HidTouchScreenOffset + 0x10, CurrEntry);
            WriteInt64(HidTouchScreenOffset + 0x18, HidEntryCount - 1);
            WriteInt64(HidTouchScreenOffset + 0x20, Timestamp);            

            long TouchEntryOffset = HidTouchScreenOffset + HidTouchHeaderSize;

            TouchEntryOffset += CurrEntry * HidTouchEntrySize;

            WriteInt64(TouchEntryOffset + 0x0, Timestamp);
            WriteInt64(TouchEntryOffset + 0x8, Points.Length);

            TouchEntryOffset += HidTouchEntryHeaderSize;

            const int Padding = 0;

            int Index = 0;

            foreach (HidTouchPoint Point in Points)
            {
                WriteInt64(TouchEntryOffset + 0x0,  Timestamp);
                WriteInt32(TouchEntryOffset + 0x8,  Padding);
                WriteInt32(TouchEntryOffset + 0xc,  Index++);
                WriteInt32(TouchEntryOffset + 0x10, Point.X);
                WriteInt32(TouchEntryOffset + 0x14, Point.Y);
                WriteInt32(TouchEntryOffset + 0x18, Point.DiameterX);
                WriteInt32(TouchEntryOffset + 0x1c, Point.DiameterY);
                WriteInt32(TouchEntryOffset + 0x20, Point.Angle);
                WriteInt32(TouchEntryOffset + 0x24, Padding);

                TouchEntryOffset += HidTouchEntryTouchSize;
            }
        }

        private unsafe long ReadInt64(long Position)
        {
            Position += SharedMemOffset;

            if ((ulong)Position + 8 > AMemoryMgr.AddrSize) return 0;

            return *((long*)((byte*)Ns.Ram + Position));
        }

        private unsafe void WriteInt32(long Position, int Value)
        {
            Position += SharedMemOffset;

            if ((ulong)Position + 4 > AMemoryMgr.AddrSize) return;

            *((int*)((byte*)Ns.Ram + Position)) = Value;
        }

        private unsafe void WriteInt64(long Position, long Value)
        {
            Position += SharedMemOffset;

            if ((ulong)Position + 8 > AMemoryMgr.AddrSize) return;

            *((long*)((byte*)Ns.Ram + Position)) = Value;
        }
    }
}