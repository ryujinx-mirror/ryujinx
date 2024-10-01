namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    enum MemoryFillValue : byte
    {
        Zero = 0,
        Stack = 0x58,
        Ipc = 0x59,
        Heap = 0x5A,
    }
}
