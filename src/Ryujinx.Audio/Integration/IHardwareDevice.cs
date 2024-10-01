using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Integration
{
    /// <summary>
    /// Represent an hardware device used in <see cref="Renderer.Dsp.Command.DeviceSinkCommand"/>
    /// </summary>
    public interface IHardwareDevice : IDisposable
    {
        /// <summary>
        /// Sets the volume level for this device.
        /// </summary>
        /// <param name="volume">The volume level to set.</param>
        void SetVolume(float volume);

        /// <summary>
        /// Gets the volume level for this device.
        /// </summary>
        /// <returns>The volume level of this device.</returns>
        float GetVolume();

        /// <summary>
        /// Get the supported sample rate of this device.
        /// </summary>
        /// <returns>The supported sample rate of this device.</returns>
        uint GetSampleRate();

        /// <summary>
        /// Get the channel count supported by this device.
        /// </summary>
        /// <returns>The channel count supported by this device.</returns>
        uint GetChannelCount();

        /// <summary>
        /// Appends new PCM16 samples to the device.
        /// </summary>
        /// <param name="data">The new PCM16 samples.</param>
        /// <param name="channelCount">The number of channels.</param>
        void AppendBuffer(ReadOnlySpan<short> data, uint channelCount);

        /// <summary>
        /// Check if the audio renderer needs to perform downmixing.
        /// </summary>
        /// <returns>True if downmixing is needed.</returns>
        public bool NeedDownmixing()
        {
            uint channelCount = GetChannelCount();

            Debug.Assert(channelCount > 0 && channelCount <= Constants.ChannelCountMax);

            return channelCount != Constants.ChannelCountMax;
        }
    }
}
