using Ryujinx.Common.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Backends.SoundIo.Native
{
    public static partial class SoundIo
    {
        private const string LibraryName = "libsoundio";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnDeviceChangeNativeDelegate(IntPtr ctx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnBackendDisconnectedDelegate(IntPtr ctx, SoundIoError err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnEventsSignalDelegate(IntPtr ctx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EmitRtPrioWarningDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void JackCallbackDelegate(IntPtr msg);

        [StructLayout(LayoutKind.Sequential)]
        public struct SoundIoStruct
        {
            public IntPtr UserData;
            public IntPtr OnDeviceChange;
            public IntPtr OnBackendDisconnected;
            public IntPtr OnEventsSignal;
            public SoundIoBackend CurrentBackend;
            public IntPtr ApplicationName;
            public IntPtr EmitRtPrioWarning;
            public IntPtr JackInfoCallback;
            public IntPtr JackErrorCallback;
        }

        public struct SoundIoChannelLayout
        {
            public IntPtr Name;
            public int ChannelCount;
            public Array24<SoundIoChannelId> Channels;

            public static IntPtr GetDefault(int channelCount)
            {
                return soundio_channel_layout_get_default(channelCount);
            }

            public static unsafe SoundIoChannelLayout GetDefaultValue(int channelCount)
            {
                return Unsafe.AsRef<SoundIoChannelLayout>((SoundIoChannelLayout*)GetDefault(channelCount));
            }
        }

        public struct SoundIoSampleRateRange
        {
            public int Min;
            public int Max;
        }

        public struct SoundIoDevice
        {
            public IntPtr SoundIo;
            public IntPtr Id;
            public IntPtr Name;
            public SoundIoDeviceAim Aim;
            public IntPtr Layouts;
            public int LayoutCount;
            public SoundIoChannelLayout CurrentLayout;
            public IntPtr Formats;
            public int FormatCount;
            public SoundIoFormat CurrentFormat;
            public IntPtr SampleRates;
            public int SampleRateCount;
            public int SampleRateCurrent;
            public double SoftwareLatencyMin;
            public double SoftwareLatencyMax;
            public double SoftwareLatencyCurrent;
            public bool IsRaw;
            public int RefCount;
            public SoundIoError ProbeError;
        }

        public struct SoundIoOutStream
        {
            public IntPtr Device;
            public SoundIoFormat Format;
            public int SampleRate;
            public SoundIoChannelLayout Layout;
            public double SoftwareLatency;
            public float Volume;
            public IntPtr UserData;
            public IntPtr WriteCallback;
            public IntPtr UnderflowCallback;
            public IntPtr ErrorCallback;
            public IntPtr Name;
            public bool NonTerminalHint;
            public int BytesPerFrame;
            public int BytesPerSample;
            public SoundIoError LayoutError;
        }

        public struct SoundIoChannelArea
        {
            public IntPtr Pointer;
            public int Step;
        }

        [LibraryImport(LibraryName)]
        internal static partial IntPtr soundio_create();

        [LibraryImport(LibraryName)]
        internal static partial SoundIoError soundio_connect(IntPtr ctx);

        [LibraryImport(LibraryName)]
        internal static partial void soundio_disconnect(IntPtr ctx);

        [LibraryImport(LibraryName)]
        internal static partial void soundio_flush_events(IntPtr ctx);

        [LibraryImport(LibraryName)]
        internal static partial int soundio_output_device_count(IntPtr ctx);

        [LibraryImport(LibraryName)]
        internal static partial int soundio_default_output_device_index(IntPtr ctx);

        [LibraryImport(LibraryName)]
        internal static partial IntPtr soundio_get_output_device(IntPtr ctx, int index);

        [LibraryImport(LibraryName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool soundio_device_supports_format(IntPtr devCtx, SoundIoFormat format);

        [LibraryImport(LibraryName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool soundio_device_supports_layout(IntPtr devCtx, IntPtr layout);

        [LibraryImport(LibraryName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool soundio_device_supports_sample_rate(IntPtr devCtx, int sampleRate);

        [LibraryImport(LibraryName)]
        internal static partial IntPtr soundio_outstream_create(IntPtr devCtx);

        [LibraryImport(LibraryName)]
        internal static partial SoundIoError soundio_outstream_open(IntPtr outStreamCtx);

        [LibraryImport(LibraryName)]
        internal static partial SoundIoError soundio_outstream_start(IntPtr outStreamCtx);

        [LibraryImport(LibraryName)]
        internal static partial SoundIoError soundio_outstream_begin_write(IntPtr outStreamCtx, IntPtr areas, IntPtr frameCount);

        [LibraryImport(LibraryName)]
        internal static partial SoundIoError soundio_outstream_end_write(IntPtr outStreamCtx);

        [LibraryImport(LibraryName)]
        internal static partial SoundIoError soundio_outstream_pause(IntPtr devCtx, [MarshalAs(UnmanagedType.Bool)] bool pause);

        [LibraryImport(LibraryName)]
        internal static partial SoundIoError soundio_outstream_set_volume(IntPtr devCtx, double volume);

        [LibraryImport(LibraryName)]
        internal static partial void soundio_outstream_destroy(IntPtr streamCtx);

        [LibraryImport(LibraryName)]
        internal static partial void soundio_destroy(IntPtr ctx);

        [LibraryImport(LibraryName)]
        internal static partial IntPtr soundio_channel_layout_get_default(int channelCount);

        [LibraryImport(LibraryName)]
        internal static partial IntPtr soundio_strerror(SoundIoError err);
    }
}
