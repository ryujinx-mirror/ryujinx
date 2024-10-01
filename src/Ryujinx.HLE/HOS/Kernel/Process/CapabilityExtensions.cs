using System.Numerics;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    static class CapabilityExtensions
    {
        public static CapabilityType GetCapabilityType(this uint cap)
        {
            return (CapabilityType)(((cap + 1) & ~cap) - 1);
        }

        public static uint GetFlag(this CapabilityType type)
        {
            return (uint)type + 1;
        }

        public static uint GetId(this CapabilityType type)
        {
            return (uint)BitOperations.TrailingZeroCount(type.GetFlag());
        }
    }
}
