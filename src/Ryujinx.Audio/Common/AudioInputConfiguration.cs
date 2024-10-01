using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Audio user input configuration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioInputConfiguration
    {
        /// <summary>
        /// The target sample rate of the user.
        /// </summary>
        /// <remarks>Only 48000Hz is considered valid, other sample rates will be refused.</remarks>
        public uint SampleRate;

        /// <summary>
        /// The target channel count of the user.
        /// </summary>
        /// <remarks>Only Stereo and Surround are considered valid, other configurations will be refused.</remarks>
        /// <remarks>Not used in audin.</remarks>
        public ushort ChannelCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private readonly ushort _reserved;
    }
}
