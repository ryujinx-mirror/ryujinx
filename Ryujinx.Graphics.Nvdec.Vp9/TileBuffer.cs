using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Nvdec.Vp9
{
    internal struct TileBuffer
    {
        public ArrayPtr<byte> Data;
        public int Size;
    }
}
