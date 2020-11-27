using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public class SoundIOOutStream : IDisposable
    {
        internal SoundIOOutStream (Pointer<SoundIoOutStream> handle)
        {
            this.handle = handle;
        }

        Pointer<SoundIoOutStream> handle;

        public void Dispose ()
        {
            Natives.soundio_outstream_destroy (handle);
        }
        // Equality (based on handle)

        public override bool Equals (object other)
        {
            var d = other as SoundIOOutStream;

            return d != null && (this.handle == d.handle);
        }

        public override int GetHashCode ()
        {
            return (int)(IntPtr)handle;
        }

        public static bool operator == (SoundIOOutStream obj1, SoundIOOutStream obj2)
        {
            return obj1 is null ? obj2 is null : obj1.Equals(obj2);
        }

        public static bool operator != (SoundIOOutStream obj1, SoundIOOutStream obj2)
        {
            return obj1 is null ? obj2 is object : !obj1.Equals(obj2);
        }

        // fields

        public SoundIODevice Device
        {
            get { return new SoundIODevice(Marshal.ReadIntPtr(handle, device_offset)); }
        }

        static readonly int device_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("device");

        public SoundIOFormat Format
        {
            get { return (SoundIOFormat) Marshal.ReadInt32(handle, format_offset); }
            set { Marshal.WriteInt32(handle, format_offset, (int) value); }
        }

        static readonly int format_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("format");

        public int SampleRate
        {
            get { return Marshal.ReadInt32(handle, sample_rate_offset); }
            set { Marshal.WriteInt32(handle, sample_rate_offset, value); }
        }

        static readonly int sample_rate_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("sample_rate");

        public SoundIOChannelLayout Layout
        {
            get { unsafe { return new SoundIOChannelLayout((IntPtr) (void*)((IntPtr)handle + layout_offset)); } }
            set
            {
                unsafe
                {
                    Buffer.MemoryCopy((void*)value.Handle, (void*)((IntPtr)handle + layout_offset), Marshal.SizeOf<SoundIoChannelLayout>(), Marshal.SizeOf<SoundIoChannelLayout>());
                }
            }
        }
        static readonly int layout_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("layout");

        public double SoftwareLatency
        {
            get { return MarshalEx.ReadDouble (handle, software_latency_offset); }
            set { MarshalEx.WriteDouble (handle, software_latency_offset, value); }
        }

        static readonly int software_latency_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("software_latency");

        public float Volume
        {
            get { return MarshalEx.ReadFloat(handle, volume_offset); }
            set { MarshalEx.WriteFloat(handle, volume_offset, value); }
        }

        static readonly int volume_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("volume");

        // error_callback
        public Action ErrorCallback
        {
            get { return error_callback; }
            set
            {
                error_callback = value;
                if (value == null)
                {
                    error_callback_native = null;
                }
                else
                {
                    error_callback_native = stream => error_callback();
                }

                var ptr = Marshal.GetFunctionPointerForDelegate(error_callback_native);
                Marshal.WriteIntPtr(handle, error_callback_offset, ptr);
            }
        }

        static readonly int error_callback_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("error_callback");

        Action error_callback;
        delegate void error_callback_delegate (IntPtr handle);
        error_callback_delegate error_callback_native;

        // write_callback
        public Action<int, int> WriteCallback
        {
            get { return write_callback; }
            set
            {
                write_callback = value;
                if (value == null)
                {
                    write_callback_native = null;
                }
                else
                {
                    write_callback_native = (h, frame_count_min, frame_count_max) => write_callback(frame_count_min, frame_count_max);
                }

                var ptr = Marshal.GetFunctionPointerForDelegate (write_callback_native);
                Marshal.WriteIntPtr (handle, write_callback_offset, ptr);
            }
        }

        static readonly int write_callback_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("write_callback");

        Action<int, int> write_callback;
        delegate void write_callback_delegate(IntPtr handle, int min, int max);
        write_callback_delegate write_callback_native;

        // underflow_callback
        public Action UnderflowCallback
        {
            get { return underflow_callback; }
            set
            {
                underflow_callback = value;
                if (value == null)
                {
                    underflow_callback_native = null;
                }
                else
                {
                    underflow_callback_native = h => underflow_callback();
                }

                var ptr = Marshal.GetFunctionPointerForDelegate (underflow_callback_native);
                Marshal.WriteIntPtr (handle, underflow_callback_offset, ptr);
            }
        }

        static readonly int underflow_callback_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("underflow_callback");
        
        Action underflow_callback;
        delegate void underflow_callback_delegate(IntPtr handle);
        underflow_callback_delegate underflow_callback_native;

        // FIXME: this should be taken care in more centralized/decent manner... we don't want to write
        // this kind of code anywhere we need string marshaling.
        List<IntPtr> allocated_hglobals = new List<IntPtr>();

        public string Name {
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

        static readonly int name_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("name");

        public bool NonTerminalHint
        {
            get { return Marshal.ReadInt32(handle, non_terminal_hint_offset) != 0; }
        }

        static readonly int non_terminal_hint_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("non_terminal_hint");

        public int BytesPerFrame
        {
            get { return Marshal.ReadInt32(handle, bytes_per_frame_offset); }
        }

        static readonly int bytes_per_frame_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("bytes_per_frame");

        public int BytesPerSample
        {
            get { return Marshal.ReadInt32(handle, bytes_per_sample_offset); }
        }

        static readonly int bytes_per_sample_offset = (int)Marshal.OffsetOf<SoundIoOutStream>("bytes_per_sample");

        public string LayoutErrorMessage
        {
            get
            {
                var code = (SoundIoError)Marshal.ReadInt32(handle, layout_error_offset);

                return code == SoundIoError.SoundIoErrorNone ? null : Marshal.PtrToStringAnsi(Natives.soundio_strerror((int)code));
            }
        }

        static readonly int layout_error_offset = (int)Marshal.OffsetOf<SoundIoOutStream> ("layout_error");

        // functions

        public void Open ()
        {
            var ret = (SoundIoError)Natives.soundio_outstream_open(handle);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public void Start ()
        {
            var ret = (SoundIoError)Natives.soundio_outstream_start(handle);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public SoundIOChannelAreas BeginWrite(ref int frameCount)
        {
            IntPtr ptrs             = default;
            int    nativeFrameCount = frameCount;

            unsafe
            {
                var frameCountPtr = &nativeFrameCount;
                var ptrptr        = &ptrs;
                var ret           = (SoundIoError)Natives.soundio_outstream_begin_write(handle, (IntPtr)ptrptr, (IntPtr)frameCountPtr);

                frameCount = *frameCountPtr;

                if (ret != SoundIoError.SoundIoErrorNone)
                {
                    throw new SoundIOException(ret);
                }

                return new SoundIOChannelAreas(ptrs, Layout.ChannelCount, frameCount);
            }
        }

        public void EndWrite ()
        {
            var ret = (SoundIoError)Natives.soundio_outstream_end_write(handle);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public void ClearBuffer ()
        {
            _ = Natives.soundio_outstream_clear_buffer(handle);
        }

        public void Pause (bool pause)
        {
            var ret = (SoundIoError)Natives.soundio_outstream_pause(handle, pause);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public double GetLatency ()
        {
            unsafe
            {
                double* dptr = null;
                IntPtr  p    = new IntPtr(dptr);

                var ret = (SoundIoError)Natives.soundio_outstream_get_latency(handle, p);
                if (ret != SoundIoError.SoundIoErrorNone)
                {
                    throw new SoundIOException(ret);
                }

                dptr = (double*)p;

                return *dptr;
            }
        }

        public void SetVolume (double volume)
        {
            var ret = (SoundIoError)Natives.soundio_outstream_set_volume(handle, volume);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }
    }
}
