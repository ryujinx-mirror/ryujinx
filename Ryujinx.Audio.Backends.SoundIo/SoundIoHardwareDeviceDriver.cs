using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Memory;
using SoundIOSharp;
using System;
using System.Collections.Concurrent;
using System.Threading;

using static Ryujinx.Audio.Integration.IHardwareDeviceDriver;

namespace Ryujinx.Audio.Backends.SoundIo
{
    public class SoundIoHardwareDeviceDriver : IHardwareDeviceDriver
    {
        private readonly SoundIO _audioContext;
        private readonly SoundIODevice _audioDevice;
        private readonly ManualResetEvent _updateRequiredEvent;
        private readonly ManualResetEvent _pauseEvent;
        private readonly ConcurrentDictionary<SoundIoHardwareDeviceSession, byte> _sessions;
        private int _disposeState;

        public SoundIoHardwareDeviceDriver()
        {
            _audioContext = new SoundIO();
            _updateRequiredEvent = new ManualResetEvent(false);
            _pauseEvent = new ManualResetEvent(true);
            _sessions = new ConcurrentDictionary<SoundIoHardwareDeviceSession, byte>();

            _audioContext.Connect();
            _audioContext.FlushEvents();

            _audioDevice = FindNonRawDefaultAudioDevice(_audioContext, true);
        }

        public static bool IsSupported => IsSupportedInternal();

        private static bool IsSupportedInternal()
        {
            SoundIO context = null;
            SoundIODevice device = null;
            SoundIOOutStream stream = null;

            bool backendDisconnected = false;

            try
            {
                context = new SoundIO();

                context.OnBackendDisconnect = (i) =>
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

                device = FindNonRawDefaultAudioDevice(context);

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
                if (stream != null)
                {
                    stream.Dispose();
                }

                if (context != null)
                {
                    context.Dispose();
                }
            }
        }

        private static SoundIODevice FindNonRawDefaultAudioDevice(SoundIO audioContext, bool fallback = false)
        {
            SoundIODevice defaultAudioDevice = audioContext.GetOutputDevice(audioContext.DefaultOutputDeviceIndex);

            if (!defaultAudioDevice.IsRaw)
            {
                return defaultAudioDevice;
            }

            for (int i = 0; i < audioContext.BackendCount; i++)
            {
                SoundIODevice audioDevice = audioContext.GetOutputDevice(i);

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

            SoundIoHardwareDeviceSession session = new SoundIoHardwareDeviceSession(this, memoryManager, sampleFormat, sampleRate, channelCount);

            _sessions.TryAdd(session, 0);

            return session;
        }

        internal bool Unregister(SoundIoHardwareDeviceSession session)
        {
            return _sessions.TryRemove(session, out _);
        }

        public static SoundIOFormat GetSoundIoFormat(SampleFormat format)
        {
            return format switch
            {
                SampleFormat.PcmInt8 => SoundIOFormat.S8,
                SampleFormat.PcmInt16 => SoundIOFormat.S16LE,
                SampleFormat.PcmInt24 => SoundIOFormat.S24LE,
                SampleFormat.PcmInt32 => SoundIOFormat.S32LE,
                SampleFormat.PcmFloat => SoundIOFormat.Float32LE,
                _ => throw new ArgumentException ($"Unsupported sample format {format}"),
            };
        }

        internal SoundIOOutStream OpenStream(SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount)
        {
            SoundIOFormat driverSampleFormat = GetSoundIoFormat(requestedSampleFormat);

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

            SoundIOOutStream result = _audioDevice.CreateOutStream();

            result.Name = "Ryujinx";
            result.Layout = SoundIOChannelLayout.GetDefault((int)requestedChannelCount);
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
