using OpenTK.Audio;
using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using static Ryujinx.Audio.Integration.IHardwareDeviceDriver;

namespace Ryujinx.Audio.Backends.OpenAL
{
    public class OpenALHardwareDeviceDriver : IHardwareDeviceDriver
    {
        private object _lock = new object();

        private AudioContext _context;
        private ManualResetEvent _updateRequiredEvent;
        private List<OpenALHardwareDeviceSession> _sessions;
        private bool _stillRunning;
        private Thread _updaterThread;

        public OpenALHardwareDeviceDriver()
        {
            _context = new AudioContext();
            _updateRequiredEvent = new ManualResetEvent(false);
            _sessions = new List<OpenALHardwareDeviceSession>();

            _stillRunning = true;
            _updaterThread = new Thread(Update)
            {
                Name = "HardwareDeviceDriver.OpenAL"
            };

            _updaterThread.Start();
        }

        public static bool IsSupported
        {
            get
            {
                try
                {
                    return AudioContext.AvailableDevices.Count > 0;
                }
                catch
                {
                    return false;
                }
            }
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
                throw new ArgumentException($"{direction}");
            }
            else if (!SupportsChannelCount(channelCount))
            {
                throw new ArgumentException($"{channelCount}");
            }

            lock (_lock)
            {
                OpenALHardwareDeviceSession session = new OpenALHardwareDeviceSession(this, memoryManager, sampleFormat, sampleRate, channelCount);

                _sessions.Add(session);

                return session;
            }
        }

        internal void Unregister(OpenALHardwareDeviceSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }

        public ManualResetEvent GetUpdateRequiredEvent()
        {
            return _updateRequiredEvent;
        }

        private void Update()
        {
            while (_stillRunning)
            {
                bool updateRequired = false;

                lock (_lock)
                {
                    foreach (OpenALHardwareDeviceSession session in _sessions)
                    {
                        if (session.Update())
                        {
                            updateRequired = true;
                        }
                    }
                }

                if (updateRequired)
                {
                    _updateRequiredEvent.Set();
                }

                // If it's not slept it will waste cycles.
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    _stillRunning = false;
                    _updaterThread.Join();

                    // Loop against all sessions to dispose them (they will unregister themself)
                    while (_sessions.Count > 0)
                    {
                        OpenALHardwareDeviceSession session = _sessions[0];

                        session.Dispose();
                    }
                }

                _context.Dispose();
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

        public bool SupportsChannelCount(uint channelCount)
        {
            return channelCount == 1 || channelCount == 2 || channelCount == 6;
        }

        public bool SupportsDirection(Direction direction)
        {
            return direction == Direction.Output;
        }
    }
}
