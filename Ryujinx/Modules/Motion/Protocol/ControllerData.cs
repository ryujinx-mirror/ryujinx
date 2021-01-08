using System.Runtime.InteropServices;

namespace Ryujinx.Modules.Motion
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ControllerDataRequest
    {
        public MessageType    Type;
        public SubscriberType SubscriberType;
        public byte           Slot;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] MacAddress;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ControllerDataResponse
    {
        public SharedResponse Shared;
        public byte           Connected;
        public uint           PacketId;
        public byte           ExtraButtons;
        public byte           MainButtons;
        public ushort         PSExtraInput;
        public ushort         LeftStickXY;
        public ushort         RightStickXY;
        public uint           DPadAnalog;
        public ulong          MainButtonsAnalog;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Touch1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Touch2;

        public ulong MotionTimestamp;
        public float AccelerometerX;
        public float AccelerometerY;
        public float AccelerometerZ;
        public float GyroscopePitch;
        public float GyroscopeYaw;
        public float GyroscopeRoll;
    }

    enum SubscriberType : byte
    {
        All,
        Slot,
        Mac
    }
}