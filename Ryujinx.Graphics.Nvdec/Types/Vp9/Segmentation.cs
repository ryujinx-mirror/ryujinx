using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Nvdec.Types.Vp9
{
    struct Segmentation
    {
        public byte Enabled;
        public byte UpdateMap;
        public byte TemporalUpdate;
        public byte AbsDelta;
        public Array8<uint> FeatureMask;
        public Array8<Array4<short>> FeatureData;
    }
}
