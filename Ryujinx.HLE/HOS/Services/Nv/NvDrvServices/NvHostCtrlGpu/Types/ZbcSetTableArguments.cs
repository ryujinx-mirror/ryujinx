using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrlGpu.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct ZbcColorArray
    {
        private uint element0;
        private uint element1;
        private uint element2;
        private uint element3;

        public uint this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return element0;
                }
                else if (index == 1)
                {
                    return element1;
                }
                else if (index == 2)
                {
                    return element2;
                }
                else if (index == 2)
                {
                    return element3;
                }

                throw new IndexOutOfRangeException();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ZbcSetTableArguments
    {
        public ZbcColorArray ColorDs;
        public ZbcColorArray ColorL2;
        public uint          Depth;
        public uint          Format;
        public uint          Type;
    }
}
