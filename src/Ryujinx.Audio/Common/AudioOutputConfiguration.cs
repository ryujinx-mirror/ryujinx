using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Audio system output configuration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioOutputConfiguration
    {
        /// <summary>
        /// The target sample rate of the system.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// The target channel count of the system.
        /// </summary>
        public uint ChannelCount;

        /// <summary>
        /// Reserved/unused
        /// </summary>
        public SampleFormat SampleFormat;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private Array3<byte> _padding;

        /// <summary>
        /// The initial audio system state.
        /// </summary>
        public AudioDeviceState AudioOutState;
    }
}
