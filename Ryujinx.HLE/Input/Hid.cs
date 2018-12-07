using Ryujinx.Common;
using Ryujinx.HLE.HOS;

namespace Ryujinx.HLE.Input
{
    public partial class Hid
    {
        private Switch _device;

        public HidControllerBase PrimaryController { get; private set; }

        internal long HidPosition;

        public Hid(Switch device, long hidPosition)
        {
            _device     = device;
            HidPosition = hidPosition;

            device.Memory.FillWithZeros(hidPosition, Horizon.HidSize);
        }

        public void InitilizePrimaryController(HidControllerType controllerType)
        {
            HidControllerId controllerId = controllerType == HidControllerType.Handheld ?
                HidControllerId.ControllerHandheld : HidControllerId.ControllerPlayer1;

            if (controllerType == HidControllerType.ProController)
            {
                PrimaryController = new HidProController(_device);
            }
            else
            {
                PrimaryController = new HidNpadController(controllerType,
                     _device,
                     (NpadColor.BodyNeonRed, NpadColor.BodyNeonRed),
                     (NpadColor.ButtonsNeonBlue, NpadColor.ButtonsNeonBlue));
            }

            PrimaryController.Connect(controllerId);
        }

        public HidControllerButtons UpdateStickButtons(
            HidJoystickPosition leftStick,
            HidJoystickPosition rightStick)
        {
            HidControllerButtons result = 0;

            if (rightStick.Dx < 0)
            {
                result |= HidControllerButtons.RStickLeft;
            }

            if (rightStick.Dx > 0)
            {
                result |= HidControllerButtons.RStickRight;
            }

            if (rightStick.Dy < 0)
            {
                result |= HidControllerButtons.RStickDown;
            }

            if (rightStick.Dy > 0)
            {
                result |= HidControllerButtons.RStickUp;
            }

            if (leftStick.Dx < 0)
            {
                result |= HidControllerButtons.LStickLeft;
            }

            if (leftStick.Dx > 0)
            {
                result |= HidControllerButtons.LStickRight;
            }

            if (leftStick.Dy < 0)
            {
                result |= HidControllerButtons.LStickDown;
            }

            if (leftStick.Dy > 0)
            {
                result |= HidControllerButtons.LStickUp;
            }

            return result;
        }

        public void SetTouchPoints(params HidTouchPoint[] points)
        {
            long touchScreenOffset = HidPosition + HidTouchScreenOffset;
            long lastEntry         = _device.Memory.ReadInt64(touchScreenOffset + 0x10);
            long currEntry         = (lastEntry + 1) % HidEntryCount;
            long timestamp         = GetTimestamp();

            _device.Memory.WriteInt64(touchScreenOffset + 0x00, timestamp);
            _device.Memory.WriteInt64(touchScreenOffset + 0x08, HidEntryCount);
            _device.Memory.WriteInt64(touchScreenOffset + 0x10, currEntry);
            _device.Memory.WriteInt64(touchScreenOffset + 0x18, HidEntryCount - 1);
            _device.Memory.WriteInt64(touchScreenOffset + 0x20, timestamp);

            long touchEntryOffset = touchScreenOffset + HidTouchHeaderSize;
            long lastEntryOffset  = touchEntryOffset + lastEntry * HidTouchEntrySize;
            long sampleCounter    = _device.Memory.ReadInt64(lastEntryOffset) + 1;

            touchEntryOffset += currEntry * HidTouchEntrySize;

            _device.Memory.WriteInt64(touchEntryOffset + 0x00, sampleCounter);
            _device.Memory.WriteInt64(touchEntryOffset + 0x08, points.Length);

            touchEntryOffset += HidTouchEntryHeaderSize;

            const int padding = 0;

            int index = 0;

            foreach (HidTouchPoint point in points)
            {
                _device.Memory.WriteInt64(touchEntryOffset + 0x00, sampleCounter);
                _device.Memory.WriteInt32(touchEntryOffset + 0x08, padding);
                _device.Memory.WriteInt32(touchEntryOffset + 0x0c, index++);
                _device.Memory.WriteInt32(touchEntryOffset + 0x10, point.X);
                _device.Memory.WriteInt32(touchEntryOffset + 0x14, point.Y);
                _device.Memory.WriteInt32(touchEntryOffset + 0x18, point.DiameterX);
                _device.Memory.WriteInt32(touchEntryOffset + 0x1c, point.DiameterY);
                _device.Memory.WriteInt32(touchEntryOffset + 0x20, point.Angle);
                _device.Memory.WriteInt32(touchEntryOffset + 0x24, padding);

                touchEntryOffset += HidTouchEntryTouchSize;
            }
        }

        internal static long GetTimestamp()
        {
            return PerformanceCounter.ElapsedMilliseconds * 19200;
        }
    }
}
