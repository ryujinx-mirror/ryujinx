using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public class SoundIOInStream : IDisposable
    {
        internal SoundIOInStream(Pointer<SoundIoInStream> handle)
        {
            this.handle = handle;
        }

        Pointer<SoundIoInStream> handle;

        public void Dispose()
        {
            Natives.soundio_instream_destroy(handle);
        }

        // Equality (based on handle)

        public override bool Equals(object other)
        {
            var d = other as SoundIOInStream;

            return d != null && (this.handle == d.handle);
        }

        public override int GetHashCode()
        {
            return (int)(IntPtr)handle;
        }

        public static bool operator == (SoundIOInStream obj1, SoundIOInStream obj2)
        {
            return obj1 is null ? obj2 is null : obj1.Equals(obj2);
        }

        public static bool operator != (SoundIOInStream obj1, SoundIOInStream obj2)
        {
            return obj1 is null ? obj2 is object : !obj1.Equals(obj2);
        }

        // fields

        public SoundIODevice Device
        {
            get { return new SoundIODevice(Marshal.ReadIntPtr(handle, device_offset)); }
        }

        static readonly int device_offset = (int)Marshal.OffsetOf<SoundIoInStream>("device");

        public SoundIOFormat Format
        {
            get { return (SoundIOFormat)Marshal.ReadInt32(handle, format_offset); }
            set { Marshal.WriteInt32(handle, format_offset, (int) value); }
        }

        static readonly int format_offset = (int)Marshal.OffsetOf<SoundIoInStream>("format");

        public int SampleRate
        {
            get { return Marshal.ReadInt32(handle, sample_rate_offset); }
            set { Marshal.WriteInt32(handle, sample_rate_offset, value); }
        }

        static readonly int sample_rate_offset = (int)Marshal.OffsetOf<SoundIoInStream>("sample_rate");

        public SoundIOChannelLayout Layout
        {
            get { return new SoundIOChannelLayout ((IntPtr) handle + layout_offset); }
            set 
            {
                unsafe
                {
                    Buffer.MemoryCopy((void*)((IntPtr)handle + layout_offset), (void*)value.Handle, Marshal.SizeOf<SoundIoChannelLayout>(), Marshal.SizeOf<SoundIoChannelLayout>());
                }
            }
        }

        static readonly int layout_offset = (int)Marshal.OffsetOf<SoundIoInStream>("layout");

        public double SoftwareLatency
        {
            get { return MarshalEx.ReadDouble(handle, software_latency_offset); }
            set { MarshalEx.WriteDouble(handle, software_latency_offset, value); }
        }

        static readonly int software_latency_offset = (int)Marshal.OffsetOf<SoundIoInStream>("software_latency");

        // error_callback
        public Action ErrorCallback
        {
            get { return error_callback; }
            set
            {
                error_callback = value;
                error_callback_native = _ => error_callback();

                var ptr = Marshal.GetFunctionPointerForDelegate(error_callback_native);

                Marshal.WriteIntPtr(handle, error_callback_offset, ptr);
            }
        }

        static readonly int error_callback_offset = (int)Marshal.OffsetOf<SoundIoInStream>("error_callback");

        Action error_callback;
        delegate void error_callback_delegate(IntPtr handle);
        error_callback_delegate error_callback_native;

        // read_callback
        public Action<int,int> ReadCallback
        {
            get { return read_callback; }
            set
            {
                read_callback = value;
                read_callback_native = (_, minFrameCount, maxFrameCount) => read_callback(minFrameCount, maxFrameCount);

                var ptr = Marshal.GetFunctionPointerForDelegate(read_callback_native);

                Marshal.WriteIntPtr(handle, read_callback_offset, ptr);
            }
        }

        static readonly int read_callback_offset = (int)Marshal.OffsetOf<SoundIoInStream>("read_callback");

        Action<int, int> read_callback;
        delegate void read_callback_delegate(IntPtr handle, int min, int max);
        read_callback_delegate read_callback_native;

        // overflow_callback
        public Action OverflowCallback
        {
            get { return overflow_callback; }
            set
            {
                overflow_callback = value;
                overflow_callback_native = _ => overflow_callback();

                var ptr = Marshal.GetFunctionPointerForDelegate(overflow_callback_native);

                Marshal.WriteIntPtr(handle, overflow_callback_offset, ptr);
            }
        }
        static readonly int overflow_callback_offset = (int)Marshal.OffsetOf<SoundIoInStream>("overflow_callback");

        Action overflow_callback;
        delegate void overflow_callback_delegate(IntPtr handle);
        overflow_callback_delegate overflow_callback_native;

        // FIXME: this should be taken care in more centralized/decent manner... we don't want to write
        // this kind of code anywhere we need string marshaling.
        List<IntPtr> allocated_hglobals = new List<IntPtr>();

        public string Name
        {
            get { return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(handle, name_offset)); }
            set
            {
                unsafe
                {
                    var existing = Marshal.ReadIntPtr(handle, name_offset);
                    if (allocated_hglobals.Contains(existing))
                    {
                        allocated_hglobals.Remove(existing);
                        Marshal.FreeHGlobal(existing);
                    }

                    var ptr = Marshal.StringToHGlobalAnsi(value);
                    Marshal.WriteIntPtr(handle, name_offset, ptr);
                    allocated_hglobals.Add(ptr);
                }
            }
        }

        static readonly int name_offset = (int)Marshal.OffsetOf<SoundIoInStream>("name");

        public bool NonTerminalHint
        {
            get { return Marshal.ReadInt32(handle, non_terminal_hint_offset) != 0; }
        }

        static readonly int non_terminal_hint_offset = (int)Marshal.OffsetOf<SoundIoInStream>("non_terminal_hint");

        public int BytesPerFrame
        {
            get { return Marshal.ReadInt32(handle, bytes_per_frame_offset); }
        }

        static readonly int bytes_per_frame_offset = (int)Marshal.OffsetOf<SoundIoInStream>("bytes_per_frame");

        public int BytesPerSample
        {
            get { return Marshal.ReadInt32(handle, bytes_per_sample_offset); }
        }

        static readonly int bytes_per_sample_offset = (int)Marshal.OffsetOf<SoundIoInStream>("bytes_per_sample");

        public string LayoutErrorMessage
        {
            get 
            {
                var code = (SoundIoError)Marshal.ReadInt32(handle, layout_error_offset);

                return code == SoundIoError.SoundIoErrorNone ? null : Marshal.PtrToStringAnsi(Natives.soundio_strerror((int)code));
            }
        }

        static readonly int layout_error_offset = (int)Marshal.OffsetOf<SoundIoInStream>("layout_error");

        // functions

        public void Open()
        {
            var ret = (SoundIoError)Natives.soundio_instream_open(handle);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public void Start()
        {
            var ret = (SoundIoError)Natives.soundio_instream_start(handle);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public SoundIOChannelAreas BeginRead(ref int frameCount)
        {
            IntPtr ptrs             = default;
            int    nativeFrameCount = frameCount;

            unsafe
            {
                var frameCountPtr = &nativeFrameCount;
                var ptrptr        = &ptrs;
                var ret           = (SoundIoError)Natives.soundio_instream_begin_read(handle, (IntPtr)ptrptr, (IntPtr)frameCountPtr);

                frameCount = *frameCountPtr;

                if (ret != SoundIoError.SoundIoErrorNone)
                {
                    throw new SoundIOException(ret);
                }

                return new SoundIOChannelAreas(ptrs, Layout.ChannelCount, frameCount);
            }
        }

        public void EndRead()
        {
            var ret = (SoundIoError)Natives.soundio_instream_end_read(handle);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public void Pause(bool pause)
        {
            var ret = (SoundIoError)Natives.soundio_instream_pause(handle, pause);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public double GetLatency()
        {
            unsafe
            {
                double* dptr = null;
                IntPtr  p    = new IntPtr(dptr);

                var ret = (SoundIoError)Natives.soundio_instream_get_latency(handle, p);
                if (ret != SoundIoError.SoundIoErrorNone)
                {
                    throw new SoundIOException(ret);
                }

                dptr = (double*)p;

                return *dptr;
            }
        }
    }
}
