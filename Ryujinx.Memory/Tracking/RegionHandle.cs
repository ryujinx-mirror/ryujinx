using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// A tracking handle for a given region of virtual memory. The Dirty flag is updated whenever any changes are made,
    /// and an action can be performed when the region is read to or written from.
    /// </summary>
    public class RegionHandle : IRegionHandle, IRange
    {
        public bool Dirty { get; private set; }
        public bool Unmapped { get; private set; }

        public ulong Address { get; }
        public ulong Size { get; }
        public ulong EndAddress { get; }

        internal IMultiRegionHandle Parent { get; set; }
        internal int SequenceNumber { get; set; }

        private event Action _onDirty;

        private RegionSignal _preAction; // Action to perform before a read or write. This will block the memory access.
        private readonly List<VirtualRegion> _regions;
        private readonly MemoryTracking _tracking;
        private bool _disposed;

        internal MemoryPermission RequiredPermission => _preAction != null ? MemoryPermission.None : (Dirty ? MemoryPermission.ReadAndWrite : MemoryPermission.Read);
        internal RegionSignal PreAction => _preAction;

        /// <summary>
        /// Create a new region handle. The handle is registered with the given tracking object,
        /// and will be notified of any changes to the specified region.
        /// </summary>
        /// <param name="tracking">Tracking object for the target memory block</param>
        /// <param name="address">Virtual address of the region to track</param>
        /// <param name="size">Size of the region to track</param>
        /// <param name="mapped">True if the region handle starts mapped</param>
        internal RegionHandle(MemoryTracking tracking, ulong address, ulong size, bool mapped = true)
        {
            Dirty = mapped;
            Unmapped = !mapped;
            Address = address;
            Size = size;
            EndAddress = address + size;

            _tracking = tracking;
            _regions = tracking.GetVirtualRegionsForHandle(address, size);
            foreach (var region in _regions)
            {
                region.Handles.Add(this);
            }
        }

        /// <summary>
        /// Signal that a memory action occurred within this handle's virtual regions.
        /// </summary>
        /// <param name="write">Whether the region was written to or read</param>
        internal void Signal(ulong address, ulong size, bool write)
        {
            RegionSignal action = Interlocked.Exchange(ref _preAction, null);
            action?.Invoke(address, size);

            if (write)
            {
                bool oldDirty = Dirty;
                Dirty = true;
                if (!oldDirty)
                {
                    _onDirty?.Invoke();
                }
                Parent?.SignalWrite();
            }
        }

        /// <summary>
        /// Consume the dirty flag for this handle, and reprotect so it can be set on the next write.
        /// </summary>
        public void Reprotect(bool asDirty = false)
        {
            Dirty = asDirty;
            lock (_tracking.TrackingLock)
            {
                foreach (VirtualRegion region in _regions)
                {
                    region.UpdateProtection();
                }
            }
        }

        /// <summary>
        /// Register an action to perform when the tracked region is read or written.
        /// The action is automatically removed after it runs.
        /// </summary>
        /// <param name="action">Action to call on read or write</param>
        public void RegisterAction(RegionSignal action)
        {
            RegionSignal lastAction = Interlocked.Exchange(ref _preAction, action);
            if (lastAction == null && action != lastAction)
            {
                lock (_tracking.TrackingLock)
                {
                    foreach (VirtualRegion region in _regions)
                    {
                        region.UpdateProtection();
                    }
                }
            }
        }

        /// <summary>
        /// Register an action to perform when the region is written to.
        /// This action will not be removed when it is called - it is called each time the dirty flag is set.
        /// </summary>
        /// <param name="action">Action to call on dirty</param>
        public void RegisterDirtyEvent(Action action)
        {
            _onDirty += action;
        }

        /// <summary>
        /// Add a child virtual region to this handle.
        /// </summary>
        /// <param name="region">Virtual region to add as a child</param>
        internal void AddChild(VirtualRegion region)
        {
            _regions.Add(region);
        }

        /// <summary>
        /// Signal that this handle has been mapped or unmapped.
        /// </summary>
        /// <param name="mapped">True if the handle has been mapped, false if unmapped</param>
        internal void SignalMappingChanged(bool mapped)
        {
            if (Unmapped == mapped)
            {
                Unmapped = !mapped;

                if (Unmapped)
                {
                    Dirty = false;
                }
            }
        }

        /// <summary>
        /// Check if this region overlaps with another.
        /// </summary>
        /// <param name="address">Base address</param>
        /// <param name="size">Size of the region</param>
        /// <returns>True if overlapping, false otherwise</returns>
        public bool OverlapsWith(ulong address, ulong size)
        {
            return Address < address + size && address < EndAddress;
        }

        /// <summary>
        /// Dispose the handle. Within the tracking lock, this removes references from virtual and physical regions.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            _disposed = true;

            lock (_tracking.TrackingLock)
            {
                foreach (VirtualRegion region in _regions)
                {
                    region.RemoveHandle(this);
                }
            }
        }
    }
}
