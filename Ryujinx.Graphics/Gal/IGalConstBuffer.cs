using System;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalConstBuffer
    {
        void LockCache();
        void UnlockCache();

        void Create(long key, long size);

        bool IsCached(long key, long size);

        void SetData(long key, long size, IntPtr hostAddress);
        void SetData(long key, byte[] data);
    }
}