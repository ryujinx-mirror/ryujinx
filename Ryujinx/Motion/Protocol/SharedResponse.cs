using System.Runtime.InteropServices;

namespace Ryujinx.Motion
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SharedResponse
    {
        public MessageType Type;
        public byte Slot;
        public SlotState State;
        public DeviceModelType ModelType;
        public ConnectionType ConnectionType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] MacAddress;
        public BatteryStatus BatteryStatus;
    }

    public enum SlotState : byte
    {
        Disconnected = 0,
        Reserved,
        Connected
    }

    public enum DeviceModelType : byte
    {
        None = 0,
        PartialGyro,
        FullGyro
    }

    public enum ConnectionType : byte
    {
        None = 0,
        USB,
        Bluetooth
    }

    public enum BatteryStatus : byte
    {
        NA = 0,
        Dying,
        Low,
        Medium,
        High,
        Full,
        Charging,
        Charged
    }
}
