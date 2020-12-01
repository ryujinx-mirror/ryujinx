using System;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    [Flags]
    enum GuestGpuStateFlags : byte
    {
        EarlyZForce = 1 << 0
    }
}
