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
            return 0;
        }

        bool QueryIsTextureBuffer(int handle, int cbufSlot = -1)
        {
            return false;
        }

        bool QueryIsTextureRectangle(int handle, int cbufSlot = -1)
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

        TextureFormat QueryTextureFormat(int handle, int cbufSlot = -1)
        {
            return TextureFormat.R8G8B8A8Unorm;
        }

        bool QueryEarlyZForce()
        {
            return false;
        }
    }
}
