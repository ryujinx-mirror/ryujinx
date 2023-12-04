namespace ARMeilleure.Memory
{
    /// <summary>
    /// Indicates the type of a memory manager and the method it uses for memory mapping
    /// and address translation. This controls the code generated for memory accesses on the JIT.
    /// </summary>
    public enum MemoryManagerType
    {
        /// <summary>
        /// Complete software MMU implementation, the read/write methods are always called,
        /// without any attempt to perform faster memory access.
        /// </summary>
        SoftwareMmu,

        /// <summary>
        /// High level implementation using a software flat page table for address translation,
        /// used to speed up address translation if possible without calling the read/write methods.
        /// </summary>
        SoftwarePageTable,

        /// <summary>
        /// High level implementation with mappings managed by the host OS, effectively using hardware
        /// page tables. No address translation is performed in software and the memory is just accessed directly.
        /// </summary>
        HostMapped,

        /// <summary>
        /// Same as the host mapped memory manager type, but without masking the address within the address space.
        /// Allows invalid access from JIT code to the rest of the program, but is faster.
        /// </summary>
        HostMappedUnsafe,
    }

    static class MemoryManagerTypeExtensions
    {
        public static bool IsHostMapped(this MemoryManagerType type)
        {
            return type == MemoryManagerType.HostMapped || type == MemoryManagerType.HostMappedUnsafe;
        }
    }
}
