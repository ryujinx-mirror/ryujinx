using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    partial class AudioSnoopManager : IAudioSnoopManager
    {
        // Note: The interface changed completely on firmware 17.0.0, this implementation is for older firmware.

        [CmifCommand(0)]
        public Result EnableDspUsageMeasurement()
        {
            return Result.Success;
        }

        [CmifCommand(1)]
        public Result DisableDspUsageMeasurement()
        {
            return Result.Success;
        }

        [CmifCommand(6)]
        public Result GetDspUsage(out uint usage)
        {
            usage = 0;

            return Result.Success;
        }
    }
}
