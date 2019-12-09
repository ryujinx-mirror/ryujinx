using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    struct UniformBufferParams
    {
        public int AddressLow;
        public int AddressHighAndSize;

        public ulong PackAddress()
        {
            return (uint)AddressLow | ((ulong)(AddressHighAndSize & 0xff) << 32);
        }

        public ulong UnpackSize()
        {
            return (ulong)((AddressHighAndSize >> 15) & 0x1ffff);
        }
    }

    struct ComputeParams
    {
        public int Unknown0;
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;
        public int Unknown5;
        public int Unknown6;
        public int Unknown7;
        public int ShaderOffset;
        public int Unknown9;
        public int Unknown10;
        public SamplerIndex SamplerIndex;
        public int GridSizeX;
        public int GridSizeYZ;
        public int Unknown14;
        public int Unknown15;
        public int Unknown16;
        public int SharedMemorySize;
        public int BlockSizeX;
        public int BlockSizeYZ;
        public int UniformBuffersConfig;
        public int Unknown21;
        public int Unknown22;
        public int Unknown23;
        public int Unknown24;
        public int Unknown25;
        public int Unknown26;
        public int Unknown27;
        public int Unknown28;

        private UniformBufferParams _uniformBuffer0;
        private UniformBufferParams _uniformBuffer1;
        private UniformBufferParams _uniformBuffer2;
        private UniformBufferParams _uniformBuffer3;
        private UniformBufferParams _uniformBuffer4;
        private UniformBufferParams _uniformBuffer5;
        private UniformBufferParams _uniformBuffer6;
        private UniformBufferParams _uniformBuffer7;

        public Span<UniformBufferParams> UniformBuffers
        {
            get
            {
                return MemoryMarshal.CreateSpan(ref _uniformBuffer0, 8);
            }
        }

        public int Unknown45;
        public int Unknown46;
        public int Unknown47;
        public int Unknown48;
        public int Unknown49;
        public int Unknown50;
        public int Unknown51;
        public int Unknown52;
        public int Unknown53;
        public int Unknown54;
        public int Unknown55;
        public int Unknown56;
        public int Unknown57;
        public int Unknown58;
        public int Unknown59;
        public int Unknown60;
        public int Unknown61;
        public int Unknown62;
        public int Unknown63;

        public int UnpackGridSizeX()
        {
            return GridSizeX & 0x7fffffff;
        }

        public int UnpackGridSizeY()
        {
            return GridSizeYZ & 0xffff;
        }

        public int UnpackGridSizeZ()
        {
            return (GridSizeYZ >> 16) & 0xffff;
        }

        public int UnpackBlockSizeX()
        {
            return (BlockSizeX >> 16) & 0xffff;
        }

        public int UnpackBlockSizeY()
        {
            return BlockSizeYZ & 0xffff;
        }

        public int UnpackBlockSizeZ()
        {
            return (BlockSizeYZ >> 16) & 0xffff;
        }

        public uint UnpackUniformBuffersEnableMask()
        {
            return (uint)UniformBuffersConfig & 0xff;
        }
    }
}