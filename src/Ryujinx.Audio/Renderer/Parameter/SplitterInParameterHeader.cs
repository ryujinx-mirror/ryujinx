using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input header for splitter update.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SplitterInParameterHeader
    {
        /// <summary>
        /// Magic of the input header.
        /// </summary>
        public uint Magic;

        /// <summary>
        /// The count of <see cref="SplitterInParameter"/> after the header.
        /// </summary>
        public uint SplitterCount;

        /// <summary>
        /// The count of splitter destinations after the header and splitter info.
        /// </summary>
        public uint SplitterDestinationCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[5];

        /// <summary>
        /// The expected constant of any input splitter header.
        /// </summary>
        private const uint ValidMagic = 0x48444E53;

        /// <summary>
        /// Check if the magic is valid.
        /// </summary>
        /// <returns>Returns true if the magic is valid.</returns>
        public readonly bool IsMagicValid()
        {
            return Magic == ValidMagic;
        }
    }
}
