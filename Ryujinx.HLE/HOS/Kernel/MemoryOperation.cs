namespace Ryujinx.HLE.HOS.Kernel
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