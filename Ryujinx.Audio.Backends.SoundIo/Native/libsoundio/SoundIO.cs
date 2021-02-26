using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public class SoundIO : IDisposable
    {
        Pointer<SoundIo> handle;

        public SoundIO()
        {
            handle = Natives.soundio_create();
        }

        internal SoundIO(Pointer<SoundIo> handle)
        {
            this.handle = handle;
        }

        public void Dispose ()
        {
            foreach (var h in allocated_hglobals)
            {
                Marshal.FreeHGlobal(h);
            }

            Natives.soundio_destroy(handle);
        }

        // Equality (based on handle)

        public override bool Equals(object other)
        {
            var d = other as SoundIO;

            return d != null && this.handle == d.handle;
        }

        public override int GetHashCode()
        {
            return (int)(IntPtr)handle;
        }

        public static bool operator == (SoundIO obj1, SoundIO obj2)
        {
            return obj1 is null ? obj2 is null : obj1.Equals(obj2);
        }

        public static bool operator != (SoundIO obj1, SoundIO obj2)
        {
            return obj1 is null ? obj2 is object : !obj1.Equals(obj2);
        }

        // fields

        // FIXME: this should be taken care in more centralized/decent manner... we don't want to write
        // this kind of code anywhere we need string marshaling.
        List<IntPtr> allocated_hglobals = new List<IntPtr>();

        public string ApplicationName {
            get { return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(handle, app_name_offset)); }
            set
            {
                unsafe
                {
                    var existing = Marshal.ReadIntPtr(handle, app_name_offset);
                    if (allocated_hglobals.Contains (existing))
                    {
                        allocated_hglobals.Remove(existing);
                        Marshal.FreeHGlobal(existing);
                    }

                    var ptr = Marshal.StringToHGlobalAnsi(value);
                    Marshal.WriteIntPtr(handle, app_name_offset, ptr);
                    allocated_hglobals.Add(ptr);
                }
            }
        }

        static readonly int app_name_offset = (int)Marshal.OffsetOf<SoundIo>("app_name");

        public SoundIOBackend CurrentBackend
        {
            get { return (SoundIOBackend)Marshal.ReadInt32(handle, current_backend_offset); }
        }

        static readonly int current_backend_offset = (int)Marshal.OffsetOf<SoundIo>("current_backend");

        // emit_rtprio_warning
        public Action EmitRealtimePriorityWarning
        {
            get { return emit_rtprio_warning; }
            set
            {
                emit_rtprio_warning = value;

                var ptr = Marshal.GetFunctionPointerForDelegate(on_devices_change);

                Marshal.WriteIntPtr(handle, emit_rtprio_warning_offset, ptr);
            }
        }

        static readonly int emit_rtprio_warning_offset = (int)Marshal.OffsetOf<SoundIo>("emit_rtprio_warning");

        Action emit_rtprio_warning;

        // jack_error_callback
        public Action<string> JackErrorCallback
        {
            get { return jack_error_callback; }
            set
            {
                jack_error_callback = value;
                if (value == null)
                {
                    jack_error_callback = null;
                }
                else
                {
                    jack_error_callback_native = msg => jack_error_callback(msg);
                }

                var ptr = Marshal.GetFunctionPointerForDelegate(jack_error_callback_native);
                Marshal.WriteIntPtr(handle, jack_error_callback_offset, ptr);
            }
        }

        static readonly int jack_error_callback_offset = (int)Marshal.OffsetOf<SoundIo>("jack_error_callback");

        Action<string> jack_error_callback;
        delegate void jack_error_delegate(string message);
        jack_error_delegate jack_error_callback_native;

        // jack_info_callback
        public Action<string> JackInfoCallback
        {
            get { return jack_info_callback; }
            set
            {
                jack_info_callback = value;
                if (value == null)
                {
                    jack_info_callback = null;
                }
                else
                {
                    jack_info_callback_native = msg => jack_info_callback(msg);
                }

                var ptr = Marshal.GetFunctionPointerForDelegate(jack_info_callback_native);
                Marshal.WriteIntPtr(handle, jack_info_callback_offset, ptr);
            }
        }

        static readonly int jack_info_callback_offset = (int)Marshal.OffsetOf<SoundIo>("jack_info_callback");

        Action<string> jack_info_callback;
        delegate void jack_info_delegate(string message);
        jack_info_delegate jack_info_callback_native;

        // on_backend_disconnect
        public Action<int> OnBackendDisconnect
        {
            get { return on_backend_disconnect; }
            set
            {
                on_backend_disconnect = value;
                if (value == null)
                {
                    on_backend_disconnect_native = null;
                }
                else
                {
                    on_backend_disconnect_native = (sio, err) => on_backend_disconnect(err);
                }

                var ptr = Marshal.GetFunctionPointerForDelegate(on_backend_disconnect_native);
                Marshal.WriteIntPtr(handle, on_backend_disconnect_offset, ptr);
            }
        }

        static readonly int on_backend_disconnect_offset = (int)Marshal.OffsetOf<SoundIo>("on_backend_disconnect");

        Action<int> on_backend_disconnect;
        delegate void on_backend_disconnect_delegate(IntPtr handle, int errorCode);
        on_backend_disconnect_delegate on_backend_disconnect_native;

        // on_devices_change
        public Action OnDevicesChange
        {
            get { return on_devices_change; }
            set
            {
                on_devices_change = value;
                if (value == null)
                {
                    on_devices_change_native = null;
                }
                else
                {
                    on_devices_change_native = sio => on_devices_change();
                }

                var ptr = Marshal.GetFunctionPointerForDelegate(on_devices_change_native);
                Marshal.WriteIntPtr(handle, on_devices_change_offset, ptr);
            }
        }

        static readonly int on_devices_change_offset = (int)Marshal.OffsetOf<SoundIo>("on_devices_change");

        Action on_devices_change;
        delegate void on_devices_change_delegate(IntPtr handle);
        on_devices_change_delegate on_devices_change_native;

        // on_events_signal
        public Action OnEventsSignal
        {
            get { return on_events_signal; }
            set
            {
                on_events_signal = value;
                if (value == null)
                {
                    on_events_signal_native = null;
                }
                else
                {
                    on_events_signal_native = sio => on_events_signal();
                }

                var ptr = Marshal.GetFunctionPointerForDelegate(on_events_signal_native);
                Marshal.WriteIntPtr(handle, on_events_signal_offset, ptr);
            }
        }

        static readonly int on_events_signal_offset = (int)Marshal.OffsetOf<SoundIo>("on_events_signal");

        Action on_events_signal;
        delegate void on_events_signal_delegate(IntPtr handle);
        on_events_signal_delegate on_events_signal_native;


        // functions

        public int BackendCount
        {
            get { return Natives.soundio_backend_count(handle); }
        }

        public int InputDeviceCount
        {
            get { return Natives.soundio_input_device_count(handle); }
        }

        public int OutputDeviceCount
        {
            get { return Natives.soundio_output_device_count(handle); }
        }

        public int DefaultInputDeviceIndex
        {
            get { return Natives.soundio_default_input_device_index(handle); }
        }

        public int DefaultOutputDeviceIndex
        {
            get { return Natives.soundio_default_output_device_index(handle); }
        }

        public SoundIOBackend GetBackend(int index)
        {
            return (SoundIOBackend)Natives.soundio_get_backend(handle, index);
        }

        public SoundIODevice GetInputDevice(int index)
        {
            return new SoundIODevice(Natives.soundio_get_input_device(handle, index));
        }

        public SoundIODevice GetOutputDevice(int index)
        {
            return new SoundIODevice(Natives.soundio_get_output_device(handle, index));
        }

        public void Connect()
        {
            var ret = (SoundIoError)Natives.soundio_connect(handle);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public void ConnectBackend(SoundIOBackend backend)
        {
            var ret = (SoundIoError)Natives.soundio_connect_backend(handle, (SoundIoBackend)backend);
            if (ret != SoundIoError.SoundIoErrorNone)
            {
                throw new SoundIOException(ret);
            }
        }

        public void Disconnect()
        {
            Natives.soundio_disconnect(handle);
        }

        public void FlushEvents()
        {
            Natives.soundio_flush_events(handle);
        }

        public void WaitEvents()
        {
            Natives.soundio_wait_events(handle);
        }

        public void Wakeup()
        {
            Natives.soundio_wakeup(handle);
        }

        public void ForceDeviceScan()
        {
            Natives.soundio_force_device_scan(handle);
        }

        public SoundIORingBuffer CreateRingBuffer(int capacity)
        {
            return new SoundIORingBuffer(Natives.soundio_ring_buffer_create(handle, capacity));
        }

        // static methods

        public static string VersionString
        {
            get { return Marshal.PtrToStringAnsi(Natives.soundio_version_string()); }
        }

        public static int VersionMajor
        {
            get { return Natives.soundio_version_major(); }
        }

        public static int VersionMinor
        {
            get { return Natives.soundio_version_minor(); }
        }

        public static int VersionPatch
        {
            get { return Natives.soundio_version_patch(); }
        }

        public static string GetBackendName(SoundIOBackend backend)
        {
            return Marshal.PtrToStringAnsi(Natives.soundio_backend_name((SoundIoBackend)backend));
        }

        public static bool HaveBackend(SoundIOBackend backend)
        {
            return Natives.soundio_have_backend((SoundIoBackend)backend);
        }

        public static int GetBytesPerSample(SoundIOFormat format)
        {
            return Natives.soundio_get_bytes_per_sample((SoundIoFormat)format);
        }

        public static int GetBytesPerFrame(SoundIOFormat format, int channelCount)
        {
            return Natives.soundio_get_bytes_per_frame((SoundIoFormat)format, channelCount);
        }

        public static int GetBytesPerSecond(SoundIOFormat format, int channelCount, int sampleRate)
        {
            return Natives.soundio_get_bytes_per_second((SoundIoFormat)format, channelCount, sampleRate);
        }

        public static string GetSoundFormatName(SoundIOFormat format)
        {
            return Marshal.PtrToStringAnsi(Natives.soundio_format_string((SoundIoFormat)format));
        }
    }
}