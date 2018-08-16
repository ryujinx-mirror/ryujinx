using System;

namespace Ryujinx.HLE.Exceptions
{
    public class InvalidNpdmException : Exception
    {
        public InvalidNpdmException(string ExMsg) : base(ExMsg) { }
    }
}
