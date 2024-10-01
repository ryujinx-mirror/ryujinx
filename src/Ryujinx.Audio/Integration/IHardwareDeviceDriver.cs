using Ryujinx.Audio.Common;
using Ryujinx.Memory;
using System;
using System.Threading;

namespace Ryujinx.Audio.Integration
{
    /// <summary>
    /// Represent an hardware device driver used in <see cref="Output.AudioOutputSystem"/>.
    /// </summary>
    public interface IHardwareDeviceDriver : IDisposable
    {
        public enum Direction
        {
            Input,
            Output,
        }

        float Volume { get; set; }

        IHardwareDeviceSession OpenDeviceSession(Direction direction, IVirtualMemoryManager memoryManager, SampleFormat sampleFormat, uint sampleRate, uint channelCount);

        ManualResetEvent GetUpdateRequiredEvent();
        ManualResetEvent GetPauseEvent();

        bool SupportsDirection(Direction direction);
        bool SupportsSampleRate(uint sampleRate);
        bool SupportsSampleFormat(SampleFormat sampleFormat);
        bool SupportsChannelCount(uint channelCount);

        static abstract bool IsSupported { get; }

        IHardwareDeviceDriver GetRealDeviceDriver()
        {
            return this;
        }
    }
}
