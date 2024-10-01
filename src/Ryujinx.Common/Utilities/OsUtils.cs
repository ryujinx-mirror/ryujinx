using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Utilities
{
    public partial class OsUtils
    {
        [LibraryImport("libc", SetLastError = true)]
        private static partial int setenv([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string value, int overwrite);

        public static void SetEnvironmentVariableNoCaching(string key, string value)
        {
            // Set the value in the cached environment variables, too.
            Environment.SetEnvironmentVariable(key, value);

            if (!OperatingSystem.IsWindows())
            {
                int res = setenv(key, value, 1);
                Debug.Assert(res != -1);
            }
        }
    }
}
