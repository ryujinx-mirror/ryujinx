using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Video;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    internal struct Surface : ISurface
    {
        public ArrayPtr<byte> YBuffer;
        public ArrayPtr<byte> UBuffer;
        public ArrayPtr<byte> VBuffer;

        public unsafe Plane YPlane => new Plane((IntPtr)YBuffer.ToPointer(), YBuffer.Length);
        public unsafe Plane UPlane => new Plane((IntPtr)UBuffer.ToPointer(), UBuffer.Length);
        public unsafe Plane VPlane => new Plane((IntPtr)VBuffer.ToPointer(), VBuffer.Length);

        public FrameField Field => FrameField.Progressive;

        public int Width { get; }
        public int Height { get; }
        public int AlignedWidth { get; }
        public int AlignedHeight { get; }
        public int Stride { get; }
        public int UvWidth { get; }
        public int UvHeight { get; }
        public int UvAlignedWidth { get; }
        public int UvAlignedHeight { get; }
        public int UvStride { get; }
        public bool HighBd => false;

        private readonly IntPtr _pointer;

        public Surface(int width, int height)
        {
            const int border = 32;
            const int ssX = 1;
            const int ssY = 1;
            const bool highbd = false;

            int alignedWidth = (width + 7) & ~7;
            int alignedHeight = (height + 7) & ~7;
            int yStride = ((alignedWidth + 2 * border) + 31) & ~31;
            int yplaneSize = (alignedHeight + 2 * border) * yStride;
            int uvWidth = alignedWidth >> ssX;
            int uvHeight = alignedHeight >> ssY;
            int uvStride = yStride >> ssX;
            int uvBorderW = border >> ssX;
            int uvBorderH = border >> ssY;
            int uvplaneSize = (uvHeight + 2 * uvBorderH) * uvStride;

            int frameSize = (highbd ? 2 : 1) * (yplaneSize + 2 * uvplaneSize);

            IntPtr pointer = Marshal.AllocHGlobal(frameSize);
            _pointer = pointer;
            Width = width;
            Height = height;
            AlignedWidth = alignedWidth;
            AlignedHeight = alignedHeight;
            Stride = yStride;
            UvWidth = (width + ssX) >> ssX;
            UvHeight = (height + ssY) >> ssY;
            UvAlignedWidth = uvWidth;
            UvAlignedHeight = uvHeight;
            UvStride = uvStride;

            ArrayPtr<byte> NewPlane(int start, int size, int border)
            {
                return new ArrayPtr<byte>(pointer + start + border, size - border);
            }

            YBuffer = NewPlane(0, yplaneSize, (border * yStride) + border);
            UBuffer = NewPlane(yplaneSize, uvplaneSize, (uvBorderH * uvStride) + uvBorderW);
            VBuffer = NewPlane(yplaneSize + uvplaneSize, uvplaneSize, (uvBorderH * uvStride) + uvBorderW);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_pointer);
        }
    }
}
