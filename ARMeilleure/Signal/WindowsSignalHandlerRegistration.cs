using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Signal
{
    unsafe class WindowsSignalHandlerRegistration
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr AddVectoredExceptionHandler(uint first, IntPtr handler);

        [DllImport("kernel32.dll")]
        private static extern ulong RemoveVectoredExceptionHandler(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        private static IntPtr _getCurrentThreadIdPtr;

        public static IntPtr RegisterExceptionHandler(IntPtr action)
        {
            return AddVectoredExceptionHandler(1, action);
        }

        public static bool RemoveExceptionHandler(IntPtr handle)
        {
            return RemoveVectoredExceptionHandler(handle) != 0;
        }

        public static IntPtr GetCurrentThreadIdFunc()
        {
            if (_getCurrentThreadIdPtr == IntPtr.Zero)
            {
                IntPtr handle = LoadLibrary("kernel32.dll");

                _getCurrentThreadIdPtr = GetProcAddress(handle, "GetCurrentThreadId");
            }

            return _getCurrentThreadIdPtr;
        }
    }
}
