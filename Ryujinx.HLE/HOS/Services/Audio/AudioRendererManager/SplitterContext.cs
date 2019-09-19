using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    class SplitterContext
    {
        public static long CalcWorkBufferSize(BehaviorInfo behaviorInfo, AudioRendererParameter parameters)
        {
            if (!behaviorInfo.IsSplitterSupported())
            {
                return 0;
            }

            long size = parameters.SplitterDestinationDataCount * 0xE0 +
                        parameters.SplitterCount                * 0x20;

            if (!behaviorInfo.IsSplitterBugFixed())
            {
                size += BitUtils.AlignUp(4 * parameters.SplitterDestinationDataCount, 16);
            }

            return size;
        }
    }
}