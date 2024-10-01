using Ryujinx.Audio.Integration;
using Ryujinx.Audio.Renderer.Dsp.Command;
using Ryujinx.Audio.Renderer.Utils;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using System;
using System.Threading;

namespace Ryujinx.Audio.Renderer.Dsp
{
    public class AudioProcessor : IDisposable
    {
        private const int MaxBufferedFrames = 5;
        private const int TargetBufferedFrames = 3;

        private enum MailboxMessage : uint
        {
            Start,
            Stop,
            RenderStart,
            RenderEnd,
        }

        private class RendererSession
        {
            public CommandList CommandList;
            public int RenderingLimit;
            public ulong AppletResourceId;
        }

        private Mailbox<MailboxMessage> _mailbox;
        private RendererSession[] _sessionCommandList;
        private Thread _workerThread;

        public IHardwareDevice[] OutputDevices { get; private set; }

        private long _lastTime;
        private long _playbackEnds;
        private readonly ManualResetEvent _event;

        private ManualResetEvent _pauseEvent;

        public AudioProcessor()
        {
            _event = new ManualResetEvent(false);
        }

        private static uint GetHardwareChannelCount(IHardwareDeviceDriver deviceDriver)
        {
            // Get the real device driver (In case the compat layer is on top of it).
            deviceDriver = deviceDriver.GetRealDeviceDriver();

            if (deviceDriver.SupportsChannelCount(6))
            {
                return 6;
            }

            // NOTE: We default to stereo as this will get downmixed to mono by the compat layer if it's not compatible.
            return 2;
        }

        public void Start(IHardwareDeviceDriver deviceDriver)
        {
            OutputDevices = new IHardwareDevice[Constants.AudioRendererSessionCountMax];

            uint channelCount = GetHardwareChannelCount(deviceDriver);

            for (int i = 0; i < OutputDevices.Length; i++)
            {
                // TODO: Don't hardcode sample rate.
                OutputDevices[i] = new HardwareDeviceImpl(deviceDriver, channelCount, Constants.TargetSampleRate);
            }

            _mailbox = new Mailbox<MailboxMessage>();
            _sessionCommandList = new RendererSession[Constants.AudioRendererSessionCountMax];
            _event.Reset();
            _lastTime = PerformanceCounter.ElapsedNanoseconds;
            _pauseEvent = deviceDriver.GetPauseEvent();

            StartThread();

            _mailbox.SendMessage(MailboxMessage.Start);

            if (_mailbox.ReceiveResponse() != MailboxMessage.Start)
            {
                throw new InvalidOperationException("Audio Processor Start response was invalid!");
            }
        }

        public void Stop()
        {
            _mailbox.SendMessage(MailboxMessage.Stop);

            if (_mailbox.ReceiveResponse() != MailboxMessage.Stop)
            {
                throw new InvalidOperationException("Audio Processor Stop response was invalid!");
            }

            foreach (IHardwareDevice device in OutputDevices)
            {
                device.Dispose();
            }
        }

        public void Send(int sessionId, CommandList commands, int renderingLimit, ulong appletResourceId)
        {
            _sessionCommandList[sessionId] = new RendererSession
            {
                CommandList = commands,
                RenderingLimit = renderingLimit,
                AppletResourceId = appletResourceId,
            };
        }

        public bool HasRemainingCommands(int sessionId)
        {
            return _sessionCommandList[sessionId] != null;
        }

        public void Signal()
        {
            _mailbox.SendMessage(MailboxMessage.RenderStart);
        }

        public void Wait()
        {
            if (_mailbox.ReceiveResponse() != MailboxMessage.RenderEnd)
            {
                throw new InvalidOperationException("Audio Processor Wait response was invalid!");
            }

            long increment = Constants.AudioProcessorMaxUpdateTimeTarget;

            long timeNow = PerformanceCounter.ElapsedNanoseconds;

            if (timeNow > _playbackEnds)
            {
                // Playback has restarted.
                _playbackEnds = timeNow;
            }

            _playbackEnds += increment;

            // The number of frames we are behind where the timer says we should be.
            long framesBehind = (timeNow - _lastTime) / increment;

            // The number of frames yet to play on the backend.
            long bufferedFrames = (_playbackEnds - timeNow) / increment + framesBehind;

            // If we've entered a situation where a lot of buffers will be queued on the backend,
            // Skip some audio frames so that playback can catch up.
            if (bufferedFrames > MaxBufferedFrames)
            {
                // Skip a few frames so that we're not too far behind. (the target number of frames)
                _lastTime += increment * (bufferedFrames - TargetBufferedFrames);
            }

            while (timeNow < _lastTime + increment)
            {
                _event.WaitOne(1);

                timeNow = PerformanceCounter.ElapsedNanoseconds;
            }

            _lastTime += increment;
        }

        private void StartThread()
        {
            _workerThread = new Thread(Work)
            {
                Name = "AudioProcessor.Worker",
            };

            _workerThread.Start();
        }

        private void Work()
        {
            if (_mailbox.ReceiveMessage() != MailboxMessage.Start)
            {
                throw new InvalidOperationException("Audio Processor Start message was invalid!");
            }

            _mailbox.SendResponse(MailboxMessage.Start);
            _mailbox.SendResponse(MailboxMessage.RenderEnd);

            Logger.Info?.Print(LogClass.AudioRenderer, "Starting audio processor");

            while (true)
            {
                _pauseEvent?.WaitOne();

                MailboxMessage message = _mailbox.ReceiveMessage();

                if (message == MailboxMessage.Stop)
                {
                    break;
                }

                if (message == MailboxMessage.RenderStart)
                {
                    long startTicks = PerformanceCounter.ElapsedNanoseconds;

                    for (int i = 0; i < _sessionCommandList.Length; i++)
                    {
                        if (_sessionCommandList[i] != null)
                        {
                            _sessionCommandList[i].CommandList.Process(OutputDevices[i]);
                            _sessionCommandList[i].CommandList.Dispose();
                            _sessionCommandList[i] = null;
                        }
                    }

                    long endTicks = PerformanceCounter.ElapsedNanoseconds;

                    long elapsedTime = endTicks - startTicks;

                    if (Constants.AudioProcessorMaxUpdateTime < elapsedTime)
                    {
                        Logger.Debug?.Print(LogClass.AudioRenderer, $"DSP too slow (exceeded by {elapsedTime - Constants.AudioProcessorMaxUpdateTime}ns)");
                    }

                    _mailbox.SendResponse(MailboxMessage.RenderEnd);
                }
            }

            Logger.Info?.Print(LogClass.AudioRenderer, "Stopping audio processor");
            _mailbox.SendResponse(MailboxMessage.Stop);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _event.Dispose();
                _mailbox?.Dispose();
            }
        }
    }
}
