using Ryujinx.Audio.Common;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Integration
{
    public class HardwareDeviceImpl : IHardwareDevice
    {
        private IHardwareDeviceSession _session;
        private uint _channelCount;
        private uint _sampleRate;
        private uint _currentBufferTag;

        private byte[] _buffer;

        public HardwareDeviceImpl(IHardwareDeviceDriver deviceDriver, uint channelCount, uint sampleRate, float volume)
        {
            _session = deviceDriver.OpenDeviceSession(IHardwareDeviceDriver.Direction.Output, null, SampleFormat.PcmInt16, sampleRate, channelCount, volume);
            _channelCount = channelCount;
            _sampleRate = sampleRate;
            _currentBufferTag = 0;

            _buffer = new byte[Constants.TargetSampleCount * channelCount * sizeof(ushort)];

            _session.Start();
        }

        public void AppendBuffer(ReadOnlySpan<short> data, uint channelCount)
        {
            data.CopyTo(MemoryMarshal.Cast<byte, short>(_buffer));

            _session.QueueBuffer(new AudioBuffer
            {
                DataPointer = _currentBufferTag++,
                Data = _buffer,
                DataSize = (ulong)_buffer.Length,
            });

            _currentBufferTag = _currentBufferTag % 4;
        }

        public void SetVolume(float volume)
        {
            _session.SetVolume(volume);
        }

        public float GetVolume()
        {
            return _session.GetVolume();
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
                _session.Dispose();
            }
        }
    }
}