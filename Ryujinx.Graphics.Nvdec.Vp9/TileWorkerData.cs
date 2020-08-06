using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Nvdec.Vp9.Dsp;
using Ryujinx.Graphics.Nvdec.Vp9.Types;

namespace Ryujinx.Graphics.Nvdec.Vp9
{
    internal struct TileWorkerData
    {
        public Reader BitReader;
        public MacroBlockD Xd;
        /* dqcoeff are shared by all the planes. So planes must be decoded serially */
        public Array32<Array32<int>> Dqcoeff;
    }
}
