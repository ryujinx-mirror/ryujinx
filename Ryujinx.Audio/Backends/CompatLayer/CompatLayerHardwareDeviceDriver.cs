using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Backends.Dummy;
using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Common.Logging;
using Ryujinx.Memory;
using System;
using System.Threading;

using static Ryujinx.Audio.Integration.IHardwareDeviceDriver;

namespace Ryujinx.Audio.Backends.CompatLayer
{
    public class CompatLayerHardwareDeviceDriver : IHardwareDeviceDriver
    {
        private IHardwareDeviceDriver _realDriver;

        public CompatLayerHardwareDeviceDriver(IHardwareDeviceDriver realDevice)
        {
            _realDriver = realDevice;
        }

        public void Dispose()
        {
            _realDriver.Dispose();
        }

        public ManualResetEvent GetUpdateRequiredEvent()
        {
            return _realDriver.GetUpdateRequiredEvent();
        }

        public ManualResetEvent GetPauseEvent()
        {
            return _realDriver.GetPauseEvent();
        }

        private uint SelectHardwareChannelCount(uint targetChannelCount)
        {
            if (_realDriver.SupportsChannelCount(targetChannelCount))
            {
                return targetChannelCount;
            }

            return targetChannelCount switch
            {
                6 => SelectHardwareChannelCount(2),
                2 => SelectHardwareChannelCount(1),
                1 => throw new ArgumentException("No valid channel configuration found!"),
                _ => throw new ArgumentException($"Invalid targetChannelCount {targetChannelCount}")
            };
        }

        public IHardwareDeviceSession OpenDeviceSession(Direction direction, IVirtualMemoryManager memoryManager, SampleFormat sampleFormat, uint sampleRate, uint channelCount, float volume)
        {
            if (channelCount == 0)
            {
                channelCount = 2;
            }

            if (sampleRate == 0)
            {
                sampleRate = Constants.TargetSampleRate;
            }

            volume = Math.Clamp(volume, 0, 1);

            if (!_realDriver.SupportsDirection(direction))
            {
                if (direction == Direction.Input)
                {
                    Logger.Warning?.Print(LogClass.Audio, "The selected audio backend doesn't support audio input, fallback to dummy...");

                    return new DummyHardwareDeviceSessionInput(this, memoryManager, sampleFormat, sampleRate, channelCount);
                }

                throw new NotImplementedException();
            }

            uint hardwareChannelCount = SelectHardwareChannelCount(channelCount);

            IHardwareDeviceSession realSession = _realDriver.OpenDeviceSession(direction, memoryManager, sampleFormat, sampleRate, hardwareChannelCount, volume);

            if (hardwareChannelCount == channelCount)
            {
                return realSession;
            }

            if (direction == Direction.Input)
            {
                Logger.Warning?.Print(LogClass.Audio, $"The selected audio backend doesn't support the requested audio input configuration, fallback to dummy...");

                // TODO: We currently don't support audio input upsampling/downsampling, implement this.
                realSession.Dispose();

                return new DummyHardwareDeviceSessionInput(this, memoryManager, sampleFormat, sampleRate, channelCount);
            }

            // It must be a HardwareDeviceSessionOutputBase.
            if (realSession is not HardwareDeviceSessionOutputBase realSessionOutputBase)
            {
                throw new InvalidOperationException($"Real driver session class type isn't based on {typeof(HardwareDeviceSessionOutputBase).Name}.");
            }

            // If we need to do post processing before sending to the hardware device, wrap around it.
            return new CompatLayerHardwareDeviceSession(realSessionOutputBase, channelCount);
        }

        public bool SupportsChannelCount(uint channelCount)
        {
            return channelCount == 1 || channelCount == 2 || channelCount == 6;
        }

        public bool SupportsSampleFormat(SampleFormat sampleFormat)
        {
            // TODO: More formats.
            return sampleFormat == SampleFormat.PcmInt16;
        }

        public bool SupportsSampleRate(uint sampleRate)
        {
            // TODO: More sample rates.
            return sampleRate == Constants.TargetSampleRate;
        }

        public IHardwareDeviceDriver GetRealDeviceDriver()
        {
            return _realDriver;
        }

        public bool SupportsDirection(Direction direction)
        {
            return direction == Direction.Input || direction == Direction.Output;
        }
    }
}
