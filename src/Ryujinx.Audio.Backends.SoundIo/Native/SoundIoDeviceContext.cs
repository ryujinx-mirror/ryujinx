using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Backends.SoundIo.Native.SoundIo;

namespace Ryujinx.Audio.Backends.SoundIo.Native
{
    public class SoundIoDeviceContext
    {
        private readonly IntPtr _context;

        public IntPtr Context => _context;

        internal SoundIoDeviceContext(IntPtr context)
        {
            _context = context;
        }

        private ref SoundIoDevice GetDeviceContext()
        {
            unsafe
            {
                return ref Unsafe.AsRef<SoundIoDevice>((SoundIoDevice*)_context);
            }
        }

        public bool IsRaw => GetDeviceContext().IsRaw;

        public string Id => Marshal.PtrToStringAnsi(GetDeviceContext().Id);

        public bool SupportsSampleRate(int sampleRate) => soundio_device_supports_sample_rate(_context, sampleRate);

        public bool SupportsFormat(SoundIoFormat format) => soundio_device_supports_format(_context, format);

        public bool SupportsChannelCount(int channelCount) => soundio_device_supports_layout(_context, SoundIoChannelLayout.GetDefault(channelCount));

        public SoundIoOutStreamContext CreateOutStream()
        {
            IntPtr context = soundio_outstream_create(_context);

            if (context == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIoOutStreamContext(context);
        }
    }
}
