using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Nvdec.FFmpeg.Native
{
    static class FFmpegApi
    {
        public const string AvCodecLibraryName = "avcodec";
        public const string AvUtilLibraryName = "avutil";

        private static readonly Dictionary<string, (int, int)> _librariesWhitelist = new Dictionary<string, (int, int)>
        {
            { AvCodecLibraryName, (58, 59) },
            { AvUtilLibraryName, (56, 57) }
        };

        private static string FormatLibraryNameForCurrentOs(string libraryName, int version)
        {
            if (OperatingSystem.IsWindows())
            {
                return $"{libraryName}-{version}.dll";
            }
            else if (OperatingSystem.IsLinux())
            {
                return $"lib{libraryName}.so.{version}";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return $"lib{libraryName}.{version}.dylib";
            }
            else
            {
                throw new NotImplementedException($"Unsupported OS for FFmpeg: {RuntimeInformation.RuntimeIdentifier}");
            }
        }


        private static bool TryLoadWhitelistedLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, out IntPtr handle)
        {
            handle = IntPtr.Zero;

            if (_librariesWhitelist.TryGetValue(libraryName, out var value))
            {
                (int minVersion, int maxVersion) = value;

                for (int version = minVersion; version <= maxVersion; version++)
                {
                    if (NativeLibrary.TryLoad(FormatLibraryNameForCurrentOs(libraryName, version), assembly, searchPath, out handle))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static FFmpegApi()
        {
            NativeLibrary.SetDllImportResolver(typeof(FFmpegApi).Assembly, (name, assembly, path) =>
            {
                IntPtr handle;

                if (name == AvUtilLibraryName && TryLoadWhitelistedLibrary(AvUtilLibraryName, assembly, path, out handle))
                {
                    return handle;
                }
                else if (name == AvCodecLibraryName && TryLoadWhitelistedLibrary(AvCodecLibraryName, assembly, path, out handle))
                {
                    return handle;
                }

                return IntPtr.Zero;
            });
        }

        public unsafe delegate void av_log_set_callback_callback(void* a0, AVLog level, [MarshalAs(UnmanagedType.LPUTF8Str)] string a2, byte* a3);

        [DllImport(AvUtilLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern AVFrame* av_frame_alloc();

        [DllImport(AvUtilLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void av_frame_unref(AVFrame* frame);

        [DllImport(AvUtilLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void av_free(AVFrame* frame);

        [DllImport(AvUtilLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void av_log_set_level(AVLog level);

        [DllImport(AvUtilLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void av_log_set_callback(av_log_set_callback_callback callback);

        [DllImport(AvUtilLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern AVLog av_log_get_level();

        [DllImport(AvUtilLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void av_log_format_line(void* ptr, AVLog level, [MarshalAs(UnmanagedType.LPUTF8Str)] string fmt, byte* vl, byte* line, int lineSize, int* printPrefix);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern AVCodec* avcodec_find_decoder(AVCodecID id);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern AVCodecContext* avcodec_alloc_context3(AVCodec* codec);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern int avcodec_open2(AVCodecContext* avctx, AVCodec* codec, void **options);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern int avcodec_close(AVCodecContext* avctx);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void avcodec_free_context(AVCodecContext** avctx);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern AVPacket* av_packet_alloc();

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void av_packet_unref(AVPacket* pkt);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern void av_packet_free(AVPacket** pkt);

        [DllImport(AvCodecLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern int avcodec_version();
    }
}
