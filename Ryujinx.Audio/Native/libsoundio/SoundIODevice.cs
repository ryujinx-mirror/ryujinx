using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public class SoundIODevice
    {
        public static SoundIOChannelLayout BestMatchingChannelLayout(SoundIODevice device1, SoundIODevice device2)
        {
            var ptr1 = Marshal.ReadIntPtr(device1.handle, layouts_offset);
            var ptr2 = Marshal.ReadIntPtr(device2.handle, layouts_offset);

            return new SoundIOChannelLayout(Natives.soundio_best_matching_channel_layout(ptr1, device1.LayoutCount, ptr2, device2.LayoutCount));
        }

        internal SoundIODevice(Pointer<SoundIoDevice> handle)
        {
            this.handle = handle;
        }

        readonly Pointer<SoundIoDevice> handle;

        // Equality (based on handle and native func)

        public override bool Equals(object other)
        {
            var d = other as SoundIODevice;

            return d != null && (this.handle == d.handle || Natives.soundio_device_equal (this.handle, d.handle));
        }

        public override int GetHashCode()
        {
            return (int)(IntPtr)handle;
        }

        public static bool operator == (SoundIODevice obj1, SoundIODevice obj2)
        {
            return obj1 is null ? obj2 is null : obj1.Equals(obj2);
        }

        public static bool operator != (SoundIODevice obj1, SoundIODevice obj2)
        {
            return obj1 is null ? obj2 is object : !obj1.Equals(obj2);
        }

        // fields

        public SoundIODeviceAim Aim
        {
            get { return (SoundIODeviceAim)Marshal.ReadInt32(handle, aim_offset); }
        }

        static readonly int aim_offset = (int)Marshal.OffsetOf<SoundIoDevice>("aim");

        public SoundIOFormat CurrentFormat
        {
            get { return (SoundIOFormat)Marshal.ReadInt32(handle, current_format_offset); }
        }

        static readonly int current_format_offset = (int)Marshal.OffsetOf<SoundIoDevice>("current_format");

        public SoundIOChannelLayout CurrentLayout
        {
            get { return new SoundIOChannelLayout((IntPtr)handle + current_layout_offset); }
        }

        static readonly int current_layout_offset = (int)Marshal.OffsetOf<SoundIoDevice>("current_layout");

        public int FormatCount
        {
            get { return Marshal.ReadInt32(handle, format_count_offset); }
        }

        static readonly int format_count_offset = (int)Marshal.OffsetOf<SoundIoDevice>("format_count");

        public IEnumerable<SoundIOFormat> Formats
        {
            get
            {
                var ptr = Marshal.ReadIntPtr(handle, formats_offset);

                for (int i = 0; i < FormatCount; i++)
                {
                    yield return (SoundIOFormat)Marshal.ReadInt32(ptr, i);
                }
            }
        }

        static readonly int formats_offset = (int)Marshal.OffsetOf<SoundIoDevice>("formats");

        public string Id
        {
            get { return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(handle, id_offset)); }
        }

        static readonly int id_offset = (int)Marshal.OffsetOf<SoundIoDevice>("id");

        public bool IsRaw
        {
            get { return Marshal.ReadInt32(handle, is_raw_offset) != 0; }
        }

        static readonly int is_raw_offset = (int)Marshal.OffsetOf<SoundIoDevice>("is_raw");

        public int LayoutCount
        {
            get { return Marshal.ReadInt32(handle, layout_count_offset); }
        }

        static readonly int layout_count_offset = (int)Marshal.OffsetOf<SoundIoDevice>("layout_count");

        public IEnumerable<SoundIOChannelLayout> Layouts
        {
            get
            {
                var ptr = Marshal.ReadIntPtr (handle, layouts_offset);

                for (int i = 0; i < LayoutCount; i++)
                {
                    yield return new SoundIOChannelLayout(ptr + i * Marshal.SizeOf<SoundIoChannelLayout>());
                }
            }
        }

        static readonly int layouts_offset = (int)Marshal.OffsetOf<SoundIoDevice>("layouts");

        public string Name
        {
            get { return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(handle, name_offset)); }
        }

        static readonly int name_offset = (int)Marshal.OffsetOf<SoundIoDevice>("name");

        public int ProbeError
        {
            get { return Marshal.ReadInt32(handle, probe_error_offset); }
        }

        static readonly int probe_error_offset = (int)Marshal.OffsetOf<SoundIoDevice>("probe_error");

        public int ReferenceCount
        {
            get { return Marshal.ReadInt32(handle, ref_count_offset); }
        }

        static readonly int ref_count_offset = (int)Marshal.OffsetOf<SoundIoDevice>("ref_count");

        public int SampleRateCount
        {
            get { return Marshal.ReadInt32(handle, sample_rate_count_offset); }
        }

        static readonly int sample_rate_count_offset = (int)Marshal.OffsetOf<SoundIoDevice>("sample_rate_count");

        public IEnumerable<SoundIOSampleRateRange> SampleRates
        {
            get
            {
                var ptr = Marshal.ReadIntPtr(handle, sample_rates_offset);

                for (int i = 0; i < SampleRateCount; i++)
                {
                    yield return new SoundIOSampleRateRange(Marshal.ReadInt32(ptr, i * 2), Marshal.ReadInt32(ptr, i * 2 + 1));
                }
            }
        }

        static readonly int sample_rates_offset = (int)Marshal.OffsetOf<SoundIoDevice>("sample_rates");

        public double SoftwareLatencyCurrent
        {
            get { return MarshalEx.ReadDouble(handle, software_latency_current_offset); }
            set { MarshalEx.WriteDouble(handle, software_latency_current_offset, value); }
        }

        static readonly int software_latency_current_offset = (int)Marshal.OffsetOf<SoundIoDevice>("software_latency_current");

        public double SoftwareLatencyMin
        {
            get { return MarshalEx.ReadDouble(handle, software_latency_min_offset); }
            set { MarshalEx.WriteDouble(handle, software_latency_min_offset, value); }
        }

        static readonly int software_latency_min_offset = (int)Marshal.OffsetOf<SoundIoDevice>("software_latency_min");

        public double SoftwareLatencyMax
        {
            get { return MarshalEx.ReadDouble(handle, software_latency_max_offset); }
            set { MarshalEx.WriteDouble(handle, software_latency_max_offset, value); }
        }

        static readonly int software_latency_max_offset = (int)Marshal.OffsetOf<SoundIoDevice>("software_latency_max");

        public SoundIO SoundIO
        {
            get { return new SoundIO(Marshal.ReadIntPtr(handle, soundio_offset)); }
        }

        static readonly int soundio_offset = (int)Marshal.OffsetOf<SoundIoDevice>("soundio");

        // functions

        public void AddReference()
        {
            Natives.soundio_device_ref(handle);
        }

        public void RemoveReference()
        {
            Natives.soundio_device_unref(handle);
        }

        public void SortDeviceChannelLayouts()
        {
            Natives.soundio_device_sort_channel_layouts(handle);
        }

        public static readonly SoundIOFormat S16NE     =  BitConverter.IsLittleEndian ? SoundIOFormat.S16LE     : SoundIOFormat.S16BE;
        public static readonly SoundIOFormat U16NE     =  BitConverter.IsLittleEndian ? SoundIOFormat.U16LE     : SoundIOFormat.U16BE;
        public static readonly SoundIOFormat S24NE     =  BitConverter.IsLittleEndian ? SoundIOFormat.S24LE     : SoundIOFormat.S24BE;
        public static readonly SoundIOFormat U24NE     =  BitConverter.IsLittleEndian ? SoundIOFormat.U24LE     : SoundIOFormat.U24BE;
        public static readonly SoundIOFormat S32NE     =  BitConverter.IsLittleEndian ? SoundIOFormat.S32LE     : SoundIOFormat.S32BE;
        public static readonly SoundIOFormat U32NE     =  BitConverter.IsLittleEndian ? SoundIOFormat.U32LE     : SoundIOFormat.U32BE;
        public static readonly SoundIOFormat Float32NE =  BitConverter.IsLittleEndian ? SoundIOFormat.Float32LE : SoundIOFormat.Float32BE;
        public static readonly SoundIOFormat Float64NE =  BitConverter.IsLittleEndian ? SoundIOFormat.Float64LE : SoundIOFormat.Float64BE;
        public static readonly SoundIOFormat S16FE     = !BitConverter.IsLittleEndian ? SoundIOFormat.S16LE     : SoundIOFormat.S16BE;
        public static readonly SoundIOFormat U16FE     = !BitConverter.IsLittleEndian ? SoundIOFormat.U16LE     : SoundIOFormat.U16BE;
        public static readonly SoundIOFormat S24FE     = !BitConverter.IsLittleEndian ? SoundIOFormat.S24LE     : SoundIOFormat.S24BE;
        public static readonly SoundIOFormat U24FE     = !BitConverter.IsLittleEndian ? SoundIOFormat.U24LE     : SoundIOFormat.U24BE;
        public static readonly SoundIOFormat S32FE     = !BitConverter.IsLittleEndian ? SoundIOFormat.S32LE     : SoundIOFormat.S32BE;
        public static readonly SoundIOFormat U32FE     = !BitConverter.IsLittleEndian ? SoundIOFormat.U32LE     : SoundIOFormat.U32BE;
        public static readonly SoundIOFormat Float32FE = !BitConverter.IsLittleEndian ? SoundIOFormat.Float32LE : SoundIOFormat.Float32BE;
        public static readonly SoundIOFormat Float64FE = !BitConverter.IsLittleEndian ? SoundIOFormat.Float64LE : SoundIOFormat.Float64BE;

        public bool SupportsFormat(SoundIOFormat format)
        {
            return Natives.soundio_device_supports_format(handle, (SoundIoFormat)format);
        }

        public bool SupportsSampleRate(int sampleRate)
        {
            return Natives.soundio_device_supports_sample_rate(handle, sampleRate);
        }

        public bool SupportsChannelCount(int channelCount)
        {
            return Natives.soundio_device_supports_layout(handle, SoundIOChannelLayout.GetDefault(channelCount).Handle);
        }

        public int GetNearestSampleRate(int sampleRate)
        {
            return Natives.soundio_device_nearest_sample_rate(handle, sampleRate);
        }

        public SoundIOInStream CreateInStream()
        {
            return new SoundIOInStream(Natives.soundio_instream_create(handle));
        }

        public SoundIOOutStream CreateOutStream()
        {
            return new SoundIOOutStream(Natives.soundio_outstream_create(handle));
        }
    }
}