using System;

namespace Ryujinx.Graphics.Shader
{
    public interface IGpuAccessor
    {
        void Log(string message)
        {
            // No default log output.
        }

        uint ConstantBuffer1Read(int offset)
        {
            return 0;
        }

        ReadOnlySpan<ulong> GetCode(ulong address, int minimumSize);

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

        bool QueryHostHasFrontFacingBug()
        {
            return false;
        }

        bool QueryHostHasVectorIndexingBug()
        {
            return false;
        }

        int QueryHostStorageBufferOffsetAlignment()
        {
            return 16;
        }

        bool QueryHostSupportsImageLoadFormatted()
        {
            return true;
        }

        bool QueryHostSupportsNonConstantTextureOffset()
        {
            return true;
        }

        bool QueryHostSupportsShaderBallot()
        {
            return true;
        }

        bool QueryHostSupportsTextureShadowLod()
        {
            return true;
        }

        SamplerType QuerySamplerType(int handle, int cbufSlot = -1)
        {
            return SamplerType.Texture2D;
        }

        bool QueryIsTextureRectangle(int handle, int cbufSlot = -1)
        {
            return false;
        }

        InputTopology QueryPrimitiveTopology()
        {
            return InputTopology.Points;
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
