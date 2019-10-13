namespace Ryujinx.Graphics.Gpu.State
{
    struct CopyBufferSwizzle
    {
        public uint Swizzle;

        public int UnpackComponentSize()
        {
            return (int)((Swizzle >> 16) & 3) + 1;
        }

        public int UnpackSrcComponentsCount()
        {
            return (int)((Swizzle >> 20) & 7) + 1;
        }

        public int UnpackDstComponentsCount()
        {
            return (int)((Swizzle >> 24) & 7) + 1;
        }
    }
}