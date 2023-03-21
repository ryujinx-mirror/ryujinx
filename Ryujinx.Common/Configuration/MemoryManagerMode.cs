namespace Ryujinx.Common.Configuration
{
    public enum MemoryManagerMode : byte
    {
        SoftwarePageTable,
        HostMapped,
        HostMappedUnsafe
    }
}
