using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    // TODO: Add missing structs
    unsafe struct HidSharedMemory
    {
        public ShMemDebugPad DebugPad;
        public ShMemTouchScreen TouchScreen;
        public ShMemMouse Mouse;
        public ShMemKeyboard Keyboard;
        public fixed byte BasicXpad[0x4 * 0x400];
        public fixed byte HomeButton[0x200];
        public fixed byte SleepButton[0x200];
        public fixed byte CaptureButton[0x200];
        public fixed byte InputDetector[0x10 * 0x80];
        public fixed byte UniquePad[0x10 * 0x400];
        public Array10<ShMemNpad> Npads;
        public fixed byte Gesture[0x800];
        public fixed byte ConsoleSixAxisSensor[0x20];
        fixed byte _padding[0x3de0];
    }
}
