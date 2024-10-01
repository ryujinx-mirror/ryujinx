using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Backends.SoundIo.Native.SoundIo;

namespace Ryujinx.Audio.Backends.SoundIo.Native
{
    public class SoundIoOutStreamContext : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void WriteCallbackDelegate(IntPtr ctx, int frameCountMin, int frameCountMax);

        private IntPtr _context;
        private IntPtr _nameStored;
        private Action<int, int> _writeCallback;
        private WriteCallbackDelegate _writeCallbackNative;

        public IntPtr Context => _context;

        internal SoundIoOutStreamContext(IntPtr context)
        {
            _context = context;
            _nameStored = IntPtr.Zero;
            _writeCallback = null;
            _writeCallbackNative = null;
        }

        private ref SoundIoOutStream GetOutContext()
        {
            unsafe
            {
                return ref Unsafe.AsRef<SoundIoOutStream>((SoundIoOutStream*)_context);
            }
        }

        public string Name
        {
            get => Marshal.PtrToStringAnsi(GetOutContext().Name);
            set
            {
                var context = GetOutContext();

                if (_nameStored != IntPtr.Zero && context.Name == _nameStored)
                {
                    Marshal.FreeHGlobal(_nameStored);
                }

                _nameStored = Marshal.StringToHGlobalAnsi(value);
                GetOutContext().Name = _nameStored;
            }
        }

        public SoundIoChannelLayout Layout
        {
            get => GetOutContext().Layout;
            set => GetOutContext().Layout = value;
        }

        public SoundIoFormat Format
        {
            get => GetOutContext().Format;
            set => GetOutContext().Format = value;
        }

        public int SampleRate
        {
            get => GetOutContext().SampleRate;
            set => GetOutContext().SampleRate = value;
        }

        public float Volume
        {
            get => GetOutContext().Volume;
            set => GetOutContext().Volume = value;
        }

        public int BytesPerFrame
        {
            get => GetOutContext().BytesPerFrame;
            set => GetOutContext().BytesPerFrame = value;
        }

        public int BytesPerSample
        {
            get => GetOutContext().BytesPerSample;
            set => GetOutContext().BytesPerSample = value;
        }

        public Action<int, int> WriteCallback
        {
            get { return _writeCallback; }
            set
            {
                _writeCallback = value;

                if (_writeCallback == null)
                {
                    _writeCallbackNative = null;
                }
                else
                {
                    _writeCallbackNative = (ctx, frameCountMin, frameCountMax) => _writeCallback(frameCountMin, frameCountMax);
                }

                GetOutContext().WriteCallback = Marshal.GetFunctionPointerForDelegate(_writeCallbackNative);
            }
        }

        private static void CheckError(SoundIoError error)
        {
            if (error != SoundIoError.None)
            {
                throw new SoundIoException(error);
            }
        }

        public void Open() => CheckError(soundio_outstream_open(_context));

        public void Start() => CheckError(soundio_outstream_start(_context));

        public void Pause(bool pause) => CheckError(soundio_outstream_pause(_context, pause));

        public void SetVolume(double volume) => CheckError(soundio_outstream_set_volume(_context, volume));

        public Span<SoundIoChannelArea> BeginWrite(ref int frameCount)
        {
            IntPtr arenas = default;
            int nativeFrameCount = frameCount;

            unsafe
            {
                var frameCountPtr = &nativeFrameCount;
                var arenasPtr = &arenas;
                CheckError(soundio_outstream_begin_write(_context, (IntPtr)arenasPtr, (IntPtr)frameCountPtr));

                frameCount = *frameCountPtr;

                return new Span<SoundIoChannelArea>((void*)arenas, Layout.ChannelCount);
            }
        }

        public void EndWrite() => CheckError(soundio_outstream_end_write(_context));

        protected virtual void Dispose(bool disposing)
        {
            if (_context != IntPtr.Zero)
            {
                soundio_outstream_destroy(_context);
                _context = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SoundIoOutStreamContext()
        {
            Dispose(false);
        }
    }
}
