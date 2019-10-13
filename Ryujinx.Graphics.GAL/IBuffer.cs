using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IBuffer : IDisposable
    {
        void CopyTo(IBuffer destination, int srcOffset, int dstOffset, int size);

        byte[] GetData(int offset, int size);

        void SetData(Span<byte> data);

        void SetData(int offset, Span<byte> data);
    }
}