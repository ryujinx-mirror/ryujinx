using System;

namespace Ryujinx.Graphics.Video
{
    public readonly record struct Plane(IntPtr Pointer, int Length);
}
