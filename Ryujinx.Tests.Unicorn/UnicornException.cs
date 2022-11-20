using Ryujinx.Tests.Unicorn.Native.Const;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Tests.Unicorn
{
    public class UnicornException : Exception
    {
        public readonly Error Error;

        internal UnicornException(Error error)
        {
            Error = error;
        }

        public override string Message
        {
            get
            {
                return Marshal.PtrToStringAnsi(Native.Interface.uc_strerror(Error));
            }
        }
    }
}