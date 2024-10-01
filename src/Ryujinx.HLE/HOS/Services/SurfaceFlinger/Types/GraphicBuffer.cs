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
        public NvGraphicBuffer Buffer;

        public readonly int Width => Header.Width;
        public readonly int Height => Header.Height;
        public readonly PixelFormat Format => Header.Format;
        public readonly int Usage => Header.Usage;

        public readonly Rect ToRect()
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

        public readonly void IncrementNvMapHandleRefCount(ulong pid)
        {
            NvMapDeviceFile.IncrementMapRefCount(pid, Buffer.NvMapId);

            for (int i = 0; i < NvGraphicBufferSurfaceArray.Length; i++)
            {
                NvMapDeviceFile.IncrementMapRefCount(pid, Buffer.Surfaces[i].NvMapHandle);
            }
        }

        public readonly void DecrementNvMapHandleRefCount(ulong pid)
        {
            NvMapDeviceFile.DecrementMapRefCount(pid, Buffer.NvMapId);

            for (int i = 0; i < NvGraphicBufferSurfaceArray.Length; i++)
            {
                NvMapDeviceFile.DecrementMapRefCount(pid, Buffer.Surfaces[i].NvMapHandle);
            }
        }

        public readonly uint GetFlattenedSize()
        {
            return (uint)Unsafe.SizeOf<GraphicBuffer>();
        }

        public readonly uint GetFdCount()
        {
            return 0;
        }
    }
}
