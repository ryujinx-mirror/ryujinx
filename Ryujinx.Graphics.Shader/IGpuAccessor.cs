namespace Ryujinx.Graphics.Shader
{
    public interface IGpuAccessor
    {
        public void Log(string message)
        {
            // No default log output.
        }

        T MemoryRead<T>(ulong address) where T : unmanaged;

        public int QueryComputeLocalSizeX()
        {
            return 1;
        }

        public int QueryComputeLocalSizeY()
        {
            return 1;
        }

        public int QueryComputeLocalSizeZ()
        {
            return 1;
        }

        public int QueryComputeLocalMemorySize()
        {
            return 0x1000;
        }

        public int QueryComputeSharedMemorySize()
        {
            return 0xc000;
        }

        public bool QueryIsTextureBuffer(int handle)
        {
            return false;
        }

        public bool QueryIsTextureRectangle(int handle)
        {
            return false;
        }

        public InputTopology QueryPrimitiveTopology()
        {
            return InputTopology.Points;
        }

        public int QueryStorageBufferOffsetAlignment()
        {
            return 16;
        }

        public bool QuerySupportsImageLoadFormatted()
        {
            return true;
        }

        public bool QuerySupportsNonConstantTextureOffset()
        {
            return true;
        }

        public bool QuerySupportsViewportSwizzle()
        {
            return true;
        }

        public TextureFormat QueryTextureFormat(int handle)
        {
            return TextureFormat.R8G8B8A8Unorm;
        }

        public int QueryViewportSwizzle(int component)
        {
            // Bit 0: Negate flag.
            // Bits 2-1: Component.
            // Example: 0b110 = W, 0b111 = -W, 0b000 = X, 0b010 = Y etc.
            return component << 1;
        }
    }
}
