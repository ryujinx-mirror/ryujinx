using Ryujinx.Audio.Backends.SoundIo.Native;
using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Memory;
using System;
using System.Collections.Concurrent;
using System.Threading;
using static Ryujinx.Audio.Backends.SoundIo.Native.SoundIo;
using static Ryujinx.Audio.Integration.IHardwareDeviceDriver;

namespace Ryujinx.Audio.Backends.SoundIo
{
    public class SoundIoHardwareDeviceDriver : IHardwareDeviceDriver
    {
        private readonly SoundIoContext _audioContext;
        private readonly SoundIoDeviceContext _audioDevice;
        private readonly ManualResetEvent _updateRequiredEvent;
        private readonly ManualResetEvent _pauseEvent;
        private readonly ConcurrentDictionary<SoundIoHardwareDeviceSession, byte> _sessions;
        private int _disposeState;

        private float _volume = 1f;

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;

                foreach (SoundIoHardwareDeviceSession session in _sessions.Keys)
                {
                    session.UpdateMasterVolume(value);
                }
            }
        }

        public SoundIoHardwareDeviceDriver()
        {
            _audioContext = SoundIoContext.Create();
            _updateRequiredEvent = new ManualResetEvent(false);
            _pauseEvent = new ManualResetEvent(true);
            _sessions = new ConcurrentDictionary<SoundIoHardwareDeviceSession, byte>();

            _audioContext.Connect();
            _audioContext.FlushEvents();

            _audioDevice = FindValidAudioDevice(_audioContext, true);
        }

        public static bool IsSupported => IsSupportedInternal();

        private static bool IsSupportedInternal()
        {
            SoundIoContext context = null;
            SoundIoDeviceContext device = null;
            SoundIoOutStreamContext stream = null;

            bool backendDisconnected = false;

            try
            {
                context = SoundIoContext.Create();
                context.OnBackendDisconnect = err =>
                {
                    backendDisconnected = true;
                };

                context.Connect();
                context.FlushEvents();

                if (backendDisconnected)
                {
                    return false;
                }

                if (context.OutputDeviceCount == 0)
                {
                    return false;
                }

                device = FindValidAudioDevice(context);

                if (device == null || backendDisconnected)
                {
                    return false;
                }

                stream = device.CreateOutStream();

                if (stream == null || backendDisconnected)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                stream?.Dispose();
                context?.Dispose();
            }
        }

        private static SoundIoDeviceContext FindValidAudioDevice(SoundIoContext audioContext, bool fallback = false)
        {
            SoundIoDeviceContext defaultAudioDevice = audioContext.GetOutputDevice(audioContext.DefaultOutputDeviceIndex);

            if (!defaultAudioDevice.IsRaw)
            {
                return defaultAudioDevice;
            }

            for (int i = 0; i < audioContext.OutputDeviceCount; i++)
            {
                SoundIoDeviceContext audioDevice = audioContext.GetOutputDevice(i);

                if (audioDevice.Id == defaultAudioDevice.Id && !audioDevice.IsRaw)
                {
                    return audioDevice;
                }
            }

            return fallback ? defaultAudioDevice : null;
        }

        public ManualResetEvent GetUpdateRequiredEvent()
        {
            return _updateRequiredEvent;
        }

        public ManualResetEvent GetPauseEvent()
        {
            return _pauseEvent;
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

            if (direction != Direction.Output)
            {
                throw new NotImplementedException("Input direction is currently not implemented on SoundIO backend!");
            }

            SoundIoHardwareDeviceSession session = new(this, memoryManager, sampleFormat, sampleRate, channelCount);

            _sessions.TryAdd(session, 0);

            return session;
        }

        internal bool Unregister(SoundIoHardwareDeviceSession session)
        {
            return _sessions.TryRemove(session, out _);
        }

        public static SoundIoFormat GetSoundIoFormat(SampleFormat format)
        {
            return format switch
            {
                SampleFormat.PcmInt8 => SoundIoFormat.S8,
                SampleFormat.PcmInt16 => SoundIoFormat.S16LE,
                SampleFormat.PcmInt24 => SoundIoFormat.S24LE,
                SampleFormat.PcmInt32 => SoundIoFormat.S32LE,
                SampleFormat.PcmFloat => SoundIoFormat.Float32LE,
                _ => throw new ArgumentException($"Unsupported sample format {format}"),
            };
        }

        internal SoundIoOutStreamContext OpenStream(SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount)
        {
            SoundIoFormat driverSampleFormat = GetSoundIoFormat(requestedSampleFormat);

            if (!_audioDevice.SupportsSampleRate((int)requestedSampleRate))
            {
                throw new ArgumentException($"This sound device does not support a sample rate of {requestedSampleRate}Hz");
            }

            if (!_audioDevice.SupportsFormat(driverSampleFormat))
            {
                throw new ArgumentException($"This sound device does not support {requestedSampleFormat}");
            }

            if (!_audioDevice.SupportsChannelCount((int)requestedChannelCount))
            {
                throw new ArgumentException($"This sound device does not support channel count {requestedChannelCount}");
            }

            SoundIoOutStreamContext result = _audioDevice.CreateOutStream();

            result.Name = "Ryujinx";
            result.Layout = SoundIoChannelLayout.GetDefaultValue((int)requestedChannelCount);
            result.Format = driverSampleFormat;
            result.SampleRate = (int)requestedSampleRate;

            return result;
        }

        internal void FlushContextEvents()
        {
            _audioContext.FlushEvents();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (Interlocked.CompareExchange(ref _disposeState, 1, 0) == 0)
            {
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (SoundIoHardwareDeviceSession session in _sessions.Keys)
                {
                    session.Dispose();
                }

                _audioContext.Disconnect();
                _audioContext.Dispose();
                _pauseEvent.Dispose();
            }
        }

        public bool SupportsSampleRate(uint sampleRate)
        {
            return _audioDevice.SupportsSampleRate((int)sampleRate);
        }

        public bool SupportsSampleFormat(SampleFormat sampleFormat)
        {
            return _audioDevice.SupportsFormat(GetSoundIoFormat(sampleFormat));
        }

        public bool SupportsChannelCount(uint channelCount)
        {
            return _audioDevice.SupportsChannelCount((int)channelCount);
        }

        public bool SupportsDirection(Direction direction)
        {
            // TODO: add direction input when supported.
            return direction == Direction.Output;
        }
    }
}
