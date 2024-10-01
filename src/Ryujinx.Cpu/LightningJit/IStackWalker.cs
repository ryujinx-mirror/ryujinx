using System;
using System.Collections.Generic;

namespace Ryujinx.Cpu.LightningJit
{
    interface IStackWalker
    {
        IEnumerable<ulong> GetCallStack(IntPtr framePointer, IntPtr codeRegionStart, int codeRegionSize, IntPtr codeRegion2Start, int codeRegion2Size);
    }
}
