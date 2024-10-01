using Ryujinx.Audio.Integration;

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Represent an audio buffer that will be used by an <see cref="IHardwareDeviceSession"/>.
    /// </summary>
    public class AudioBuffer
    {
        /// <summary>
        /// Unique tag of this buffer.
        /// </summary>
        /// <remarks>Unique per session</remarks>
        public ulong BufferTag;

        /// <summary>
        /// Pointer to the user samples.
        /// </summary>
        public ulong DataPointer;

        /// <summary>
        /// Size of the user samples region.
        /// </summary>
        public ulong DataSize;

        /// <summary>
        ///  The timestamp at which the buffer was played.
        /// </summary>
        /// <remarks>Not used but useful for debugging</remarks>
        public ulong PlayedTimestamp;

        /// <summary>
        /// The user samples.
        /// </summary>
        public byte[] Data;
    }
}
