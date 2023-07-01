using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information header for mix updates on REV7 and later
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MixInParameterDirtyOnlyUpdate
    {
        /// <summary>
        /// Magic of the header
        /// </summary>
        /// <remarks>Never checked on hardware.</remarks>
        public uint Magic;

        /// <summary>
        /// The count of <see cref="MixParameter"/> following this header.
        /// </summary>
        public uint MixCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed byte _reserved[24];
    }
}
