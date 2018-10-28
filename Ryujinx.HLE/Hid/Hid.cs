using Ryujinx.Common;
using Ryujinx.HLE.HOS;
using System;

namespace Ryujinx.HLE.Input
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

        private Switch Device;

        private long HidPosition;

        public Hid(Switch Device, long HidPosition)
        {
            this.Device      = Device;
            this.HidPosition = HidPosition;

            Device.Memory.FillWithZeros(HidPosition, Horizon.HidSize);

            InitializeJoyconPair(
                JoyConColor.Body_Neon_Red,
                JoyConColor.Buttons_Neon_Red,
                JoyConColor.Body_Neon_Blue,
                JoyConColor.Buttons_Neon_Blue);
        }

        private void InitializeJoyconPair(
            JoyConColor LeftColorBody,
            JoyConColor LeftColorButtons,
            JoyConColor RightColorBody,
            JoyConColor RightColorButtons)
        {
            long BaseControllerOffset = HidPosition + HidControllersOffset + 8 * HidControllerSize;

            HidControllerType Type = HidControllerType.ControllerType_Handheld;

            bool IsHalf = false;

            HidControllerColorDesc SingleColorDesc =
                HidControllerColorDesc.ColorDesc_ColorsNonexistent;

            JoyConColor SingleColorBody    = JoyConColor.Black;
            JoyConColor SingleColorButtons = JoyConColor.Black;

            HidControllerColorDesc SplitColorDesc = 0;

            Device.Memory.WriteInt32(BaseControllerOffset + 0x00, (int)Type);

            Device.Memory.WriteInt32(BaseControllerOffset + 0x04, IsHalf ? 1 : 0);

            Device.Memory.WriteInt32(BaseControllerOffset + 0x08, (int)SingleColorDesc);
            Device.Memory.WriteInt32(BaseControllerOffset + 0x0c, (int)SingleColorBody);
            Device.Memory.WriteInt32(BaseControllerOffset + 0x10, (int)SingleColorButtons);
            Device.Memory.WriteInt32(BaseControllerOffset + 0x14, (int)SplitColorDesc);

            Device.Memory.WriteInt32(BaseControllerOffset + 0x18, (int)LeftColorBody);
            Device.Memory.WriteInt32(BaseControllerOffset + 0x1c, (int)LeftColorButtons);

            Device.Memory.WriteInt32(BaseControllerOffset + 0x20, (int)RightColorBody);
            Device.Memory.WriteInt32(BaseControllerOffset + 0x24, (int)RightColorButtons);
        }

        private HidControllerButtons UpdateStickButtons(
            HidJoystickPosition LeftStick,
            HidJoystickPosition RightStick)
        {
            HidControllerButtons Result = 0;

            if (RightStick.DX < 0)
            {
                Result |= HidControllerButtons.KEY_RSTICK_LEFT;
            }

            if (RightStick.DX > 0)
            {
                Result |= HidControllerButtons.KEY_RSTICK_RIGHT;
            }

            if (RightStick.DY < 0)
            {
                Result |= HidControllerButtons.KEY_RSTICK_DOWN;
            }

            if (RightStick.DY > 0)
            {
                Result |= HidControllerButtons.KEY_RSTICK_UP;
            }

            if (LeftStick.DX < 0)
            {
                Result |= HidControllerButtons.KEY_LSTICK_LEFT;
            }

            if (LeftStick.DX > 0)
            {
                Result |= HidControllerButtons.KEY_LSTICK_RIGHT;
            }

            if (LeftStick.DY < 0)
            {
                Result |= HidControllerButtons.KEY_LSTICK_DOWN;
            }

            if (LeftStick.DY > 0)
            {
                Result |= HidControllerButtons.KEY_LSTICK_UP;
            }

            return Result;
        }

        public void SetJoyconButton(
            HidControllerId      ControllerId,
            HidControllerLayouts ControllerLayout,
            HidControllerButtons Buttons,
            HidJoystickPosition  LeftStick,
            HidJoystickPosition  RightStick)
        {
            Buttons |= UpdateStickButtons(LeftStick, RightStick);

            long ControllerOffset = HidPosition + HidControllersOffset;

            ControllerOffset += (int)ControllerId * HidControllerSize;

            ControllerOffset += HidControllerHeaderSize;

            ControllerOffset += (int)ControllerLayout * HidControllerLayoutsSize;

            long LastEntry = Device.Memory.ReadInt64(ControllerOffset + 0x10);

            long CurrEntry = (LastEntry + 1) % HidEntryCount;

            long Timestamp = GetTimestamp();

            Device.Memory.WriteInt64(ControllerOffset + 0x00, Timestamp);
            Device.Memory.WriteInt64(ControllerOffset + 0x08, HidEntryCount);
            Device.Memory.WriteInt64(ControllerOffset + 0x10, CurrEntry);
            Device.Memory.WriteInt64(ControllerOffset + 0x18, HidEntryCount - 1);

            ControllerOffset += HidControllersLayoutHeaderSize;

            long LastEntryOffset = ControllerOffset + LastEntry * HidControllersInputEntrySize;

            ControllerOffset += CurrEntry * HidControllersInputEntrySize;

            long SampleCounter = Device.Memory.ReadInt64(LastEntryOffset) + 1;

            Device.Memory.WriteInt64(ControllerOffset + 0x00, SampleCounter);
            Device.Memory.WriteInt64(ControllerOffset + 0x08, SampleCounter);

            Device.Memory.WriteInt64(ControllerOffset + 0x10, (uint)Buttons);

            Device.Memory.WriteInt32(ControllerOffset + 0x18, LeftStick.DX);
            Device.Memory.WriteInt32(ControllerOffset + 0x1c, LeftStick.DY);

            Device.Memory.WriteInt32(ControllerOffset + 0x20, RightStick.DX);
            Device.Memory.WriteInt32(ControllerOffset + 0x24, RightStick.DY);

            Device.Memory.WriteInt64(ControllerOffset + 0x28,
                (uint)HidControllerConnState.Controller_State_Connected |
                (uint)HidControllerConnState.Controller_State_Wired);
        }

        public void SetTouchPoints(params HidTouchPoint[] Points)
        {
            long TouchScreenOffset = HidPosition + HidTouchScreenOffset;

            long LastEntry = Device.Memory.ReadInt64(TouchScreenOffset + 0x10);

            long CurrEntry = (LastEntry + 1) % HidEntryCount;

            long Timestamp = GetTimestamp();

            Device.Memory.WriteInt64(TouchScreenOffset + 0x00, Timestamp);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x08, HidEntryCount);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x10, CurrEntry);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x18, HidEntryCount - 1);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x20, Timestamp);

            long TouchEntryOffset = TouchScreenOffset + HidTouchHeaderSize;

            long LastEntryOffset = TouchEntryOffset + LastEntry * HidTouchEntrySize;

            long SampleCounter = Device.Memory.ReadInt64(LastEntryOffset) + 1;

            TouchEntryOffset += CurrEntry * HidTouchEntrySize;

            Device.Memory.WriteInt64(TouchEntryOffset + 0x00, SampleCounter);
            Device.Memory.WriteInt64(TouchEntryOffset + 0x08, Points.Length);

            TouchEntryOffset += HidTouchEntryHeaderSize;

            const int Padding = 0;

            int Index = 0;

            foreach (HidTouchPoint Point in Points)
            {
                Device.Memory.WriteInt64(TouchEntryOffset + 0x00, Timestamp);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x08, Padding);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x0c, Index++);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x10, Point.X);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x14, Point.Y);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x18, Point.DiameterX);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x1c, Point.DiameterY);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x20, Point.Angle);
                Device.Memory.WriteInt32(TouchEntryOffset + 0x24, Padding);

                TouchEntryOffset += HidTouchEntryTouchSize;
            }
        }

        private static long GetTimestamp()
        {
            return PerformanceCounter.ElapsedMilliseconds * 19200;
        }
    }
}
