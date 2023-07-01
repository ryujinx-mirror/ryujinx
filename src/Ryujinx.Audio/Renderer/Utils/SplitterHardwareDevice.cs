using Ryujinx.Audio.Integration;
using System;

namespace Ryujinx.Audio.Renderer.Utils
{
    public class SplitterHardwareDevice : IHardwareDevice
    {
        private readonly IHardwareDevice _baseDevice;
        private readonly IHardwareDevice _secondaryDevice;

        public SplitterHardwareDevice(IHardwareDevice baseDevice, IHardwareDevice secondaryDevice)
        {
            _baseDevice = baseDevice;
            _secondaryDevice = secondaryDevice;
        }

        public void AppendBuffer(ReadOnlySpan<short> data, uint channelCount)
        {
            _baseDevice.AppendBuffer(data, channelCount);
            _secondaryDevice?.AppendBuffer(data, channelCount);
        }

        public void SetVolume(float volume)
        {
            _baseDevice.SetVolume(volume);
            _secondaryDevice.SetVolume(volume);
        }

        public float GetVolume()
        {
            return _baseDevice.GetVolume();
        }

        public uint GetChannelCount()
        {
            return _baseDevice.GetChannelCount();
        }

        public uint GetSampleRate()
        {
            return _baseDevice.GetSampleRate();
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
                _baseDevice.Dispose();
                _secondaryDevice?.Dispose();
            }
        }
    }
}
