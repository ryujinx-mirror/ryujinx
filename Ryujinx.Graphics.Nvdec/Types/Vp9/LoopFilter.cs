using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Nvdec.Types.Vp9
{
    struct LoopFilter
    {
        public byte ModeRefDeltaEnabled;
        public Array4<sbyte> RefDeltas;
        public Array2<sbyte> ModeDeltas;
    }
}
