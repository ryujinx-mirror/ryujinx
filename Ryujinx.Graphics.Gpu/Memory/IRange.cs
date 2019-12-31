namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Range of memory.
    /// </summary>
    interface IRange
    {
        ulong Address { get; }
        ulong Size    { get; }

        bool OverlapsWith(ulong address, ulong size);
    }
}