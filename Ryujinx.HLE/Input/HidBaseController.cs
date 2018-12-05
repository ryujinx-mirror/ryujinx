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

        public HidControllerBase(HidControllerType ControllerType, Switch Device)
        {
            this.Device = Device;

            HidControllerType = ControllerType;
        }

        public virtual void Connect(HidControllerId ControllerId)
        {
            this.ControllerId = ControllerId;

            Offset = Device.Hid.HidPosition + HidControllersOffset + (int)ControllerId * HidControllerSize;

            Device.Memory.FillWithZeros(Offset, 0x5000);

            Device.Memory.WriteInt32(Offset + 0x00, (int)HidControllerType);
        }

        public abstract void SendInput(
            HidControllerButtons Buttons,
            HidJoystickPosition  LeftStick,
            HidJoystickPosition  RightStick);

        protected long WriteInput(
            HidControllerButtons Buttons,
            HidJoystickPosition  LeftStick,
            HidJoystickPosition  RightStick, 
            HidControllerLayouts ControllerLayout)
        {
            long ControllerOffset = Offset + HidControllerHeaderSize;

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

            return ControllerOffset;
        }
    }
}
