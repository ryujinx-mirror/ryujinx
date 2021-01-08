using System.Runtime.InteropServices;

namespace Ryujinx.Modules.Motion
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SharedResponse
    {
        public MessageType     Type;
        public byte            Slot;
        public SlotState       State;
        public DeviceModelType ModelType;
        public ConnectionType  ConnectionType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[]        MacAddress;
        public BatteryStatus BatteryStatus;
    }

    public enum SlotState : byte
    {
        Disconnected,
        Reserved,
        Connected
    }

    public enum DeviceModelType : byte
    {
        None,
        PartialGyro,
        FullGyro
    }

    public enum ConnectionType : byte
    {
        None,
        USB,
        Bluetooth
    }

    public enum BatteryStatus : byte
    {
        NA,
        Dying,
        Low,
        Medium,
        High,
        Full,
        Charging,
        Charged
    }
}