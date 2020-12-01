using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GraphicBuffer : IFlattenable
    {
        public GraphicBufferHeader Header;
        public NvGraphicBuffer     Buffer;

        public int Width => Header.Width;
        public int Height => Header.Height;
        public PixelFormat Format => Header.Format;
        public int Usage => Header.Usage;

        public Rect ToRect()
        {
            return new Rect(Width, Height);
        }

        public void Flatten(Parcel parcel)
        {
            parcel.WriteUnmanagedType(ref Header);
            parcel.WriteUnmanagedType(ref Buffer);
        }

        public void Unflatten(Parcel parcel)
        {
            Header = parcel.ReadUnmanagedType<GraphicBufferHeader>();

            int expectedSize = Unsafe.SizeOf<NvGraphicBuffer>() / 4;

            if (Header.IntsCount != expectedSize)
            {
                throw new NotImplementedException($"Unexpected Graphic Buffer ints count (expected 0x{expectedSize:x}, found 0x{Header.IntsCount:x})");
            }

            Buffer = parcel.ReadUnmanagedType<NvGraphicBuffer>();
        }

        public void IncrementNvMapHandleRefCount(long pid)
        {
            NvMapDeviceFile.IncrementMapRefCount(pid, Buffer.NvMapId);

            for (int i = 0; i < Buffer.Surfaces.Length; i++)
            {
                NvMapDeviceFile.IncrementMapRefCount(pid, Buffer.Surfaces[i].NvMapHandle);
            }
        }

        public void DecrementNvMapHandleRefCount(long pid)
        {
            NvMapDeviceFile.DecrementMapRefCount(pid, Buffer.NvMapId);

            for (int i = 0; i < Buffer.Surfaces.Length; i++)
            {
                NvMapDeviceFile.DecrementMapRefCount(pid, Buffer.Surfaces[i].NvMapHandle);
            }
        }

        public uint GetFlattenedSize()
        {
            return (uint)Unsafe.SizeOf<GraphicBuffer>();
        }

        public uint GetFdCount()
        {
            return 0;
        }
    }
}