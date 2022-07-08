using Ryujinx.Audio.Renderer.Common;
using System.Runtime.InteropServices;
using CpuAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a memory pool.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MemoryPoolInParameter
    {
        /// <summary>
        /// The CPU address used by the memory pool.
        /// </summary>
        public CpuAddress CpuAddress;

        /// <summary>
        /// The size used by the memory pool.
        /// </summary>
        public ulong Size;

        /// <summary>
        /// The target state the user wants.
        /// </summary>
        public MemoryPoolUserState State;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}
