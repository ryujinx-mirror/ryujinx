using System;

namespace Ryujinx.Memory
{
    /// <summary>
    /// Memory access permission control.
    /// </summary>
    [Flags]
    public enum MemoryPermission
    {
        /// <summary>
        /// No access is allowed on the memory region.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow reads on the memory region.
        /// </summary>
        Read = 1 << 0,

        /// <summary>
        /// Allow writes on the memory region.
        /// </summary>
        Write = 1 << 1,

        /// <summary>
        /// Allow code execution on the memory region.
        /// </summary>
        Execute = 1 << 2,

        /// <summary>
        /// Allow reads and writes on the memory region.
        /// </summary>
        ReadAndWrite = Read | Write,

        /// <summary>
        /// Allow reads and code execution on the memory region.
        /// </summary>
        ReadAndExecute = Read | Execute,

        /// <summary>
        /// Allow reads, writes, and code execution on the memory region.
        /// </summary>
        ReadWriteExecute = Read | Write | Execute,

        /// <summary>
        /// Indicates an invalid protection.
        /// </summary>
        Invalid = 255,
    }
}
