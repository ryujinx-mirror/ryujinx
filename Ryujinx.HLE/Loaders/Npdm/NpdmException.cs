using System;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class InvalidNpdmException : Exception
    {
        public InvalidNpdmException(string ExMsg) : base(ExMsg) { }
    }
}
