using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KTransferMemory : KAutoObject
    {
        public ulong Address { get; private set; }
        public ulong Size    { get; private set; }

        public KTransferMemory(Horizon system, ulong address, ulong size) : base(system)
        {
            Address = address;
            Size    = size;
        }
    }
}