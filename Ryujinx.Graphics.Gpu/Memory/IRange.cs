namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Range of memory.
    /// </summary>
    /// <typeparam name="T">GPU resource type</typeparam>
    interface IRange<T>
    {
        ulong Address { get; }
        ulong Size    { get; }

        bool OverlapsWith(ulong address, ulong size);
    }
}