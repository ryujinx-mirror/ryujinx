namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Buffer to texture copy parameters.
    /// </summary>
    struct CopyBufferTexture
    {
        public MemoryLayout MemoryLayout;
        public int          Width;
        public int          Height;
        public int          Depth;
        public int          RegionZ;
        public ushort       RegionX;
        public ushort       RegionY;
    }
}