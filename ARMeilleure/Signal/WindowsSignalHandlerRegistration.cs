using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Signal
{
    class WindowsSignalHandlerRegistration
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr AddVectoredExceptionHandler(uint first, IntPtr handler);

        [DllImport("kernel32.dll")]
        private static extern ulong RemoveVectoredExceptionHandler(IntPtr handle);

        public static IntPtr RegisterExceptionHandler(IntPtr action)
        {
            return AddVectoredExceptionHandler(1, action);
        }

        public static bool RemoveExceptionHandler(IntPtr handle)
        {
            return RemoveVectoredExceptionHandler(handle) != 0;
        }
    }
}
