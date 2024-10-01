using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input header for a splitter state update.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SplitterInParameter
    {
        /// <summary>
        /// Magic of the input header.
        /// </summary>
        public uint Magic;

        /// <summary>
        /// Target splitter id.
        /// </summary>
        public int Id;

        /// <summary>
        /// Target sample rate to use on the splitter.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Count of splitter destinations.
        /// </summary>
        /// <remarks>Splitter destination ids are defined right after this header.</remarks>
        public int DestinationCount;

        /// <summary>
        /// The expected constant of any input header.
        /// </summary>
        private const uint ValidMagic = 0x49444E53;

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
