using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Signal
{
    unsafe partial class WindowsSignalHandlerRegistration
    {
        [LibraryImport("kernel32.dll")]
        private static partial IntPtr AddVectoredExceptionHandler(uint first, IntPtr handler);

        [LibraryImport("kernel32.dll")]
        private static partial ulong RemoveVectoredExceptionHandler(IntPtr handle);

        [LibraryImport("kernel32.dll", SetLastError = true, EntryPoint = "LoadLibraryA")]
        private static partial IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

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
