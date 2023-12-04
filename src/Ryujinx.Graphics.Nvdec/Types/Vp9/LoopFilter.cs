using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Nvdec.Types.Vp9
{
    struct LoopFilter
    {
#pragma warning disable CS0649 // Field is never assigned to
        public byte ModeRefDeltaEnabled;
        public Array4<sbyte> RefDeltas;
        public Array2<sbyte> ModeDeltas;
#pragma warning restore CS0649
    }
}
