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
        private readonly IHardwareDeviceDriver _realDriver;

        public static bool IsSupported => true;

        public float Volume
        {
            get => _realDriver.Volume;
            set => _realDriver.Volume = value;
        }

        public CompatLayerHardwareDeviceDriver(IHardwareDeviceDriver realDevice)
        {
            _realDriver = realDevice;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
                _ => throw new ArgumentException($"Invalid targetChannelCount {targetChannelCount}"),
            };
        }

        private SampleFormat SelectHardwareSampleFormat(SampleFormat targetSampleFormat)
        {
            if (_realDriver.SupportsSampleFormat(targetSampleFormat))
            {
                return targetSampleFormat;
            }

            // Attempt conversion from PCM16.
            if (targetSampleFormat == SampleFormat.PcmInt16)
            {
                // Prefer PCM32 if we need to convert.
                if (_realDriver.SupportsSampleFormat(SampleFormat.PcmInt32))
                {
                    return SampleFormat.PcmInt32;
                }

                // If not supported, PCM float provides the best quality with a cost lower than PCM24.
                if (_realDriver.SupportsSampleFormat(SampleFormat.PcmFloat))
                {
                    return SampleFormat.PcmFloat;
                }

                if (_realDriver.SupportsSampleFormat(SampleFormat.PcmInt24))
                {
                    return SampleFormat.PcmInt24;
                }

                // If nothing is truly supported, attempt PCM8 at the cost of losing quality.
                if (_realDriver.SupportsSampleFormat(SampleFormat.PcmInt8))
                {
                    return SampleFormat.PcmInt8;
                }
            }

            throw new ArgumentException("No valid sample format configuration found!");
        }

        public IHardwareDeviceSession OpenDeviceSession(Direction direction, IVirtualMemoryManager memoryManager, SampleFormat sampleFormat, uint sampleRate, uint channelCount)
        {
            if (channelCount == 0)
            {
                channelCount = 2;
            }

            if (sampleRate == 0)
            {
                sampleRate = Constants.TargetSampleRate;
            }

            if (!_realDriver.SupportsDirection(direction))
            {
                if (direction == Direction.Input)
                {
                    Logger.Warning?.Print(LogClass.Audio, "The selected audio backend doesn't support audio input, fallback to dummy...");

                    return new DummyHardwareDeviceSessionInput(this, memoryManager);
                }

                throw new NotImplementedException();
            }

            SampleFormat hardwareSampleFormat = SelectHardwareSampleFormat(sampleFormat);
            uint hardwareChannelCount = SelectHardwareChannelCount(channelCount);

            IHardwareDeviceSession realSession = _realDriver.OpenDeviceSession(direction, memoryManager, hardwareSampleFormat, sampleRate, hardwareChannelCount);

            if (hardwareChannelCount == channelCount && hardwareSampleFormat == sampleFormat)
            {
                return realSession;
            }

            if (hardwareSampleFormat != sampleFormat)
            {
                Logger.Warning?.Print(LogClass.Audio, $"{sampleFormat} isn't supported by the audio device, conversion to {hardwareSampleFormat} will happen.");

                if (hardwareSampleFormat < sampleFormat)
                {
                    Logger.Warning?.Print(LogClass.Audio, $"{hardwareSampleFormat} has lower quality than {sampleFormat}, expect some loss in audio fidelity.");
                }
            }

            if (direction == Direction.Input)
            {
                Logger.Warning?.Print(LogClass.Audio, "The selected audio backend doesn't support the requested audio input configuration, fallback to dummy...");

                // TODO: We currently don't support audio input upsampling/downsampling, implement this.
                realSession.Dispose();

                return new DummyHardwareDeviceSessionInput(this, memoryManager);
            }

            // It must be a HardwareDeviceSessionOutputBase.
            if (realSession is not HardwareDeviceSessionOutputBase realSessionOutputBase)
            {
                throw new InvalidOperationException($"Real driver session class type isn't based on {typeof(HardwareDeviceSessionOutputBase).Name}.");
            }

            // If we need to do post processing before sending to the hardware device, wrap around it.
            return new CompatLayerHardwareDeviceSession(realSessionOutputBase, sampleFormat, channelCount);
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
