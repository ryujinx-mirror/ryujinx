using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Audio user buffer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioUserBuffer
    {
        /// <summary>
        /// Pointer to the next buffer (ignored).
        /// </summary>
        public ulong NextBuffer;

        /// <summary>
        /// Pointer to the user samples.
        /// </summary>
        public ulong Data;

        /// <summary>
        /// Capacity of the buffer (unused).
        /// </summary>
        public ulong Capacity;

        /// <summary>
        /// Size of the user samples region.
        /// </summary>
        public ulong DataSize;

        /// <summary>
        /// Offset in the user samples region (unused).
        /// </summary>
        public ulong DataOffset;
    }
}
