using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Mouse
{
    struct MouseState : ISampledData
    {
        public ulong SamplingNumber;
        public int X;
        public int Y;
        public int DeltaX;
        public int DeltaY;
        public int WheelDeltaX;
        public int WheelDeltaY;
        public MouseButton Buttons;
        public MouseAttribute Attributes;

        ulong ISampledData.SamplingNumber => SamplingNumber;
    }
}
