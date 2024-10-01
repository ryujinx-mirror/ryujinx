using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu.Signal
{
    static partial class WindowsSignalHandlerRegistration
    {
        [LibraryImport("kernel32.dll")]
        private static partial IntPtr AddVectoredExceptionHandler(uint first, IntPtr handler);

        [LibraryImport("kernel32.dll")]
        private static partial ulong RemoveVectoredExceptionHandler(IntPtr handle);

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
