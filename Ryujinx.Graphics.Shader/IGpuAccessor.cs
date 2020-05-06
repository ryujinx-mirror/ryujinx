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

        public bool QuerySupportsNonConstantTextureOffset()
        {
            return true;
        }

        public TextureFormat QueryTextureFormat(int handle)
        {
            return TextureFormat.R8G8B8A8Unorm;
        }
    }
}
