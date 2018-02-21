using System;

namespace Ryujinx.Core.OsHle.Objects
{
    static class ErrorCode
    {
        public static long MakeError(ErrorModule Module, int Code)
        {
            return (int)Module | (Code << 9);
        }
    }
}