using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Dsp;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Backends.CompatLayer
{
    class CompatLayerHardwareDeviceSession : HardwareDeviceSessionOutputBase
    {
        private readonly HardwareDeviceSessionOutputBase _realSession;
        private readonly SampleFormat _userSampleFormat;
        private readonly uint _userChannelCount;

        public CompatLayerHardwareDeviceSession(HardwareDeviceSessionOutputBase realSession, SampleFormat userSampleFormat, uint userChannelCount) : base(realSession.MemoryManager, realSession.RequestedSampleFormat, realSession.RequestedSampleRate, userChannelCount)
        {
            _realSession = realSession;
            _userSampleFormat = userSampleFormat;
            _userChannelCount = userChannelCount;
        }

        public override void Dispose()
        {
            _realSession.Dispose();
        }

        public override ulong GetPlayedSampleCount()
        {
            return _realSession.GetPlayedSampleCount();
        }

        public override float GetVolume()
        {
            return _realSession.GetVolume();
        }

        public override void PrepareToClose()
        {
            _realSession.PrepareToClose();
        }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            SampleFormat realSampleFormat = _realSession.RequestedSampleFormat;

            if (_userSampleFormat != realSampleFormat)
            {
                if (_userSampleFormat != SampleFormat.PcmInt16)
                {
                    throw new NotImplementedException("Converting formats other than PCM16 is not supported.");
                }

                int userSampleCount = buffer.Data.Length / BackendHelper.GetSampleSize(_userSampleFormat);

                ReadOnlySpan<short> samples = MemoryMarshal.Cast<byte, short>(buffer.Data);
                byte[] convertedSamples = new byte[BackendHelper.GetSampleSize(realSampleFormat) * userSampleCount];

                switch (realSampleFormat)
                {
                    case SampleFormat.PcmInt8:
                        PcmHelper.ConvertSampleToPcm8(MemoryMarshal.Cast<byte, sbyte>(convertedSamples), samples);
                        break;
                    case SampleFormat.PcmInt24:
                        PcmHelper.ConvertSampleToPcm24(convertedSamples, samples);
                        break;
                    case SampleFormat.PcmInt32:
                        PcmHelper.ConvertSampleToPcm32(MemoryMarshal.Cast<byte, int>(convertedSamples), samples);
                        break;
                    case SampleFormat.PcmFloat:
                        PcmHelper.ConvertSampleToPcmFloat(MemoryMarshal.Cast<byte, float>(convertedSamples), samples);
                        break;
                    default:
                        throw new NotImplementedException($"Sample format conversion from {_userSampleFormat} to {realSampleFormat} not implemented.");
                }

                buffer.Data = convertedSamples;
            }

            _realSession.QueueBuffer(buffer);
        }

        public override bool RegisterBuffer(AudioBuffer buffer, byte[] samples)
        {
            if (samples == null)
            {
                return false;
            }

            if (_userChannelCount != _realSession.RequestedChannelCount)
            {
                if (_userSampleFormat != SampleFormat.PcmInt16)
                {
                    throw new NotImplementedException("Downmixing formats other than PCM16 is not supported.");
                }

                ReadOnlySpan<short> samplesPCM16 = MemoryMarshal.Cast<byte, short>(samples);

                if (_userChannelCount == 6)
                {
                    samplesPCM16 = Downmixing.DownMixSurroundToStereo(samplesPCM16);

                    if (_realSession.RequestedChannelCount == 1)
                    {
                        samplesPCM16 = Downmixing.DownMixStereoToMono(samplesPCM16);
                    }
                }
                else if (_userChannelCount == 2 && _realSession.RequestedChannelCount == 1)
                {
                    samplesPCM16 = Downmixing.DownMixStereoToMono(samplesPCM16);
                }
                else
                {
                    throw new NotImplementedException($"Downmixing from {_userChannelCount} to {_realSession.RequestedChannelCount} not implemented.");
                }

                samples = MemoryMarshal.Cast<short, byte>(samplesPCM16).ToArray();
            }

            AudioBuffer fakeBuffer = new()
            {
                BufferTag = buffer.BufferTag,
                DataPointer = buffer.DataPointer,
                DataSize = (ulong)samples.Length,
            };

            bool result = _realSession.RegisterBuffer(fakeBuffer, samples);

            if (result)
            {
                buffer.Data = fakeBuffer.Data;
                buffer.DataSize = fakeBuffer.DataSize;
            }

            return result;
        }

        public override void SetVolume(float volume)
        {
            _realSession.SetVolume(volume);
        }

        public override void Start()
        {
            _realSession.Start();
        }

        public override void Stop()
        {
            _realSession.Stop();
        }

        public override void UnregisterBuffer(AudioBuffer buffer)
        {
            _realSession.UnregisterBuffer(buffer);
        }

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            return _realSession.WasBufferFullyConsumed(buffer);
        }
    }
}
