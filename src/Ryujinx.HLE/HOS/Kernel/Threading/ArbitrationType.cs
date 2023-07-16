namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    enum ArbitrationType
    {
        WaitIfLessThan = 0,
        DecrementAndWaitIfLessThan = 1,
        WaitIfEqual = 2,
    }
}
