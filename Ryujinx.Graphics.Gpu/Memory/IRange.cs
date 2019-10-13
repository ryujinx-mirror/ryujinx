namespace Ryujinx.Graphics.Gpu.Memory
{
    interface IRange<T>
    {
        ulong Address { get; }
        ulong Size    { get; }

        bool OverlapsWith(ulong address, ulong size);
    }
}