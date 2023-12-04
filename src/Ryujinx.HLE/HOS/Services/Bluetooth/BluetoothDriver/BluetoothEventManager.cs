using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Bluetooth.BluetoothDriver
{
    static class BluetoothEventManager
    {
        public static KEvent InitializeBleDebugEvent;
        public static int InitializeBleDebugEventHandle;

        public static KEvent UnknownBleDebugEvent;
        public static int UnknownBleDebugEventHandle;

        public static KEvent RegisterBleDebugEvent;
        public static int RegisterBleDebugEventHandle;

        public static KEvent InitializeBleEvent;
        public static int InitializeBleEventHandle;

        public static KEvent UnknownBleEvent;
        public static int UnknownBleEventHandle;

        public static KEvent RegisterBleEvent;
        public static int RegisterBleEventHandle;
    }
}
