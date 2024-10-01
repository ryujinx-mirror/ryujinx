namespace ARMeilleure.Memory
{
    public interface IJitMemoryAllocator
    {
        IJitMemoryBlock Allocate(ulong size);
        IJitMemoryBlock Reserve(ulong size);
    }
}
