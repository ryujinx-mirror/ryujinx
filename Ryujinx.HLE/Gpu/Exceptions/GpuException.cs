using System;

namespace Ryujinx.HLE.Gpu.Exceptions
{
    class GpuException : Exception
    {
        public GpuException() : base() { }

        public GpuException(string ExMsg) : base(ExMsg) { }
    }
}