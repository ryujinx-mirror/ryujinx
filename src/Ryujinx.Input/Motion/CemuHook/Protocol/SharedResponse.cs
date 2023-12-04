using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Input.Motion.CemuHook.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SharedResponse
    {
        public MessageType Type;
        public byte Slot;
        public SlotState State;
        public DeviceModelType ModelType;
        public ConnectionType ConnectionType;

        public Array6<byte> MacAddress;
        public BatteryStatus BatteryStatus;
    }

    public enum SlotState : byte
    {
        Disconnected,
        Reserved,
        Connected,
    }

    public enum DeviceModelType : byte
    {
        None,
        PartialGyro,
        FullGyro,
    }

    public enum ConnectionType : byte
    {
        None,
        USB,
        Bluetooth,
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
        Charged,
    }
}
