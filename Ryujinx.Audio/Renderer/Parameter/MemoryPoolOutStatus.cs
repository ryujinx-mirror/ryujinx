using Ryujinx.Audio.Renderer.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information for a memory pool.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MemoryPoolOutStatus
    {
        /// <summary>
        /// The current server memory pool state.
        /// </summary>
        public MemoryPoolUserState State;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}
