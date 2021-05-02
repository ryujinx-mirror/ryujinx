using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    struct SixAxisSensorState : ISampledData
    {
        public ulong DeltaTime;
        public ulong SamplingNumber;
        public HidVector Acceleration;
        public HidVector AngularVelocity;
        public HidVector Angle;
        public Array9<float> Direction;
        public SixAxisSensorAttribute Attributes;
        private uint _reserved;

        ulong ISampledData.SamplingNumber => SamplingNumber;
    }
}