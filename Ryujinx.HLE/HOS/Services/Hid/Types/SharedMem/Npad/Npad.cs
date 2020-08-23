using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    // TODO: Add missing structs
    unsafe struct ShMemNpad
    {
        public NpadStateHeader Header;
        public Array7<NpadLayout> Layouts; // One for each NpadLayoutsIndex
        public Array6<NpadSixAxis> Sixaxis;
        public DeviceType DeviceType;
        uint _padding1;
        public NpadSystemProperties SystemProperties;
        public uint NpadSystemButtonProperties;
        public Array3<BatteryCharge> BatteryState;
        public fixed byte NfcXcdDeviceHandleHeader[0x20];
        public fixed byte NfcXcdDeviceHandleState[0x20 * 2];
        public ulong Mutex;
        public fixed byte NpadGcTriggerHeader[0x20];
        public fixed byte NpadGcTriggerState[0x18 * 17];
        fixed byte _padding2[0xC38];
    }
}