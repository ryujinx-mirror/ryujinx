using System;

namespace ARMeilleure.Memory
{
    class MemoryProtectionException : Exception
    {
        public MemoryProtectionException(MemoryProtection protection) :  base($"Failed to set memory protection to \"{protection}\".") { }
    }
}