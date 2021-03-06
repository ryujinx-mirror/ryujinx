using Ryujinx.Memory.Range;
using System.Collections.Generic;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// A region of virtual memory.
    /// </summary>
    class VirtualRegion : AbstractRegion
    {
        public List<RegionHandle> Handles = new List<RegionHandle>();

        private readonly MemoryTracking _tracking;

        public VirtualRegion(MemoryTracking tracking, ulong address, ulong size) : base(address, size)
        {
            _tracking = tracking;
        }

        public override void Signal(ulong address, ulong size, bool write)
        {
            foreach (var handle in Handles)
            {
                handle.Signal(address, size, write);
            }

            UpdateProtection();
        }

        /// <summary>
        /// Signal that this region has been mapped or unmapped.
        /// </summary>
        /// <param name="mapped">True if the region has been mapped, false if unmapped</param>
        public void SignalMappingChanged(bool mapped)
        {
            foreach (RegionHandle handle in Handles)
            {
                handle.SignalMappingChanged(mapped);
            }
        }

        /// <summary>
        /// Gets the strictest permission that the child handles demand. Assumes that the tracking lock has been obtained.
        /// </summary>
        /// <returns>Protection level that this region demands</returns>
        public MemoryPermission GetRequiredPermission()
        {
            // Start with Read/Write, each handle can strip off permissions as necessary.
            // Assumes the tracking lock has already been obtained.

            MemoryPermission result = MemoryPermission.ReadAndWrite;

            foreach (var handle in Handles)
            {
                result &= handle.RequiredPermission;
                if (result == 0) return result;
            }
            return result;
        }

        /// <summary>
        /// Updates the protection for this virtual region.
        /// </summary>
        public void UpdateProtection()
        {
            _tracking.ProtectVirtualRegion(this, GetRequiredPermission());
        }

        /// <summary>
        /// Removes a handle from this virtual region. If there are no handles left, this virtual region is removed.
        /// </summary>
        /// <param name="handle">Handle to remove</param>
        public void RemoveHandle(RegionHandle handle)
        {
            lock (_tracking.TrackingLock)
            {
                Handles.Remove(handle);
                UpdateProtection();
                if (Handles.Count == 0)
                {
                    _tracking.RemoveVirtual(this);
                }
            }
        }

        public override INonOverlappingRange Split(ulong splitAddress)
        {
            VirtualRegion newRegion = new VirtualRegion(_tracking, splitAddress, EndAddress - splitAddress);
            Size = splitAddress - Address;

            // The new region inherits all of our parents.
            newRegion.Handles = new List<RegionHandle>(Handles);
            foreach (var parent in Handles)
            {
                parent.AddChild(newRegion);
            }

            return newRegion;
        }
    }
}
