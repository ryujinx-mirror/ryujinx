namespace Ryujinx.Graphics.Gpu.Memory
{
    public class UnmapEventArgs
    {
        public ulong Address { get; }
        public ulong Size { get; }

        public UnmapEventArgs(ulong address, ulong size)
        {
            Address = address;
            Size = size;
        }
    }
}
