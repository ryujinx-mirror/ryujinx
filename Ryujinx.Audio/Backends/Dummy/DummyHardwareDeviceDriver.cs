using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Memory;
using System.Threading;

using static Ryujinx.Audio.Integration.IHardwareDeviceDriver;

namespace Ryujinx.Audio.Backends.Dummy
{
    public class DummyHardwareDeviceDriver : IHardwareDeviceDriver
    {
        private ManualResetEvent _updateRequiredEvent;
        private ManualResetEvent _pauseEvent;

        public static bool IsSupported => true;

        public DummyHardwareDeviceDriver()
        {
            _updateRequiredEvent = new ManualResetEvent(false);
            _pauseEvent = new ManualResetEvent(true);
        }

        public IHardwareDeviceSession OpenDeviceSession(Direction direction, IVirtualMemoryManager memoryManager, SampleFormat sampleFormat, uint sampleRate, uint channelCount, float volume)
        {
            if (sampleRate == 0)
            {
                sampleRate = Constants.TargetSampleRate;
            }

            if (channelCount == 0)
            {
                channelCount = 2;
            }

            if (direction == Direction.Output)
            {
                return new DummyHardwareDeviceSessionOutput(this, memoryManager, sampleFormat, sampleRate, channelCount, volume);
            }
            else
            {
                return new DummyHardwareDeviceSessionInput(this, memoryManager, sampleFormat, sampleRate, channelCount);
            }
        }

        public ManualResetEvent GetUpdateRequiredEvent()
        {
            return _updateRequiredEvent;
        }

        public ManualResetEvent GetPauseEvent()
        {
            return _pauseEvent;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // NOTE: The _updateRequiredEvent will be disposed somewhere else.
                _pauseEvent.Dispose();
            }
        }

        public bool SupportsSampleRate(uint sampleRate)
        {
            return true;
        }

        public bool SupportsSampleFormat(SampleFormat sampleFormat)
        {
            return true;
        }

        public bool SupportsDirection(Direction direction)
        {
            return direction == Direction.Output || direction == Direction.Input;
        }

        public bool SupportsChannelCount(uint channelCount)
        {
            return channelCount == 1 || channelCount == 2 || channelCount == 6;
        }
    }
}