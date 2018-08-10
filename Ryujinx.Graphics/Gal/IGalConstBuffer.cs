using System;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalConstBuffer
    {
        void LockCache();
        void UnlockCache();

        void Create(long Key, long Size);

        bool IsCached(long Key, long Size);

        void SetData(long Key, long Size, IntPtr HostAddress);
    }
}