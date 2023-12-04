using System;

namespace Ryujinx.Graphics.Gpu.Synchronization
{
    /// <summary>
    /// Modifier flags for creating host sync.
    /// </summary>
    [Flags]
    internal enum HostSyncFlags
    {
        None = 0,

        /// <summary>
        /// Present if host sync is being created by a syncpoint.
        /// </summary>
        Syncpoint = 1 << 0,

        /// <summary>
        /// Present if the sync should signal as soon as possible.
        /// </summary>
        Strict = 1 << 1,

        /// <summary>
        /// Present will force the sync to be created, even if no actions are eligible.
        /// </summary>
        Force = 1 << 2,

        StrictSyncpoint = Strict | Syncpoint,
    }
}
