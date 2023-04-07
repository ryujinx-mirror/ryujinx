using System;

namespace Ryujinx.Cpu
{
    /// <summary>
    /// Disk cache load state report and management interface.
    /// </summary>
    public interface IDiskCacheLoadState
    {
        /// <summary>
        /// Event used to report the cache load progress.
        /// </summary>
        event Action<LoadState, int, int> StateChanged;

        /// <summary>
        /// Cancels the disk cache load process.
        /// </summary>
        void Cancel();
    }
}
