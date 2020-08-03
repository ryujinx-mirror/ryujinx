using Ryujinx.Common.Logging;
using System;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Synchronization
{
    /// <summary>
    /// GPU synchronization manager.
    /// </summary>
    public class SynchronizationManager
    {
        /// <summary>
        /// The maximum number of syncpoints supported by the GM20B.
        /// </summary>
        public const int MaxHardwareSyncpoints = 192;

        /// <summary>
        /// Array containing all hardware syncpoints.
        /// </summary>
        private Syncpoint[] _syncpoints;

        public SynchronizationManager()
        {
            _syncpoints = new Syncpoint[MaxHardwareSyncpoints];

            for (uint i = 0; i < _syncpoints.Length; i++)
            {
                _syncpoints[i] = new Syncpoint(i);
            }
        }

        /// <summary>
        /// Increment the value of a syncpoint with a given id.
        /// </summary>
        /// <param name="id">The id of the syncpoint</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when id >= MaxHardwareSyncpoints</exception>
        /// <returns>The incremented value of the syncpoint</returns>
        public uint IncrementSyncpoint(uint id)
        {
            if (id >= MaxHardwareSyncpoints)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            return _syncpoints[id].Increment();
        }

        /// <summary>
        /// Get the value of a syncpoint with a given id.
        /// </summary>
        /// <param name="id">The id of the syncpoint</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when id >= MaxHardwareSyncpoints</exception>
        /// <returns>The value of the syncpoint</returns>
        public uint GetSyncpointValue(uint id)
        {
            if (id >= MaxHardwareSyncpoints)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            return _syncpoints[id].Value;
        }

        /// <summary>
        /// Register a new callback on a syncpoint with a given id at a target threshold.
        /// The callback will be called once the threshold is reached and will automatically be unregistered.
        /// </summary>
        /// <param name="id">The id of the syncpoint</param>
        /// <param name="threshold">The target threshold</param>
        /// <param name="callback">The callback to call when the threshold is reached</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when id >= MaxHardwareSyncpoints</exception>
        /// <returns>The created SyncpointWaiterHandle object or null if already past threshold</returns>
        public SyncpointWaiterHandle RegisterCallbackOnSyncpoint(uint id, uint threshold, Action callback)
        {
            if (id >= MaxHardwareSyncpoints)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            return _syncpoints[id].RegisterCallback(threshold, callback);
        }

        /// <summary>
        /// Unregister a callback on a given syncpoint.
        /// </summary>
        /// <param name="id">The id of the syncpoint</param>
        /// <param name="waiterInformation">The waiter information to unregister</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when id >= MaxHardwareSyncpoints</exception>
        public void UnregisterCallback(uint id, SyncpointWaiterHandle waiterInformation)
        {
            if (id >= MaxHardwareSyncpoints)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            _syncpoints[id].UnregisterCallback(waiterInformation);
        }

        /// <summary>
        /// Wait on a syncpoint with a given id at a target threshold.
        /// The callback will be called once the threshold is reached and will automatically be unregistered.
        /// </summary>
        /// <param name="id">The id of the syncpoint</param>
        /// <param name="threshold">The target threshold</param>
        /// <param name="timeout">The timeout</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when id >= MaxHardwareSyncpoints</exception>
        /// <returns>True if timed out</returns>
        public bool WaitOnSyncpoint(uint id, uint threshold, TimeSpan timeout)
        {
            if (id >= MaxHardwareSyncpoints)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            // TODO: Remove this when GPU channel scheduling will be implemented.
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                timeout = TimeSpan.FromSeconds(1);
            }

            using (ManualResetEvent waitEvent = new ManualResetEvent(false))
            {
                var info = _syncpoints[id].RegisterCallback(threshold, () => waitEvent.Set());

                if (info == null)
                {
                    return false;
                }

                bool signaled = waitEvent.WaitOne(timeout);

                if (!signaled && info != null)
                {
                    Logger.Error?.Print(LogClass.Gpu, $"Wait on syncpoint {id} for threshold {threshold} took more than {timeout.TotalMilliseconds}ms, resuming execution...");

                    _syncpoints[id].UnregisterCallback(info);
                }

                return !signaled;
            }
        }
    }
}
