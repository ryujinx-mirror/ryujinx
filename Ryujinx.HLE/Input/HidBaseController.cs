using static Ryujinx.HLE.Input.Hid;

namespace Ryujinx.HLE.Input
{
    public abstract class HidControllerBase : IHidDevice
    {
        protected HidControllerType HidControllerType;
        protected Switch            Device;
        protected HidControllerId   ControllerId;

        public long Offset    { get; private set; }
        public bool Connected { get; protected set; }

        public HidControllerBase(HidControllerType controllerType, Switch device)
        {
            Device = device;

            HidControllerType = controllerType;
        }

        public virtual void Connect(HidControllerId controllerId)
        {
            ControllerId = controllerId;

            Offset = Device.Hid.HidPosition + HidControllersOffset + (int)controllerId * HidControllerSize;

            Device.Memory.FillWithZeros(Offset, 0x5000);

            Device.Memory.WriteInt32(Offset + 0x00, (int)HidControllerType);
        }

        public abstract void SendInput(
            HidControllerButtons buttons,
            HidJoystickPosition  leftStick,
            HidJoystickPosition  rightStick);

        protected long WriteInput(
            HidControllerButtons buttons,
            HidJoystickPosition  leftStick,
            HidJoystickPosition  rightStick, 
            HidControllerLayouts controllerLayout)
        {
            long controllerOffset = Offset + HidControllerHeaderSize;

            controllerOffset += (int)controllerLayout * HidControllerLayoutsSize;

            long lastEntry = Device.Memory.ReadInt64(controllerOffset + 0x10);
            long currEntry = (lastEntry + 1) % HidEntryCount;
            long timestamp = GetTimestamp();

            Device.Memory.WriteInt64(controllerOffset + 0x00, timestamp);
            Device.Memory.WriteInt64(controllerOffset + 0x08, HidEntryCount);
            Device.Memory.WriteInt64(controllerOffset + 0x10, currEntry);
            Device.Memory.WriteInt64(controllerOffset + 0x18, HidEntryCount - 1);

            controllerOffset += HidControllersLayoutHeaderSize;

            long lastEntryOffset = controllerOffset + lastEntry * HidControllersInputEntrySize;

            controllerOffset += currEntry * HidControllersInputEntrySize;

            long sampleCounter = Device.Memory.ReadInt64(lastEntryOffset) + 1;

            Device.Memory.WriteInt64(controllerOffset + 0x00, sampleCounter);
            Device.Memory.WriteInt64(controllerOffset + 0x08, sampleCounter);
            Device.Memory.WriteInt64(controllerOffset + 0x10, (uint)buttons);

            Device.Memory.WriteInt32(controllerOffset + 0x18, leftStick.Dx);
            Device.Memory.WriteInt32(controllerOffset + 0x1c, leftStick.Dy);
            Device.Memory.WriteInt32(controllerOffset + 0x20, rightStick.Dx);
            Device.Memory.WriteInt32(controllerOffset + 0x24, rightStick.Dy);

            return controllerOffset;
        }
    }
}
