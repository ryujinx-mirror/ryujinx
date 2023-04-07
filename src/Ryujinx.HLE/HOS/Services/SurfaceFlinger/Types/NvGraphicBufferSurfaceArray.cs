using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
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

        public int Length => 3;
    }
}