using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IBuffer : IDisposable
    {
        void CopyTo(IBuffer destination, int srcOffset, int dstOffset, int size);

        byte[] GetData(int offset, int size);

        void SetData(ReadOnlySpan<byte> data);

        void SetData(int offset, ReadOnlySpan<byte> data);
    }
}