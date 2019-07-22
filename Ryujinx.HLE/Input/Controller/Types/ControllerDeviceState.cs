using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ControllerDeviceState
    {
        public ControllerDeviceType DeviceType;
        public int                  Padding;
        public DeviceFlags          DeviceFlags;
        public int                  UnintendedHomeButtonInputProtectionEnabled;
        public BatteryState         PowerInfo0BatteryState;
        public BatteryState         PowerInfo1BatteryState;
        public BatteryState         PowerInfo2BatteryState;
        public fixed byte           ControllerMac[16];
        public fixed byte           ControllerMac2[16];
    }
}
