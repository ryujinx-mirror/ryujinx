using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Nvdec.Vp9.Dsp;
using Ryujinx.Graphics.Nvdec.Vp9.Types;
using Ryujinx.Graphics.Video;

namespace Ryujinx.Graphics.Nvdec.Vp9
{
    internal struct TileWorkerData
    {
        public ArrayPtr<byte> DataEnd;
        public int BufStart;
        public int BufEnd;
        public Reader BitReader;
        public Vp9BackwardUpdates Counts;
        public MacroBlockD Xd;
        /* dqcoeff are shared by all the planes. So planes must be decoded serially */
        public Array32<Array32<int>> Dqcoeff;
        public InternalErrorInfo ErrorInfo;
    }
}
