using Ryujinx.Graphics.Gpu.Engine.Types;

namespace Ryujinx.Graphics.Gpu.Engine.Dma
{
    /// <summary>
    /// Buffer to texture copy parameters.
    /// </summary>
    struct DmaTexture
    {
#pragma warning disable CS0649
        public MemoryLayout MemoryLayout;
        public int Width;
        public int Height;
        public int Depth;
        public int RegionZ;
        public ushort RegionX;
        public ushort RegionY;
#pragma warning restore CS0649
    }
}