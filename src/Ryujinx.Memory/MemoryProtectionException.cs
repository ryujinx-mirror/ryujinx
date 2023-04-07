using System;

namespace Ryujinx.Memory
{
    class MemoryProtectionException : Exception
    {
        public MemoryProtectionException()
        {
        }

        public MemoryProtectionException(MemoryPermission permission) : base($"Failed to set memory protection to \"{permission}\".")
        {
        }

        public MemoryProtectionException(string message) : base(message)
        {
        }

        public MemoryProtectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
