using System;

namespace Ryujinx.Graphics.Vulkan
{
    [Flags]
    internal enum FeedbackLoopAspects
    {
        None = 0,
        Color = 1 << 0,
        Depth = 1 << 1,
    }
}
