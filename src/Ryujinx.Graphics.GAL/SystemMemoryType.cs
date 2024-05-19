namespace Ryujinx.Graphics.GAL
{
    public enum SystemMemoryType
    {
        /// <summary>
        /// The backend manages the ownership of memory. This mode never supports host imported memory.
        /// </summary>
        BackendManaged,

        /// <summary>
        /// Device memory has similar performance to host memory, usually because it's shared between CPU/GPU.
        /// Use host memory whenever possible.
        /// </summary>
        UnifiedMemory,

        /// <summary>
        /// GPU storage to host memory goes though a slow interconnect, but it would still be preferable to use it if the data is flushed back often.
        /// Assumes constant buffer access to host memory is rather fast.
        /// </summary>
        DedicatedMemory,

        /// <summary>
        /// GPU storage to host memory goes though a slow interconnect, that is very slow when doing access from storage.
        /// When frequently accessed, copy buffers to host memory using DMA.
        /// Assumes constant buffer access to host memory is rather fast.
        /// </summary>
        DedicatedMemorySlowStorage
    }
}
