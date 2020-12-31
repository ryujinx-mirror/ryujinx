//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Ryujinx.Audio.Renderer.Dsp.Command;
using Ryujinx.Audio.Renderer.Integration;
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
            RenderEnd
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
        private HardwareDevice[] _outputDevices;

        private long _lastTime;
        private long _playbackEnds;
        private ManualResetEvent _event;

        public AudioProcessor()
        {
            _event = new ManualResetEvent(false);
        }

        public void SetOutputDevices(HardwareDevice[] outputDevices)
        {
            _outputDevices = outputDevices;
        }

        public void Start()
        {
            _mailbox = new Mailbox<MailboxMessage>();
            _sessionCommandList = new RendererSession[RendererConstants.AudioRendererSessionCountMax];
            _event.Reset();
            _lastTime = PerformanceCounter.ElapsedNanoseconds;

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
        }

        public void Send(int sessionId, CommandList commands, int renderingLimit, ulong appletResourceId)
        {
            _sessionCommandList[sessionId] = new RendererSession
            {
                CommandList = commands,
                RenderingLimit = renderingLimit,
                AppletResourceId = appletResourceId
            };
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

            long increment = RendererConstants.AudioProcessorMaxUpdateTimeTarget;

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
                Name = "AudioProcessor.Worker"
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
                            _sessionCommandList[i].CommandList.Process(_outputDevices[i]);
                            _sessionCommandList[i] = null;
                        }
                    }

                    long endTicks = PerformanceCounter.ElapsedNanoseconds;

                    long elapsedTime = endTicks - startTicks;

                    if (RendererConstants.AudioProcessorMaxUpdateTime < elapsedTime)
                    {
                        Logger.Debug?.Print(LogClass.AudioRenderer, $"DSP too slow (exceeded by {elapsedTime - RendererConstants.AudioProcessorMaxUpdateTime}ns)");
                    }

                    _mailbox.SendResponse(MailboxMessage.RenderEnd);
                }
            }

            Logger.Info?.Print(LogClass.AudioRenderer, "Stopping audio processor");
            _mailbox.SendResponse(MailboxMessage.Stop);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _event.Dispose();
            }
        }
    }
}
