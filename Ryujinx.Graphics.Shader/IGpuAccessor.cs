namespace Ryujinx.Graphics.Shader
{
    public interface IGpuAccessor
    {
        void Log(string message)
        {
            // No default log output.
        }

        T MemoryRead<T>(ulong address) where T : unmanaged;

        bool MemoryMapped(ulong address)
        {
            return true;
        }

        int QueryComputeLocalSizeX()
        {
            return 1;
        }

        int QueryComputeLocalSizeY()
        {
            return 1;
        }

        int QueryComputeLocalSizeZ()
        {
            return 1;
        }

        int QueryComputeLocalMemorySize()
        {
            return 0x1000;
        }

        int QueryComputeSharedMemorySize()
        {
            return 0xc000;
        }

        uint QueryConstantBufferUse()
        {
            return 0xffff;
        }

        bool QueryIsTextureBuffer(int handle)
        {
            return false;
        }

        bool QueryIsTextureRectangle(int handle)
        {
            return false;
        }

        InputTopology QueryPrimitiveTopology()
        {
            return InputTopology.Points;
        }

        int QueryStorageBufferOffsetAlignment()
        {
            return 16;
        }

        bool QuerySupportsImageLoadFormatted()
        {
            return true;
        }

        bool QuerySupportsNonConstantTextureOffset()
        {
            return true;
        }

        TextureFormat QueryTextureFormat(int handle)
        {
            return TextureFormat.R8G8B8A8Unorm;
        }

        bool QueryEarlyZForce()
        {
            return false;
        }
    }
}
