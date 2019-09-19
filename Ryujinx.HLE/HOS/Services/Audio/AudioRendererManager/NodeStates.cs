using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    static class NodeStates
    {
        public static long GetWorkBufferSize(int totalMixCount)
        {
            int size = BitUtils.AlignUp(totalMixCount, AudioRendererConsts.BufferAlignment);

            if (size < 0)
            {
                size |= 7;
            }

            return 4 * (totalMixCount * totalMixCount) + 12 * totalMixCount + 2 * (size / 8);
        }
    }
}