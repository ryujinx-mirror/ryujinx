namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    enum MemoryOperation
    {
        MapPa,
        MapVa,
        Allocate,
        Unmap,
        ChangePermRw,
        ChangePermsAndAttributes
    }
}