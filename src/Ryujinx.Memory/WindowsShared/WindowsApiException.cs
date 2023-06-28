using System;
using System.Runtime.Versioning;

namespace Ryujinx.Memory.WindowsShared
{
    [SupportedOSPlatform("windows")]
    class WindowsApiException : Exception
    {
        public WindowsApiException()
        {
        }

        public WindowsApiException(string functionName) : base(CreateMessage(functionName))
        {
        }

        public WindowsApiException(string functionName, Exception inner) : base(CreateMessage(functionName), inner)
        {
        }

        private static string CreateMessage(string functionName)
        {
            return $"{functionName} returned error code 0x{WindowsApi.GetLastError():X}.";
        }
    }
}
