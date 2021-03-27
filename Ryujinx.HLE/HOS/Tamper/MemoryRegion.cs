using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Tamper
{
    /// <summary>
    /// The regions in the virtual address space of the process that are used as base address of memory operations.
    /// </summary>
    enum MemoryRegion
    {
        /// <summary>
        /// The position of the NSO associated with the cheat in the virtual address space.
        /// NOTE: A game can have several NSOs, but the cheat only associates itself with one.
        /// </summary>
        NSO = 0x0,

        /// <summary>
        /// The address of the heap, as determined by the kernel.
        /// </summary>
        Heap = 0x1
    }
}
