namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Split GPU virtual address.
    /// </summary>
    struct GpuVa
    {
        public uint High;
        public uint Low;

        /// <summary>
        /// Packs the split address into a 64-bits address value.
        /// </summary>
        /// <returns>The 64-bits address value</returns>
        public ulong Pack()
        {
            return Low | ((ulong)High << 32);
        }
    }
}
