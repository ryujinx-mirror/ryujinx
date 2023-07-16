namespace Ryujinx.HLE.HOS.Kernel.Common
{
    enum LimitableResource : byte
    {
        Memory = 0,
        Thread = 1,
        Event = 2,
        TransferMemory = 3,
        Session = 4,

        Count = 5,
    }
}
