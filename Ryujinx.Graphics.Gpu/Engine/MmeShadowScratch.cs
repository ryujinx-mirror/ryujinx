using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    [StructLayout(LayoutKind.Sequential, Size = 1024)]
    struct MmeShadowScratch
    {
#pragma warning disable CS0169
        private uint _e0;
#pragma warning restore CS0169
        public ref uint this[int index] => ref ToSpan()[index];
        public Span<uint> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 256);
    }
}
