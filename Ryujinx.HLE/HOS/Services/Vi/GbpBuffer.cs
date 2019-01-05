using Ryujinx.Common;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Android
{
    [StructLayout(LayoutKind.Sequential, Size = 0x28)]
    struct GraphicBufferHeader
    {
        public int Magic;
        public int Width;
        public int Height;
        public int Stride;
        public int Format;
        public int Usage;

        public int Pid;
        public int RefCount;

        public int FdsCount;
        public int IntsCount;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x58)]
    struct NvGraphicBufferSurface
    {
        [FieldOffset(0)]
        public uint Width;

        [FieldOffset(0x4)]
        public uint Height;

        [FieldOffset(0x8)]
        public ColorFormat ColorFormat;

        [FieldOffset(0x10)]
        public int Layout;

        [FieldOffset(0x14)]
        public int Pitch;

        [FieldOffset(0x18)]
        public int NvMapHandle;

        [FieldOffset(0x1C)]
        public int Offset;

        [FieldOffset(0x20)]
        public int Kind;

        [FieldOffset(0x24)]
        public int BlockHeightLog2;

        [FieldOffset(0x28)]
        public int ScanFormat;

        [FieldOffset(0x30)]
        public long Flags;

        [FieldOffset(0x38)]
        public long Size;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct NvGraphicBufferSurfaceArray
    {
        [FieldOffset(0x0)]
        private NvGraphicBufferSurface Surface0;

        [FieldOffset(0x58)]
        private NvGraphicBufferSurface Surface1;

        [FieldOffset(0xb0)]
        private NvGraphicBufferSurface Surface2;

        public NvGraphicBufferSurface this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return Surface0;
                }
                else if (index == 1)
                {
                    return Surface1;
                }
                else if (index == 2)
                {
                    return Surface2;
                }

                throw new IndexOutOfRangeException();
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x144)]
    struct NvGraphicBuffer
    {
        [FieldOffset(0x4)]
        public int NvMapId;

        [FieldOffset(0xC)]
        public int Magic;

        [FieldOffset(0x10)]
        public int Pid;

        [FieldOffset(0x14)]
        public int Type;

        [FieldOffset(0x18)]
        public int Usage;

        [FieldOffset(0x1C)]
        public int PixelFormat;

        [FieldOffset(0x20)]
        public int ExternalPixelFormat;

        [FieldOffset(0x24)]
        public int Stride;

        [FieldOffset(0x28)]
        public int FrameBufferSize;

        [FieldOffset(0x2C)]
        public int PlanesCount;

        [FieldOffset(0x34)]
        public NvGraphicBufferSurfaceArray Surfaces;
    }

    struct GbpBuffer
    {
        public GraphicBufferHeader Header { get; private set; }
        public NvGraphicBuffer     Buffer { get; private set; }

        public int Size => Marshal.SizeOf<NvGraphicBuffer>() + Marshal.SizeOf<GraphicBufferHeader>();

        public GbpBuffer(BinaryReader reader)
        {
            Header = reader.ReadStruct<GraphicBufferHeader>();

            // ignore fds
            // TODO: check if that is used in official implementation
            reader.BaseStream.Position += Header.FdsCount * 4;

            if (Header.IntsCount != 0x51)
            {
                throw new System.NotImplementedException($"Unexpected Graphic Buffer ints count (expected 0x51, found 0x{Header.IntsCount:x}");
            }

            Buffer = reader.ReadStruct<NvGraphicBuffer>();
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteStruct(Header);
            writer.WriteStruct(Buffer);
        }
    }
}