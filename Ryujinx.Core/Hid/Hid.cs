using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using System;

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

        private object ShMemLock;

        private (AMemory, long)[] ShMemPositions;

        public Hid()
        {
            ShMemLock = new object();

            ShMemPositions = new (AMemory, long)[0];
        }

        internal void ShMemMap(object sender, EventArgs e)
        {
            HSharedMem SharedMem = (HSharedMem)sender;

            lock (ShMemLock)
            {
                ShMemPositions = SharedMem.GetVirtualPositions();

                (AMemory Memory, long Position) ShMem = ShMemPositions[ShMemPositions.Length - 1];

                Logging.Info(LogClass.ServiceHid, $"HID shared memory successfully mapped to 0x{ShMem.Position:x16}!");

                Init(ShMem.Memory, ShMem.Position);
            }
        }

        internal void ShMemUnmap(object sender, EventArgs e)
        {
            HSharedMem SharedMem = (HSharedMem)sender;

            lock (ShMemLock)
            {
                ShMemPositions = SharedMem.GetVirtualPositions();
            }
        }

        private void Init(AMemory Memory, long Position)
        {
            InitializeJoyconPair(
                Memory,
                Position,
                JoyConColor.Body_Neon_Red,
                JoyConColor.Buttons_Neon_Red,
                JoyConColor.Body_Neon_Blue,
                JoyConColor.Buttons_Neon_Blue);
        }

        private void InitializeJoyconPair(
            AMemory     Memory,
            long        Position,
            JoyConColor LeftColorBody,
            JoyConColor LeftColorButtons,
            JoyConColor RightColorBody,
            JoyConColor RightColorButtons)
        {
            long BaseControllerOffset = Position + HidControllersOffset + 8 * HidControllerSize;

            HidControllerType Type =
                HidControllerType.ControllerType_Handheld |
                HidControllerType.ControllerType_JoyconPair;

            bool IsHalf = false;

            HidControllerColorDesc SingleColorDesc =
                HidControllerColorDesc.ColorDesc_ColorsNonexistent;

            JoyConColor SingleColorBody    = JoyConColor.Black;
            JoyConColor SingleColorButtons = JoyConColor.Black;

            HidControllerColorDesc SplitColorDesc = 0;

            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x0,  (int)Type);

            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x4,  IsHalf ? 1 : 0);

            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x8,  (int)SingleColorDesc);
            Memory.WriteInt32Unchecked(BaseControllerOffset + 0xc,  (int)SingleColorBody);
            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x10, (int)SingleColorButtons);
            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x14, (int)SplitColorDesc);

            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x18, (int)LeftColorBody);
            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x1c, (int)LeftColorButtons);

            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x20, (int)RightColorBody);
            Memory.WriteInt32Unchecked(BaseControllerOffset + 0x24, (int)RightColorButtons);
        }

        public void SetJoyconButton(
            HidControllerId      ControllerId,
            HidControllerLayouts ControllerLayout,
            HidControllerButtons Buttons,
            HidJoystickPosition  LeftStick,
            HidJoystickPosition  RightStick)
        {
            lock (ShMemLock)
            {
                foreach ((AMemory Memory, long Position) in ShMemPositions)
                {
                    long ControllerOffset = Position + HidControllersOffset;

                    ControllerOffset += (int)ControllerId * HidControllerSize;

                    ControllerOffset += HidControllerHeaderSize;

                    ControllerOffset += (int)ControllerLayout * HidControllerLayoutsSize;

                    long LastEntry = Memory.ReadInt64Unchecked(ControllerOffset + 0x10);

                    long CurrEntry = (LastEntry + 1) % HidEntryCount;

                    long Timestamp = GetTimestamp();

                    Memory.WriteInt64Unchecked(ControllerOffset + 0x0,  Timestamp);
                    Memory.WriteInt64Unchecked(ControllerOffset + 0x8,  HidEntryCount);
                    Memory.WriteInt64Unchecked(ControllerOffset + 0x10, CurrEntry);
                    Memory.WriteInt64Unchecked(ControllerOffset + 0x18, HidEntryCount - 1);

                    ControllerOffset += HidControllersLayoutHeaderSize;

                    ControllerOffset += CurrEntry * HidControllersInputEntrySize;

                    Memory.WriteInt64Unchecked(ControllerOffset + 0x0,  Timestamp);
                    Memory.WriteInt64Unchecked(ControllerOffset + 0x8,  Timestamp);

                    Memory.WriteInt64Unchecked(ControllerOffset + 0x10, (uint)Buttons);

                    Memory.WriteInt32Unchecked(ControllerOffset + 0x18, LeftStick.DX);
                    Memory.WriteInt32Unchecked(ControllerOffset + 0x1c, LeftStick.DY);

                    Memory.WriteInt64Unchecked(ControllerOffset + 0x20, RightStick.DX);
                    Memory.WriteInt64Unchecked(ControllerOffset + 0x24, RightStick.DY);

                    Memory.WriteInt64Unchecked(ControllerOffset + 0x28,
                        (uint)HidControllerConnState.Controller_State_Connected |
                        (uint)HidControllerConnState.Controller_State_Wired);
                }
            }
        }

        public void SetTouchPoints(params HidTouchPoint[] Points)
        {
            lock (ShMemLock)
            {
                foreach ((AMemory Memory, long Position) in ShMemPositions)
                {
                    long TouchScreenOffset = Position + HidTouchScreenOffset;

                    long LastEntry = Memory.ReadInt64Unchecked(TouchScreenOffset + 0x10);

                    long CurrEntry = (LastEntry + 1) % HidEntryCount;

                    long Timestamp = GetTimestamp();

                    Memory.WriteInt64Unchecked(TouchScreenOffset + 0x0,  Timestamp);
                    Memory.WriteInt64Unchecked(TouchScreenOffset + 0x8,  HidEntryCount);
                    Memory.WriteInt64Unchecked(TouchScreenOffset + 0x10, CurrEntry);
                    Memory.WriteInt64Unchecked(TouchScreenOffset + 0x18, HidEntryCount - 1);
                    Memory.WriteInt64Unchecked(TouchScreenOffset + 0x20, Timestamp);

                    long TouchEntryOffset = TouchScreenOffset + HidTouchHeaderSize;

                    long LastEntryOffset = TouchEntryOffset + LastEntry * HidTouchEntrySize;

                    long SampleCounter = Memory.ReadInt64Unchecked(LastEntryOffset) + 1;

                    TouchEntryOffset += CurrEntry * HidTouchEntrySize;

                    Memory.WriteInt64Unchecked(TouchEntryOffset + 0x0, SampleCounter);
                    Memory.WriteInt64Unchecked(TouchEntryOffset + 0x8, Points.Length);

                    TouchEntryOffset += HidTouchEntryHeaderSize;

                    const int Padding = 0;

                    int Index = 0;

                    foreach (HidTouchPoint Point in Points)
                    {
                        Memory.WriteInt64Unchecked(TouchEntryOffset + 0x0,  Timestamp);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0x8,  Padding);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0xc,  Index++);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0x10, Point.X);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0x14, Point.Y);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0x18, Point.DiameterX);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0x1c, Point.DiameterY);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0x20, Point.Angle);
                        Memory.WriteInt32Unchecked(TouchEntryOffset + 0x24, Padding);

                        TouchEntryOffset += HidTouchEntryTouchSize;
                    }
                }
            }
        }

        private static long GetTimestamp()
        {
            return (long)((ulong)Environment.TickCount * 19_200);
        }
    }
}