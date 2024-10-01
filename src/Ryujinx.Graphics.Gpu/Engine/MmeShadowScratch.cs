using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    /// <summary>
    /// Represents temporary storage used by macros.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1024)]
    struct MmeShadowScratch
    {
#pragma warning disable CS0169 // The private field is never used
        private uint _e0;
#pragma warning restore CS0169
        public ref uint this[int index] => ref AsSpan()[index];
        public Span<uint> AsSpan() => MemoryMarshal.CreateSpan(ref _e0, 256);
    }
}
