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
        private List<PhysicalRegion> _physicalChildren;

        private readonly MemoryTracking _tracking;

        public VirtualRegion(MemoryTracking tracking, ulong address, ulong size) : base(address, size)
        {
            _tracking = tracking;

            UpdatePhysicalChildren();
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
        /// Clears all physical children of this region. Assumes that the tracking lock has been obtained.
        /// </summary>
        private void ClearPhysicalChildren()
        {
            if (_physicalChildren != null)
            {
                foreach (PhysicalRegion child in _physicalChildren)
                {
                    child.RemoveParent(this);
                }
            }
        }

        /// <summary>
        /// Updates the physical children of this region, assuming that they are clear and that the tracking lock has been obtained.
        /// </summary>
        private void UpdatePhysicalChildren()
        {
            _physicalChildren = _tracking.GetPhysicalRegionsForVirtual(Address, Size);

            foreach (PhysicalRegion child in _physicalChildren)
            {
                child.VirtualParents.Add(this);
            }
        }

        /// <summary>
        /// Recalculates the physical children for this virtual region. Assumes that the tracking lock has been obtained.
        /// </summary>
        public void RecalculatePhysicalChildren()
        {
            ClearPhysicalChildren();
            UpdatePhysicalChildren();
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
        /// Updates the protection for this virtual region, and all child physical regions.
        /// </summary>
        public void UpdateProtection()
        {
            // Re-evaluate protection for all physical children.

            _tracking.ProtectVirtualRegion(this, GetRequiredPermission());
            lock (_tracking.TrackingLock)
            {
                foreach (var child in _physicalChildren)
                {
                    child.UpdateProtection();
                }
            }
        }

        /// <summary>
        /// Removes a handle from this virtual region. If there are no handles left, this virtual region is removed.
        /// </summary>
        /// <param name="handle">Handle to remove</param>
        public void RemoveHandle(RegionHandle handle)
        {
            bool removedRegions = false;
            lock (_tracking.TrackingLock)
            {
                Handles.Remove(handle);
                UpdateProtection();
                if (Handles.Count == 0)
                {
                    _tracking.RemoveVirtual(this);
                    foreach (var child in _physicalChildren)
                    {
                        removedRegions |= child.RemoveParent(this);
                    }
                }
            }

            if (removedRegions)
            {
                // The first lock will unprotect any regions that have been removed. This second lock will remove them.
                lock (_tracking.TrackingLock)
                {
                    foreach (var child in _physicalChildren)
                    {
                        child.TryDelete();
                    }
                }
            }
        }

        /// <summary>
        /// Add a child physical region to this virtual region. Assumes that the tracking lock has been obtained.
        /// </summary>
        /// <param name="region">Physical region to add as a child</param>
        public void AddChild(PhysicalRegion region)
        {
            _physicalChildren.Add(region);
        }

        public override INonOverlappingRange Split(ulong splitAddress)
        {
            ClearPhysicalChildren();
            VirtualRegion newRegion = new VirtualRegion(_tracking, splitAddress, EndAddress - splitAddress);
            Size = splitAddress - Address;
            UpdatePhysicalChildren();

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
