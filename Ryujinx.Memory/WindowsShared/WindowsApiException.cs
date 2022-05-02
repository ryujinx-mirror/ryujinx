using System;

namespace Ryujinx.Memory.WindowsShared
{
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