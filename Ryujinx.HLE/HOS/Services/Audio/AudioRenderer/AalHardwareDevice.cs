using Ryujinx.Audio;
using Ryujinx.Audio.Renderer;
using Ryujinx.Audio.Renderer.Integration;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRenderer
{
    public class AalHardwareDevice : HardwareDevice
    {
        private IAalOutput _output;
        private int _trackId;
        private int _bufferTag;
        private int _nextTag;
        private AutoResetEvent _releaseEvent;

        private uint _channelCount;
        private uint _sampleRate;

        private short[] _buffer;

        private Queue<long> _releasedTags;

        public AalHardwareDevice(int bufferTag, IAalOutput output, uint channelCount, uint sampleRate)
        {
            _bufferTag = bufferTag;
            _channelCount = channelCount;
            _sampleRate = sampleRate;
            _output = output;
            _releaseEvent = new AutoResetEvent(true);
            _trackId = _output.OpenTrack((int)sampleRate, (int)channelCount, AudioCallback);
            _releasedTags = new Queue<long>();

            _buffer = new short[RendererConstants.TargetSampleCount * channelCount];

            _output.Start(_trackId);
        }

        private void AudioCallback()
        {
            long[] released = _output.GetReleasedBuffers(_trackId, int.MaxValue);

            lock (_releasedTags)
            {
                foreach (long tag in released)
                {
                    _releasedTags.Enqueue(tag);
                }
            }
        }

        private long GetReleasedTag()
        {
            lock (_releasedTags)
            {
                if (_releasedTags.Count > 0)
                {
                    return _releasedTags.Dequeue();
                }

                return (_bufferTag << 16) | (_nextTag++);
            }
        }

        public void AppendBuffer(ReadOnlySpan<short> data, uint channelCount)
        {
            data.CopyTo(_buffer.AsSpan());

            _output.AppendBuffer(_trackId, GetReleasedTag(), _buffer);
        }

        public uint GetChannelCount()
        {
            return _channelCount;
        }

        public uint GetSampleRate()
        {
            return _sampleRate;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _output.Stop(_trackId);
                _output.CloseTrack(_trackId);
                _releaseEvent.Dispose();
            }
        }
    }
}
