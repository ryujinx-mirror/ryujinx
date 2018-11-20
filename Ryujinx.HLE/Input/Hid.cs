using Ryujinx.Common;
using Ryujinx.HLE.HOS;

namespace Ryujinx.HLE.Input
{
    public partial class Hid
    {
        private Switch Device;

        public HidControllerBase PrimaryController { get; private set; }

        internal long HidPosition;

        public Hid(Switch Device, long HidPosition)
        {
            this.Device      = Device;
            this.HidPosition = HidPosition;

            Device.Memory.FillWithZeros(HidPosition, Horizon.HidSize);
        }

        public void InitilizePrimaryController(HidControllerType ControllerType)
        {
            HidControllerId ControllerId = ControllerType == HidControllerType.Handheld ?
                HidControllerId.CONTROLLER_HANDHELD : HidControllerId.CONTROLLER_PLAYER_1;

            if (ControllerType == HidControllerType.ProController)
            {
                PrimaryController = new HidProController(Device);
            }
            else
            {
                PrimaryController = new HidNpadController(ControllerType,
                     Device,
                     (NpadColor.Body_Neon_Red, NpadColor.Body_Neon_Red),
                     (NpadColor.Buttons_Neon_Blue, NpadColor.Buttons_Neon_Blue));
            }

            PrimaryController.Connect(ControllerId);
        }

        private HidControllerButtons UpdateStickButtons(
            HidJoystickPosition LeftStick,
            HidJoystickPosition RightStick)
        {
            HidControllerButtons Result = 0;

            if (RightStick.DX < 0)
            {
                Result |= HidControllerButtons.RStickLeft;
            }

            if (RightStick.DX > 0)
            {
                Result |= HidControllerButtons.RStickRight;
            }

            if (RightStick.DY < 0)
            {
                Result |= HidControllerButtons.RStickDown;
            }

            if (RightStick.DY > 0)
            {
                Result |= HidControllerButtons.RStickUp;
            }

            if (LeftStick.DX < 0)
            {
                Result |= HidControllerButtons.LStickLeft;
            }

            if (LeftStick.DX > 0)
            {
                Result |= HidControllerButtons.LStickRight;
            }

            if (LeftStick.DY < 0)
            {
                Result |= HidControllerButtons.LStickDown;
            }

            if (LeftStick.DY > 0)
            {
                Result |= HidControllerButtons.LStickUp;
            }

            return Result;
        }

        public void SetTouchPoints(params HidTouchPoint[] Points)
        {
            long TouchScreenOffset = HidPosition + HidTouchScreenOffset;
            long LastEntry         = Device.Memory.ReadInt64(TouchScreenOffset + 0x10);
            long CurrEntry         = (LastEntry + 1) % HidEntryCount;
            long Timestamp         = GetTimestamp();

            Device.Memory.WriteInt64(TouchScreenOffset + 0x00, Timestamp);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x08, HidEntryCount);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x10, CurrEntry);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x18, HidEntryCount - 1);
            Device.Memory.WriteInt64(TouchScreenOffset + 0x20, Timestamp);

            long TouchEntryOffset = TouchScreenOffset + HidTouchHeaderSize;
            long LastEntryOffset  = TouchEntryOffset + LastEntry * HidTouchEntrySize;
            long SampleCounter    = Device.Memory.ReadInt64(LastEntryOffset) + 1;

            TouchEntryOffset += CurrEntry * HidTouchEntrySize;

            Device.Memory.WriteInt64(TouchEntryOffset + 0x00, SampleCounter);
            Device.Memory.WriteInt64(TouchEntryOffset + 0x08, Points.Length);

            TouchEntryOffset += HidTouchEntryHeaderSize;

            const int Padding = 0;

            int Index = 0;

            foreach (HidTouchPoint Point in Points)
            {
                Device.Memory.WriteInt64(TouchEntryOffset + 0x00, SampleCounter);
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

        internal static long GetTimestamp()
        {
            return PerformanceCounter.ElapsedMilliseconds * 19200;
        }
    }
}
