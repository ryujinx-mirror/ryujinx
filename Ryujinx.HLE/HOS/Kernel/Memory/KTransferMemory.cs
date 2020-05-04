using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KTransferMemory : KAutoObject
    {
        public ulong Address { get; private set; }
        public ulong Size    { get; private set; }

        public KTransferMemory(KernelContext context, ulong address, ulong size) : base(context)
        {
            Address = address;
            Size    = size;
        }
    }
}