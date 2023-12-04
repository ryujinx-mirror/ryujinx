using System;

namespace Ryujinx.HLE.Exceptions
{
    class InvalidFirmwarePackageException : Exception
    {
        public InvalidFirmwarePackageException(string message) : base(message) { }
    }
}
