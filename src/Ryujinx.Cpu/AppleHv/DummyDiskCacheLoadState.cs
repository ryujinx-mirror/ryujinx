using System;

namespace Ryujinx.Cpu.AppleHv
{
    public class DummyDiskCacheLoadState : IDiskCacheLoadState
    {
#pragma warning disable CS0067 // The event is never used
        /// <inheritdoc/>
        public event Action<LoadState, int, int> StateChanged;
#pragma warning restore CS0067

        /// <inheritdoc/>
        public void Cancel()
        {
        }
    }
}
