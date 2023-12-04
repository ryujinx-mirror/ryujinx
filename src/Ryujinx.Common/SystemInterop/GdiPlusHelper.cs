using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.Common.SystemInterop
{
    [SupportedOSPlatform("windows")]
    public static partial class GdiPlusHelper
    {
        private const string LibraryName = "gdiplus.dll";

        private static readonly IntPtr _initToken;

        static GdiPlusHelper()
        {
            CheckStatus(GdiplusStartup(out _initToken, StartupInputEx.Default, out _));
        }

        private static void CheckStatus(int gdiStatus)
        {
            if (gdiStatus != 0)
            {
                throw new Exception($"GDI Status Error: {gdiStatus}");
            }
        }

        private struct StartupInputEx
        {
            public int GdiplusVersion;

#pragma warning disable CS0649 // Field is never assigned to
            public IntPtr DebugEventCallback;
            public int SuppressBackgroundThread;
            public int SuppressExternalCodecs;
            public int StartupParameters;
#pragma warning restore CS0649

            public static StartupInputEx Default => new()
            {
                // We assume Windows 8 and upper
                GdiplusVersion = 2,
                DebugEventCallback = IntPtr.Zero,
                SuppressBackgroundThread = 0,
                SuppressExternalCodecs = 0,
                StartupParameters = 0,
            };
        }

        private struct StartupOutput
        {
            public IntPtr NotificationHook;
            public IntPtr NotificationUnhook;
        }

        [LibraryImport(LibraryName)]
        private static partial int GdiplusStartup(out IntPtr token, in StartupInputEx input, out StartupOutput output);

        [LibraryImport(LibraryName)]
        private static partial int GdipCreateFromHWND(IntPtr hwnd, out IntPtr graphics);

        [LibraryImport(LibraryName)]
        private static partial int GdipDeleteGraphics(IntPtr graphics);

        [LibraryImport(LibraryName)]
        private static partial int GdipGetDpiX(IntPtr graphics, out float dpi);

        public static float GetDpiX(IntPtr hwnd)
        {
            CheckStatus(GdipCreateFromHWND(hwnd, out IntPtr graphicsHandle));
            CheckStatus(GdipGetDpiX(graphicsHandle, out float result));
            CheckStatus(GdipDeleteGraphics(graphicsHandle));

            return result;
        }
    }
}
