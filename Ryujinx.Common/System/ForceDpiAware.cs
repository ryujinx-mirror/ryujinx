using Ryujinx.Common.Logging;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.System
{
    public static class ForceDpiAware
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static readonly double _standardDpiScale = 96.0;
        private static readonly double _maxScaleFactor   = 1.25;

        /// <summary>
        /// Marks the application as DPI-Aware when running on the Windows operating system.
        /// </summary>
        public static void Windows()
        {
            // Make process DPI aware for proper window sizing on high-res screens.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
        }

        public static double GetWindowScaleFactor()
        {
            double userDpiScale;

            try
            {
                userDpiScale = Graphics.FromHwnd(IntPtr.Zero).DpiX;
            }
            catch (Exception e)
            {
                Logger.Warning?.Print(LogClass.Application, $"Couldn't determine monitor DPI: {e.Message}");
                userDpiScale = 96.0;
            }

            return Math.Min(userDpiScale / _standardDpiScale, _maxScaleFactor);
        }
    }
}
